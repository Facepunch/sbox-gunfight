global using Sandbox;
global using SandboxEditor;
global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Threading.Tasks;

namespace Facepunch.Gunfight;

partial class GunfightGame : Game
{
	public static new GunfightGame Current => Game.Current as GunfightGame;

	[Net] public GunfightHud Hud { get; set; }
	[Net] public TeamScores Scores { get; set; }

	public GunfightGame()
	{
		//
		// Create the HUD entity. This is always broadcast to all clients
		// and will create the UI panels clientside.
		//
		if ( IsServer )
		{
			Hud = new GunfightHud();
			Scores = new();

			Global.TickRate = 30;
		}
	}

	[Event.Entity.PostSpawn]
	public void PostEntitySpawn()
	{
		// Try to set up the active gamemode
		GamemodeSystem.SetupGamemode();
	}

	protected Player CreatePawn( Client cl )
	{
		cl.Pawn?.Delete();

		var gamemode = GamemodeSystem.Current;
		GunfightPlayer player;

		if ( gamemode.IsValid() )
			player = gamemode.GetPawn( cl );
		else
			player = new GunfightPlayer();

		player.UpdateClothes( cl );
		cl.Pawn = player;

		return player;
	}

	public override void ClientJoined( Client cl )
	{
		base.ClientJoined( cl );

		var player = CreatePawn( cl );

		// Inform the active gamemode
		GamemodeSystem.Current?.OnClientJoined( cl );

		player.Respawn();
	}

	public override void ClientDisconnect( Client cl, NetworkDisconnectionReason reason )
	{
		base.ClientDisconnect( cl, reason );

		// Inform the active gamemode
		GamemodeSystem.Current?.OnClientLeft( cl, reason );
	}

	public override void MoveToSpawnpoint( Entity entity )
	{
		var player = entity as GunfightPlayer;
		var gamemode = GamemodeSystem.Current;

		var gamemodeTransform = gamemode?.GetSpawn( player );
		if ( gamemodeTransform is not null )
		{
			player.Transform = gamemodeTransform.Value;
			return;
		}

		gamemode?.PreSpawn( player );

		var query = Entity.All.OfType<SpawnPoint>();
		if ( player.SpawnPointTag != null )
			query = query.Where( x => x.Tags.Has( player.SpawnPointTag ) );

		var spawnpoint = query.OrderByDescending( x => SpawnpointWeight( player, x ) ).FirstOrDefault();

		if ( spawnpoint == null )
		{
			Log.Warning( $"Couldn't find spawnpoint for {player}!" );
			return;
		}

		player.Transform = spawnpoint.Transform;

		if ( entity is GunfightPlayer pl )
		{
			pl.SetViewAngles( entity.Rotation.Angles() );
		}
	}

	/// <summary>
	/// The higher the better
	/// </summary>
	public float SpawnpointWeight( Entity pawn, Entity spawnpoint )
	{
		// We want to find the closest player (worst weight)
		float distance = float.MaxValue;

		foreach ( var client in Client.All )
		{
			if ( client.Pawn == null ) continue;
			if ( client.Pawn == pawn ) continue;
			if ( client.Pawn.LifeState != LifeState.Alive ) continue;

			var spawnDist = (spawnpoint.Position - client.Pawn.Position).Length;
			distance = MathF.Min( distance, spawnDist );
		}

		//Log.Info( $"{spawnpoint} is {distance} away from any player" );

		return distance;
	}

	public override void PostCameraSetup( ref CameraSetup camSetup )
	{
		base.PostCameraSetup( ref camSetup );

		CameraModifier.Apply( ref camSetup );

		camSetup.ZNear = 5f;
	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		// Simulate active gamemode
		GamemodeSystem.Current?.Simulate( cl );
	}

	Color RedColor = new Color( 1f, 0.2f, 0.2f, 1f );
	public override void FrameSimulate( Client cl )
	{
		base.FrameSimulate( cl );

		var postProcess = Map.Camera.FindOrCreateHook<Sandbox.Effects.ScreenEffects>();

		//postProcess.Sharpen = 0.1f;
		postProcess.Vignette.Intensity = 0.5f;
		//postProcess.Vignette.Roundness = 1.5f;
		//postProcess.Vignette.Smoothness = 0.5f;
		//postProcess.Vignette.Color = Color.Black.WithAlpha( 0.2f );

		Audio.SetEffect( "core.player.death.muffle1", 0 );

		if ( GunfightCamera.Target is GunfightPlayer localPlayer )
		{
			var timeSinceDamage = localPlayer.TimeSinceDamage.Relative;
			var damageUi = timeSinceDamage.LerpInverse( 0.25f, 0.0f, true ) * 0.3f;
			if ( damageUi > 0 )
			{
				//postProcess.Saturation -= damageUi;

				//postProcess.Vignette.Color = Color.Lerp( postProcess.Vignette.Color, RedColor, damageUi );
				//postProcess.Vignette.Intensity += damageUi;
				//postProcess.Vignette.Smoothness += damageUi;
				//postProcess.Vignette.Roundness += damageUi;

				//postProcess.MotionBlur.Scale = damageUi * 0.5f;
			}

			var healthDelta = localPlayer.Health.LerpInverse( 0, 100.0f, true );

			healthDelta = MathF.Pow( healthDelta, 2f );

			//postProcess.Vignette.Color = Color.Lerp( postProcess.Vignette.Color, RedColor, 1 - healthDelta );
			//postProcess.Vignette.Intensity += (1 - healthDelta) * 0.1f;
			//postProcess.Vignette.Smoothness += (1 - healthDelta);
			//postProcess.Vignette.Roundness += (1 - healthDelta) * 0.1f;
			//postProcess.Saturation = MathF.Pow( healthDelta, 0.2f );
			//postProcess.FilmGrain.Intensity += (1 - healthDelta) * 0.1f;

			Audio.SetEffect( "core.player.death.muffle1", 1 - healthDelta, velocity: 2.0f );
		}

		// Let the gamemode control post process
		GamemodeSystem.Current?.PostProcessTick();

		// Simulate active gamemode
		GamemodeSystem.Current?.FrameSimulate( cl );
	}

	public static void Explosion( Entity weapon, Entity owner, Vector3 position, float radius, float damage, float forceScale )
	{
		// Effects
		Sound.FromWorld( "rust_pumpshotgun.shootdouble", position );
		Particles.Create( "particles/explosion/barrel_explosion/explosion_barrel.vpcf", position );

		// Damage, etc
		var overlaps = Entity.FindInSphere( position, radius );

		foreach ( var overlap in overlaps )
		{
			if ( overlap is not ModelEntity ent || !ent.IsValid() )
				continue;

			if ( ent.LifeState != LifeState.Alive )
				continue;

			if ( !ent.PhysicsBody.IsValid() )
				continue;

			if ( ent.IsWorld )
				continue;

			var targetPos = ent.PhysicsBody.MassCenter;

			var dist = Vector3.DistanceBetween( position, targetPos );
			if ( dist > radius )
				continue;

			var tr = Trace.Ray( position, targetPos )
				.Ignore( weapon )
				.WorldOnly()
				.Run();

			if ( tr.Fraction < 0.98f )
				continue;

			var distanceMul = 1.0f - Math.Clamp( dist / radius, 0.0f, 1.0f );
			var dmg = damage * distanceMul;
			var force = (forceScale * distanceMul) * ent.PhysicsBody.Mass;
			var forceDir = (targetPos - position).Normal;

			var damageInfo = DamageInfo.Explosion( position, forceDir * force, dmg )
				.WithWeapon( weapon )
				.WithAttacker( owner );

			ent.TakeDamage( damageInfo );
		}
	}

	[ClientRpc]
	public override void OnKilledMessage( long leftid, string left, long rightid, string right, string method )
	{
		KillFeed.Current?.AddEntry( leftid, left, rightid, right, method );
	}

	public override void RenderHud()
	{
		var player = GunfightCamera.Target;
		if ( !player.IsValid() ) return;

		player.RenderHud( Screen.Size );
	}

	/// <summary>
	/// An entity, which is a pawn, and has a client, has been killed.
	/// </summary>
	public override void OnKilled( Client client, Entity pawn )
	{
		Host.AssertServer();

		Log.Info( $"{client.Name} was killed." );

		if ( pawn.LastAttacker != null )
		{
			if ( pawn.LastAttacker.Client != null )
			{
				var wep = pawn.LastAttackerWeapon as GunfightWeapon;
				if ( wep != null )
				{
					OnKilledMessage( pawn.LastAttacker.Client.PlayerId, pawn.LastAttacker.Client.Name, client.PlayerId, client.Name, wep.WeaponDefinition.WeaponShortName );
				}
				else
				{
					OnKilledMessage( pawn.LastAttacker.Client.PlayerId, pawn.LastAttacker.Client.Name, client.PlayerId, client.Name, pawn.LastAttackerWeapon?.ClassName );

				}
			}
			else
			{
				OnKilledMessage( pawn.LastAttacker.NetworkIdent, pawn.LastAttacker.ToString(), client.PlayerId, client.Name, "killed" );
			}
		}
		else
		{
			OnKilledMessage( 0, "", client.PlayerId, client.Name, "died" );
		}
	}

	public override void BuildInput( InputBuilder input )
	{
		base.BuildInput( input );
		
		if ( input.StopProcessing ) return;

		GamemodeSystem.Current?.BuildInput( input );
	}
}

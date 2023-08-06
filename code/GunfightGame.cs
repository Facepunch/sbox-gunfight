global using Sandbox;
global using Editor;
global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Threading.Tasks;

namespace Facepunch.Gunfight;

partial class GunfightGame : GameManager
{
	public static new GunfightGame Current => GameManager.Current as GunfightGame;

	[Net] public GunfightHud Hud { get; set; }
	[Net] public TeamScores Scores { get; set; }

	public GunfightGame()
	{
		//
		// Create the HUD entity. This is always broadcast to all clients
		// and will create the UI panels clientside.
		//
		if ( Game.IsServer )
		{
			Hud = new GunfightHud();
			_ = new LoadoutSystem();
			Scores = new();
		}
	}

	[GameEvent.Entity.PostSpawn]
	public void PostEntitySpawn()
	{
		// Try to set up the active gamemode
		GamemodeSystem.SetupGamemode();
	}

	public GunfightPlayer CreatePawn( IClient cl )
	{
		cl.Pawn?.Delete();

		var gamemode = GamemodeSystem.Current;
		GunfightPlayer player = null;

		if ( gamemode.IsValid() )
			player = gamemode.GetPawn( cl );
		else
			player = new();

		player.UpdateClothes( cl );
		cl.Pawn = player;

		return player;
	}

	public override void ClientJoined( IClient cl )
	{
		base.ClientJoined( cl );

		var player = CreatePawn( cl );

		// Inform the active gamemode
		GamemodeSystem.Current?.OnClientJoined( cl );

		player.Respawn();
	}

	public override void ClientDisconnect( IClient cl, NetworkDisconnectionReason reason )
	{
		base.ClientDisconnect( cl, reason );

		// Inform the active gamemode
		GamemodeSystem.Current?.OnClientLeft( cl, reason );
	}

	public override void OnVoicePlayed( IClient cl )
	{
		base.OnVoicePlayed( cl );

		GunVoiceList.Current.OnVoicePlayed( cl.SteamId, cl.Voice.CurrentLevel );
	}

	public override void MoveToSpawnpoint( Entity entity )
	{
		var player = entity as GunfightPlayer;
		var gamemode = GamemodeSystem.Current;

		gamemode?.PreSpawn( player );

		var transform = gamemode?.GetDefaultSpawnPoint( player );
		if ( transform is null )
		{
			// Look through legacy spawn points, as a last ditch effort
			transform = Entity.All.OfType<SpawnPoint>()
				.FirstOrDefault( x => player.SpawnPointTag != null && x.Tags.Has( player.SpawnPointTag ) )
				?.Transform;
		}

		// Did we fuck up?
		if ( transform is null )
		{
			Log.Warning( $"Couldn't find spawnpoint for {player}" );
			return;
		}

		player.Transform = transform.Value;
	}

	protected float FovOffset { get; set; } = 0f;
	public static float AddedCameraFOV { get; set; } = 0f;

	[GameEvent.Client.PostCamera]
	public void PostCameraSetup()
	{
		if ( Game.LocalPawn.LifeState != LifeState.Alive )
		{
			return;
		}
		
		FovOffset = FovOffset.LerpTo( AddedCameraFOV, Time.Delta * 10f, true );
		Camera.FieldOfView += FovOffset;

		CameraModifier.Apply();
		Camera.ZNear = 5f;
	}

	public override void Simulate( IClient cl )
	{
		base.Simulate( cl );

		// Simulate active gamemode
		GamemodeSystem.Current?.Simulate( cl );
	}

	Color RedColor => new Color( 0.1f, 0f, 0f, 0.1f );

	// TODO - Delete this
	TimeUntil ActivateHack = 2f;
	public override void FrameSimulate( IClient cl )
	{
		base.FrameSimulate( cl );

		if ( GunfightCamera.Target is GunfightPlayer localPlayer && ActivateHack )
		{
			var postProcess = Camera.Main.FindOrCreateHook<Sandbox.Effects.ScreenEffects>();
			postProcess.Sharpen = 0.1f;
			postProcess.Vignette.Intensity = 0.5f;
			postProcess.Vignette.Roundness = 1.5f;
			postProcess.Vignette.Smoothness = 0.5f;
			postProcess.Vignette.Color = Color.Black.WithAlpha( 0.2f );
			postProcess.MotionBlur.Scale = 0f;
			postProcess.Saturation = 0;

			var timeSinceDamage = localPlayer.TimeSinceDamage.Relative;
			var damageUi = MathF.Pow( timeSinceDamage.LerpInverse( 0.8f, 0.0f, true ), 0.3f );
			var shortDamageUi = timeSinceDamage.LerpInverse( 0.2f, 0.0f, true );
			var hp = localPlayer.Health;

			if ( localPlayer.LifeState == LifeState.Respawnable )
			{
				hp = 100;
				damageUi = 0;
				shortDamageUi = 0;
			}

			if ( damageUi > 0 )
			{
				postProcess.Vignette.Color = Color.Lerp( postProcess.Vignette.Color, RedColor, damageUi );
				postProcess.Vignette.Intensity += damageUi * 0.05f;
				postProcess.Vignette.Smoothness += damageUi * 0.1f;
			}

			postProcess.ChromaticAberration.Scale = shortDamageUi * 1f;
			postProcess.Saturation -= shortDamageUi * 0.1f;
			postProcess.Pixelation = shortDamageUi * 0.05f;

			var healthDelta = hp.LerpInverse( 0, localPlayer.MaxHealth, true );
			healthDelta = MathF.Pow( healthDelta, 0.6f );

			postProcess.Vignette.Color = Color.Lerp( postProcess.Vignette.Color, RedColor, 1 - healthDelta );
			postProcess.Saturation += healthDelta;
			postProcess.ChromaticAberration.Scale += MathF.Pow( 1f - healthDelta, 3f ) * MathF.Sin( Time.Now * 1f );

			Audio.SetEffect( "core.player.death.muffle1", 1 - healthDelta, velocity: 2.0f );

			// Let the gamemode control post process
			GamemodeSystem.Current?.PostProcessTick();
		}

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
				.StaticOnly()
				.Run();

			if ( tr.Fraction < 0.98f )
				continue;

			var distanceMul = 1.0f - Math.Clamp( dist / radius, 0.0f, 1.0f );
			var dmg = damage * distanceMul;
			var force = (forceScale * distanceMul) * ent.PhysicsBody.Mass;
			var forceDir = (targetPos - position).Normal;

			var damageInfo = DamageInfo.FromExplosion( position, forceDir * force, dmg )
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

	/// <summary>
	/// An entity, which is a pawn, and has a client, has been killed.
	/// </summary>
	public override void OnKilled( IClient client, Entity pawn )
	{
		Game.AssertServer();

		Log.Info( $"{client.Name} was killed." );

		if ( pawn.LastAttacker != null )
		{
			if ( pawn.LastAttacker.Client != null )
			{
				GunfightHud.ShowDeathInformation( To.Single( client ), pawn.LastAttacker.Client );
				
				var wep = pawn.LastAttackerWeapon as GunfightWeapon;
				if ( wep != null )
				{
					OnKilledMessage( pawn.LastAttacker.Client.SteamId, pawn.LastAttacker.Client.Name, client.SteamId, client.Name, wep.WeaponDefinition.WeaponShortName );
				}
				else
				{
					OnKilledMessage( pawn.LastAttacker.Client.SteamId, pawn.LastAttacker.Client.Name, client.SteamId, client.Name, pawn.LastAttackerWeapon?.ClassName );

				}
			}
			else
			{
				OnKilledMessage( pawn.LastAttacker.NetworkIdent, pawn.LastAttacker.ToString(), client.SteamId, client.Name, "killed" );
			}
		}
		else
		{
			OnKilledMessage( 0, "", client.SteamId, client.Name, "died" );

			GunfightHud.ShowDeathInformation( To.Single( client ), client );
		}
	}

	public override void BuildInput()
	{
		base.BuildInput();
		
		GamemodeSystem.Current?.BuildInput();
	}
}

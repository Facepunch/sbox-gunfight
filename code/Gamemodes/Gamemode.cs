using Sandbox.UI;

namespace Facepunch.Gunfight;

public abstract partial class Gamemode : Entity
{
	/// <summary>
	/// A quick accessor to get how many people are in the game
	/// </summary>
	[Net] public int PlayerCount { get; private set; }

	/// <summary>
	/// Can specify a panel to be created when the gamemode is made.
	/// </summary>
	/// <returns></returns>
	public virtual Panel HudPanel => null;

	/// <summary>
	/// Gamemodes can define what pawn to create
	/// </summary>
	/// <param name="cl"></param>
	/// <returns></returns>
	public virtual GunfightPlayer GetPawn( IClient cl ) => new GunfightPlayer();

	/// <summary>
	/// Lets gamemodes define what teams are available in a gamemode
	/// </summary>
	public virtual List<Team> TeamSetup => new() { Team.Unassigned };

	/// <summary>
	/// Accessor for score system
	/// </summary>
	public TeamScores Scores => GunfightGame.Current.Scores;

	public virtual bool AllowSpectating => false;

	public virtual bool NoFlags => false;

	/// <summary>
	/// Decides whether or not players can move
	/// </summary>
	/// <returns></returns>
	public virtual bool AllowMovement => true;

	/// <summary>
	/// Decides whether or not players can respawn
	/// </summary>
	/// <returns></returns>
	public virtual bool AllowRespawning => true;

	/// <summary>
	/// Decides whether or not players can take damage
	/// </summary>
	/// <returns></returns>
	public virtual bool AllowDamage => true;

	[ConVar.Server( "gunfight_friendly_fire_override" )]
	public static bool FriendlyFireOverride { get; set; } = false;

	[ConVar.Replicated( "gunfight_thirdperson" )]
	public static bool ThirdPersonConVar { get; set; } = false;

	// Stats
	[ConVar.Server( "gunfight_minimum_players" )]
	protected static int MinimumPlayersConVar { get; set; } = 2;

	/// <summary>
	/// How many players should be in the game before it starts?
	/// </summary>
	public virtual int MinimumPlayers => MinimumPlayersConVar;

	/// <summary>
	/// Should third person be enabled? By default, it's controlled by a game ConVar.
	/// </summary>
	public virtual bool AllowThirdPerson => ThirdPersonConVar;

	/// <summary>
	/// Should you be able to shoot teammates?
	/// </summary>
	public virtual bool AllowFriendlyFire => true;

	/// <summary>
	/// Are capture points allowed to be used as spawn points?
	/// </summary>
	public virtual bool CapturePointsAreSpawnPoints => false;

	/// <summary>
	/// What's the max score for this game? Can be unused.
	/// </summary>
	public virtual int MaximumScore => 4;

	protected GunfightPlayer LastKilledPlayer;

	public virtual string GetTimeLeftLabel()
	{
		return "00:00";
	}

	public virtual string GetGameStateLabel()
	{
		return "N/A";
	}

	public virtual void CreatePawn( IClient cl )
	{
		cl.Pawn?.Delete();
		var pawn = GetPawn( cl );
		cl.Pawn = pawn;
		pawn.Respawn();
	}

	public override void Spawn()
	{
		base.Spawn();
		Transmit = TransmitType.Always;
	}

	/// <summary>
	/// Called when a client joins the game
	/// </summary>
	/// <param name="cl"></param>
	public virtual void OnClientJoined( IClient cl )
	{
		PlayerCount++;
		AssignTeam( cl );
	}
	
	public virtual void AssignTeam( IClient cl )
	{
	}

	public virtual void Initialize()
	{
		if ( NoFlags )
		{
			// Delete capture points
			foreach ( var ent in Entity.All.OfType<CapturePointEntity>() )
			{
				ent.Delete();
			}
		}
		
	}

	/// <summary>
	/// Called when a client leaves the game
	/// </summary>
	/// <param name="cl"></param>
	/// <param name="reason"></param>
	public virtual void OnClientLeft( IClient cl, NetworkDisconnectionReason reason )
	{
		PlayerCount--;
	}

	/// <summary>
	/// Used to apply a loadout to a player
	/// </summary>
	/// <param name="player"></param>
	/// <returns></returns>
	public virtual bool PlayerLoadout( GunfightPlayer player )
	{
		return false;
	}

	protected static Loadout GetRandomLoadout( bool unique = true )
	{
		var loadouts = Loadout.WithTag( "gunfight" ).ToList();
		if ( unique ) loadouts = loadouts/*.Where( x => x != LoadoutSystem.MatchLoadout )*/.ToList();

		if ( loadouts.Count < 1 ) return null;

		var index = Game.Random.Int( 1, loadouts.Count() ) - 1;
		var loadout = loadouts[index];

		return loadout;
	}

	protected void RandomizeLoadout( bool unique = true )
	{
		LoadoutSystem.MatchLoadout = GetRandomLoadout( unique );
	}

	/// <summary>
	/// Called when the score changes for any team
	/// </summary>
	/// <param name="team"></param>
	/// <param name="score"></param>
	/// <param name="maxReached"></param>
	public virtual void OnScoreChanged( Team team, int score, bool maxReached = false )
	{
	}

	/// <summary>
	/// Called when a player dies.
	/// Gamemodes can define the life state a player will move to upon death. 
	/// <see cref="LifeState.Respawning"/> is the default behavior, which will automatically respawn the player in a few seconds. See <see cref="GunfightPlayer.Simulate(Client)"/>
	/// <see cref="LifeState.Dead"/> means the gamemode has to respawn the player.
	/// </summary>
	/// <param name="player"></param>
	/// <param name="damageInfo"></param>
	/// <param name="lifeState"></param>
	public virtual void OnPlayerKilled( GunfightPlayer player, DamageInfo damageInfo, out LifeState lifeState )
	{
		lifeState = LifeState.Respawning;
	}

	public virtual void PostPlayerKilled( GunfightPlayer player, DamageInfo lastDamage )
	{
		LastKilledPlayer = player;

		// Payback
		if ( lastDamage.Attacker is GunfightPlayer attacker )
		{
			if ( attacker.LastKiller == player )
			{
				Progression.GiveAward( attacker.Client, "Payback" );
				attacker.LastKiller = null;
			}
		}

		// Let the player know who killed them last
		player.LastKiller = lastDamage.Attacker;
	}

	public float GetSpawnpointWeight( GunfightPlayer pawn, Entity spawnpoint )
	{
		// We want to find the closest player (worst weight)
		float distance = float.MaxValue;

		foreach ( var client in Game.Clients )
		{
			var player = client.Pawn as GunfightPlayer;
			if ( player == null ) continue;
			if ( player == pawn ) continue;
			if ( player.LifeState != LifeState.Alive ) continue;

			var spawnDist = (spawnpoint.Position - client.Pawn.Position).Length;
			distance = MathF.Min( distance, spawnDist );
		}

		//Log.Info( $"{spawnpoint} is {distance} away from any player" );

		return distance;
	}

	public IEnumerable<ISpawnPoint> GetValidSpawnPoints( GunfightPlayer player )
	{
		return Entity.All.OfType<ISpawnPoint>()
			.Where( x => x.IsValidSpawn( player ) ).OrderByDescending( x => x.GetSpawnPriority() );
	}

	/// <summary>
	/// Allows gamemodes to override player spawn locations
	/// </summary>
	/// <param name="player"></param>
	/// <returns></returns>
	public virtual Transform? GetDefaultSpawnPoint( GunfightPlayer player )
	{
		// Default behavior
		var spawnPoints = GetValidSpawnPoints( player );
		if ( spawnPoints.Count() < 1 ) return null;

		Log.Info( $"{spawnPoints.Count()} valid spawn points found." );

		return Game.Random.FromList( spawnPoints.ToList() )?.GetSpawnTransform();
	}

	public virtual void PreSpawn( GunfightPlayer player )
	{
		//
	}

	public virtual void RespawnAllPlayers()
	{
		All.OfType<GunfightPlayer>().ToList().ForEach( x => x.Respawn() );
	}

	/// <summary>
	/// Called on Client Tick, allows gamemodes to define custom post processing
	/// </summary>
	public virtual void PostProcessTick()
	{
	}

	public virtual void CleanupMap()
	{
	}

	public override void BuildInput()
	{
		if ( !AllowMovement )
		{
			Input.AnalogMove = Vector3.Zero;
			Input.Clear( "Jump" );
			Input.Clear( "duck" );
			Input.Clear( "Attack1" );
			Input.StopProcessing = true;
		}
	}

	public virtual void ResetStats()
	{
		foreach( var client in Game.Clients )
		{
			client.SetInt( "frags", 0 );
			client.SetInt( "deaths", 0 );
			client.SetInt( "score", 0 );
			client.SetInt( "captures", 0 );
		}
	}

	public virtual void OnFlagCaptured( CapturePointEntity flag, Team team )
	{
		//
	}

	public virtual bool CanPlayerRegenerate( GunfightPlayer player )
	{
		return true;
	}

	[ClientRpc]
	public static void ShowWinningTeam( Team team = Team.Unassigned )
	{
		UI.WinningTeamDisplay.AddToHud( team );
	}

	[ConCmd.Admin( "gunfight_debug_ui_teamdisplay" )]
	public static void DebugShowTeamDisplay( bool win = true )
	{
		ShowWinningTeam( To.Everyone, win ? Team.BLUFOR : Team.OPFOR );
	}

	[Event( "gunfight.hudrender" )]
	protected void HudRender()
	{
		GunfightHud.HudState = HudVisibilityState.Visible;

		if ( UI.WinningTeamDisplay.Visible )
			GunfightHud.HudState = HudVisibilityState.Invisible;

		UpdateHudRenderState();
	}

	public virtual void UpdateHudRenderState()
	{
		Event.Run( "gunfight.hudrender.post" );
	}

	[GameEvent.Tick.Server]
	protected void EventServerTick()
	{
		TickServer();
	}

	protected virtual void TickServer()
	{
		//
	}
}

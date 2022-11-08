using Sandbox.UI;

namespace Facepunch.Gunfight;

public abstract partial class Gamemode : Entity
{
	/// <summary>
	/// A quick accessor to get how many people are in the game
	/// </summary>
	[Net] public int PlayerCount { get; private set; }

	[Net] public Loadout CurrentLoadout { get; protected set; }

	/// <summary>
	/// Can specify a panel to be created when the gamemode is made.
	/// </summary>
	/// <returns></returns>
	public virtual Panel GetHudPanel() => null;

	/// <summary>
	/// Gamemodes can define what pawn to create
	/// </summary>
	/// <param name="cl"></param>
	/// <returns></returns>
	public virtual GunfightPlayer GetPawn( Client cl ) => new GunfightPlayer();

	/// <summary>
	/// Lets gamemodes define what teams are available in a gamemode
	/// </summary>
	public virtual List<Team> TeamSetup => new() { Team.Unassigned };

	/// <summary>
	/// Accessor for score system
	/// </summary>
	public TeamScores Scores => GunfightGame.Current.Scores;

	public virtual int MaximumScore => 4;

	public virtual string GetTimeLeftLabel()
	{
		return "00:00";
	}

	public virtual string GetGameStateLabel()
	{
		return "N/A";
	}

	public virtual void CreatePawn( Client cl )
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
	public virtual void OnClientJoined( Client cl )
	{
		PlayerCount++;
		
		AssignTeam( cl );
	}
	
	public virtual void AssignTeam( Client cl )
	{
		// Do nothing for now	
	}

	public virtual void Initialize()
	{
	}

	public virtual bool AllowSpectating()
	{
		return !AllowRespawning();
	}

	/// <summary>
	/// Called when a client leaves the game
	/// </summary>
	/// <param name="cl"></param>
	/// <param name="reason"></param>
	public virtual void OnClientLeft( Client cl, NetworkDisconnectionReason reason )
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
		//
	}

	/// <summary>
	/// Allows gamemodes to override player spawn locations
	/// </summary>
	/// <param name="player"></param>
	/// <returns></returns>
	public virtual Transform? GetSpawn( GunfightPlayer player )
	{
		return null;
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
	/// Decides whether or not players can move
	/// </summary>
	/// <returns></returns>
	public virtual bool AllowMovement()
	{
		return true;
	}

	/// <summary>
	/// Decides whether or not players can respawn
	/// </summary>
	/// <returns></returns>
	public virtual bool AllowRespawning()
	{
		return true;
	}

	/// <summary>
	/// Decides whether or not players can take damage
	/// </summary>
	/// <returns></returns>
	public virtual bool AllowDamage()
	{
		return true;
	}

	[ConVar.Server( "gunfight_friendly_fire_override" )]
	public static bool FriendlyFireOverride { get; set; } = false;

	public virtual bool AllowFriendlyFire()
	{
		return true;
	}

	/// <summary>
	/// Called on Client Tick, allows gamemodes to define custom post processing
	/// </summary>
	/// <param name="postProcess"></param>
	public virtual void PostProcessTick()
	{
	}

	public virtual void CleanupMap()
	{
		//
	}

	public override void BuildInput( InputBuilder input )
	{
		if ( !AllowMovement() )
		{
			input.InputDirection = Vector3.Zero;
			input.ClearButton( InputButton.Jump );
			input.ClearButton( InputButton.Duck );
			input.ClearButton( InputButton.PrimaryAttack );

			input.StopProcessing = true;
		}
	}

	public virtual void ResetStats()
	{
		foreach( var client in Client.All )
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
		//
	}

	[Event.Tick.Server]
	protected void EventServerTick()
	{
		TickServer();
	}

	protected virtual void TickServer()
	{
		//
	}
}

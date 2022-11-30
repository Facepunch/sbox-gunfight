using Sandbox.Internal;
using Sandbox.UI;
using System.ComponentModel.DataAnnotations;

namespace Facepunch.Gunfight.War;

[Display( Name = "Conquest", Description = "All-out war against two large teams. First team to drain the other team's tickets wins." )]
public partial class WarGamemode : Gamemode
{
	[Net] public GameState State { get; set; } = GameState.WaitingForPlayers;
	[Net] public RealTimeSince TimeSinceStateChanged { get; protected set; }
	[Net] public RealTimeUntil TimeUntilNextState { get; protected set; }
	public TimeSpan TimeRemaining => TimeSpan.FromSeconds( TimeUntilNextState );
	public string FormattedTimeRemaining => TimeRemaining.ToString( @"mm\:ss" );
	[Net] public Team WinningTeam { get; set; } = Team.Unassigned;
	
	TimeSince LastTicket = 0;

	protected string CachedTimeRemaining { get; set; }
	public override void Spawn()
	{
		base.Spawn();

		LoadoutSystem.AllowCustomLoadouts = true;
	}

	public override void Initialize()
	{
		Scores.SetScore( Team.BLUFOR, MaximumScore );
		Scores.SetScore( Team.OPFOR, MaximumScore );
	}

	[ConVar.Server( "gunfight_gamemode_war_maxscore" )]
	private static int MaximumScoreConfig { get; set; } = 300;
	public override int MaximumScore => MaximumScoreConfig;
	public override List<Team> TeamSetup => new() { Team.BLUFOR, Team.OPFOR };
	public override bool AllowFriendlyFire => false;
	public override bool AllowRespawning => false;
	public override bool AllowSpectating => true;
	public override bool AllowMovement => State != GameState.Countdown;
	public override bool AllowDamage => State != GameState.Countdown;
	public override bool CapturePointsAreSpawnPoints => true;
	public override Panel HudPanel => new WarHud();

	public override bool CanPlayerRegenerate( GunfightPlayer player )
	{
		return true;
	}


	public override void AssignTeam( Client cl )
	{
		var teamComponent = cl.Components.GetOrCreate<TeamComponent>();
		teamComponent.Team = TeamSystem.GetLowestCount();

		UI.GunfightChatbox.AddChatEntry( To.Everyone, cl.Name, $"joined {teamComponent.Team.GetName()}", cl.PlayerId, false );

		VerifyEnoughPlayers();
	}

	public override bool PlayerLoadout( GunfightPlayer player )
	{
		RandomizeLoadout();
		LoadoutSystem.GetLoadout( player.Client )?.Give( player );
		GunfightStatusPanel.RpcUpdate( To.Everyone );

		return true;
	}

	public override void PostPlayerKilled( GunfightPlayer player, DamageInfo lastDamage )
	{
		base.PostPlayerKilled( player, lastDamage );

		var team = player.Team;
		if ( team != Team.Unassigned )
		{
			Scores.AddScore( team, -1 );
		}
	}

	public override string GetGameStateLabel()
	{
		return State switch
		{
			GameState.WaitingForPlayers => $"WAITING FOR PLAYER(S)",
			_ => base.GetGameStateLabel()
		};
	}

	public override string GetTimeLeftLabel()
	{
		return State switch
		{
			GameState.WaitingForPlayers => "WAITING",
			_ => FormattedTimeRemaining
		};
	}

	public override void OnFlagCaptured( CapturePointEntity flag, Team team )
	{
		UI.GunfightChatbox.AddInformation( To.Everyone, $"{team.GetName()} captured {flag.NiceName}" );
		War.CapturePoints.MarkCapture( To.Everyone, flag );
	}

	public void SetGameState( GameState newState )
	{
		var old = State;
		State = newState;

		Log.Info( $"Game State Changed to {newState}" );

		OnGameStateChanged( old, newState );
	}

	protected void VerifyEnoughPlayers()
	{
		if ( State == GameState.WaitingForPlayers )
		{
			if ( PlayerCount >= MinimumPlayers )
			{
				SetGameState( GameState.Countdown );
			}
		}
	}

	void ResetCapturePoints()
	{
		Entity.All.OfType<CapturePointEntity>().ToList().ForEach( x => x.Initialize() );
	}
	
	protected void OnGameStateChanged( GameState before, GameState after )
	{
		TimeSinceStateChanged = 0;

		if ( after == GameState.WaitingForPlayers )
		{
			WinningTeam = Team.Unassigned;
			CleanupMap();
			Initialize();
			ResetCapturePoints();
			ResetStats();
			VerifyEnoughPlayers();
			RespawnAllPlayers();
		}
		if ( after == GameState.Countdown )
		{
			var countdown = 10f;
			CleanupMap();
			Initialize();
			ResetCapturePoints();
			ResetStats();
			TimeUntilNextState = countdown;
			RespawnAllPlayers();
			Sound.FromScreen( To.Everyone, "sounds/ui/ten_seconds.sound" );

			UI.GamemodeIdentity.RpcShow( To.Everyone, countdown.CeilToInt() );
		}
		else if ( after == GameState.Active )
		{
			TimeUntilNextState = 600;
			Sound.FromScreen( To.Everyone, "sounds/music/round_intro/epic_intro_1.sound" );
		}
		else if ( after == GameState.End )
		{
			TimeUntilNextState = 10;
			ShowWinningTeam( To.Everyone, WinningTeam );
		}

		Event.Run( "gunfight.gamestate.changed", before, after );
	}

	public override void OnScoreChanged( Team team, int score, bool maxReached = false )
	{
		if ( score == 0 && State == GameState.Active )
		{
			var winner = team.GetOpponent();
			WinningTeam = winner;
			SetGameState( GameState.End );
		}
	}

	public enum GameState
	{
		WaitingForPlayers,
		Countdown,
		Active,
		End
	}

	protected void DecideWinner()
	{
		WinningTeam = Scores.GetHighestTeam();
		SetGameState( GameState.End );
	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		if ( IsServer )
		{
			SimulateTickets();

			if ( TimeUntilNextState )
			{
				if ( State == GameState.Countdown )
					SetGameState( GameState.Active );
				else if ( State == GameState.Active )
					DecideWinner();
				else if ( State == GameState.End )
					SetGameState( GameState.WaitingForPlayers );
			}
		}
	}

	public void SimulateTickets()
	{
		if ( LastTicket < 5f ) return;
		
		LastTicket = 0;

		foreach ( var capturePoint in Entity.All.OfType<CapturePointEntity>() )
		{
			var team = capturePoint.Team;
			if ( team != Team.Unassigned )
			{
				var otherTeam = team.GetOpponent();
				Scores.AddScore( otherTeam, -1 );
			}
		}
	}

	public override void OnPlayerKilled( GunfightPlayer player, DamageInfo damageInfo, out LifeState lifeState )
	{
		lifeState = LifeState.Dead;

		_ = SendToOverview( player );
	}

	public async Task SendToOverview( GunfightPlayer player )
	{
		await GameTask.DelaySeconds( 5f );
		player.LifeState = LifeState.Respawnable;
		SpawnOverview.Send( To.Single( player ) );
	}

	[ConCmd.Server( "gunfight_war_respawn" )]
	public static void AskToRespawn( int netIdent = 0 )
	{
		var player = ConsoleSystem.Caller.Pawn as GunfightPlayer;
		if ( player.LifeState != LifeState.Respawnable ) return;

		player.Respawn();

		if ( netIdent != 0 )
		{
			if ( Entity.FindByIndex( netIdent ) is ISpawnPoint spawnPoint && spawnPoint.IsValidSpawn( player ) )
			{
				var newTransform = spawnPoint.GetSpawnTransform();
				if ( newTransform != null )
					player.Transform = newTransform.Value;
			}
		}

		SendRespawnAnim( To.Single( player ) );
	}

	[ClientRpc]
	public static void SendRespawnAnim()
	{
		_ = new PlayerTransition( GunfightCamera.CameraOverride.Value.Position );
	}
}

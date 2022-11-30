using Sandbox.UI;
using System.ComponentModel.DataAnnotations;

namespace Facepunch.Gunfight;

[Display( Name = "Kill Confirmed", Description = "Each kill will leave behind dog tags for players to collect." )]
public partial class KillConfirmedGamemode : Gamemode
{
	[Net] public GameState State { get; protected set; }
	[Net] public RealTimeSince TimeSinceStateChanged { get; protected set; }
	[Net] public RealTimeUntil TimeUntilNextState { get; protected set; }
	[Net] public Team WinningTeam { get; protected set; }

	public TimeSpan TimeRemaining => TimeSpan.FromSeconds( TimeUntilNextState );
	public string FormattedTimeRemaining => TimeRemaining.ToString( @"mm\:ss" );
	protected string CachedTimeRemaining { get; set; }
	
	public override int MaximumScore => ConVarMaxScore;
	public override Panel HudPanel => new UI.KillConfirmedHud();
	public override List<Team> TeamSetup => new() { Team.BLUFOR, Team.OPFOR };
	public override bool AllowMovement => State != GameState.RoundCountdown;
	public override bool AllowDamage => State != GameState.RoundCountdown;
	public override bool AllowFriendlyFire => false;
	public override bool AllowSpectating => true;

	[ConVar.Server( "gunfight_kc_round_countdown" )]
	protected static float RoundCountdownLength { get; set; } = 10f;

	[ConVar.Server( "gunfight_kc_round_duration" )]
	protected static float RoundLength { get; set; } = 600f;

	[ConVar.Server( "gunfight_kc_max_score" )]
	protected static int ConVarMaxScore { get; set; } = 75;

	protected float GameWonLength => 15f;

	public override void Spawn()
	{
		base.Spawn();
		LoadoutSystem.AllowCustomLoadouts = true;
	}

	public override void AssignTeam( Client cl )
	{
		var teamComponent = cl.Components.GetOrCreate<TeamComponent>();
		teamComponent.Team = TeamSystem.GetLowestCount();
		UI.GunfightChatbox.AddChatEntry( To.Everyone, cl.Name, $"joined {teamComponent.Team.GetName()}", cl.PlayerId, false );
	}

	public override void OnClientJoined( Client cl )
	{
		base.OnClientJoined( cl );

		VerifyEnoughPlayers();
	}

	protected void VerifyEnoughPlayers()
	{
		if ( State == GameState.WaitingForPlayers )
		{
			if ( PlayerCount >= MinimumPlayers )
				SetGameState( GameState.RoundCountdown );
		}
	}


	public override bool PlayerLoadout( GunfightPlayer player )
	{
		RandomizeLoadout();
		LoadoutSystem.GetLoadout( player.Client )?.Give( player );
		GunfightStatusPanel.RpcUpdate( To.Everyone );

		return true;
	}

	public override bool CanPlayerRegenerate( GunfightPlayer player )
	{
		return true;
	}

	public override void PreSpawn( GunfightPlayer player )
	{
		player.SpawnPointTag = player.Team == Team.BLUFOR ? "one" : "two";
	}

	public void SetGameState( GameState newState )
	{
		var old = State;
		State = newState;

		Log.Info( $"Game State Changed to {newState}" );

		OnGameStateChanged( old, newState );
	}

	public override string GetGameStateLabel()
	{
		return State switch
		{
			GameState.WaitingForPlayers => $"NEED {MinimumPlayers - PlayerCount} PLAYER(S)",
			GameState.GameWon => $"{WinningTeam.GetName()} won",
			_ => base.GetGameStateLabel()
		};
	}

	public override string GetTimeLeftLabel()
	{
		return State switch
		{
			_ => FormattedTimeRemaining
		};
	}

	protected void OnGameStateChanged( GameState before, GameState after )
	{
		TimeSinceStateChanged = 0;

		GunfightStatusPanel.RpcUpdate( To.Everyone );

		if ( after == GameState.WaitingForPlayers )
		{
			WinningTeam = Team.Unassigned;
			var scores = GunfightGame.Current.Scores;
			scores.Reset();

			ResetStats();
			VerifyEnoughPlayers();
		}
		if ( after == GameState.RoundCountdown )
		{
			CleanupMap();
			TimeUntilNextState = RoundCountdownLength;
			RandomizeLoadout();
			RespawnAllPlayers();

			UI.GamemodeIdentity.RpcShow( To.Everyone, RoundCountdownLength.CeilToInt() );
			UI.LoadoutPanel.RpcShow( To.Everyone );
		}
		else if ( after == GameState.RoundActive )
			TimeUntilNextState = RoundLength;
		else if ( after == GameState.GameWon )
		{
			TimeUntilNextState = GameWonLength;
			ShowWinningTeam( To.Everyone, WinningTeam );
		}

		Event.Run( "gunfight.gamestate.changed", before, after );
	}

	protected void OnSecond()
	{
		CachedTimeRemaining = FormattedTimeRemaining;
		OnSecondElapsed();
	}

	protected virtual void OnSecondElapsed()
	{
		if ( State != GameState.RoundCountdown ) return;

		if ( TimeUntilNextState <= 6f && TimeUntilNextState >= 0f )
		{
			if ( TimeUntilNextState <= 1f )
			{
				Sound.FromScreen( "countdown.start" );
			}
			else
			{
				Sound.FromScreen( "countdown.beep" );
			}
		}
	}

	[Event.Frame]
	protected void Frame()
	{
		if ( CachedTimeRemaining != FormattedTimeRemaining )
		{
			OnSecond();
		}
	}

	public override void Simulate( Client cl )
	{
		if ( !IsServer ) return;

		if ( TimeUntilNextState )
		{
			// If the round counts down to 0, start the round
			if ( State == GameState.RoundCountdown )
				SetGameState( GameState.RoundActive );
			else if ( State == GameState.RoundActive )
				SetGameState( GameState.GameWon );
			else if ( State == GameState.GameWon )
				SetGameState( GameState.WaitingForPlayers );
		}
	}

	public override void PostProcessTick()
	{
		//var postProcess = Map.Camera.FindOrCreateHook<Sandbox.Effects.ScreenEffects>();
		//postProcess.colo.Enabled = State == GameState.RoundCountdown;
		//postProcess.ChromaticAberration.Scale = State == GameState.RoundCountdown ? 1 : 0;
		//if ( State == GameState.RoundCountdown )
		//{
		//	postProcess.ColorOverlay.Color = new Color( 0, 0, 0.3f );
		//	postProcess.ColorOverlay.Mode = StandardPostProcess.ColorOverlaySettings.OverlayMode.Mix;
		//	postProcess.ColorOverlay.Amount = 0.1f;

		//	postProcess.ChromaticAberration.Offset = new Vector3( -0.0007f, -0.0007f, 0f );
		//}
	}

	public override void OnScoreChanged( Team team, int score, bool maxReached = false )
	{
		if ( maxReached )
		{
			WinningTeam = team;
			SetGameState( GameState.GameWon );
		}
	}

	public override void OnPlayerKilled( GunfightPlayer player, DamageInfo damageInfo, out LifeState lifeState )
	{
		lifeState = LifeState.Respawning;
	}

	public override void PostPlayerKilled( GunfightPlayer player, DamageInfo lastDamage )
	{
		if ( State != GameState.RoundActive ) 
			return;

		_ = new DogtagEntity
		{
			Position = player.Position,
			ScoringTeam = player.Team
		};
    }

	public override void CleanupMap()
	{
		foreach( var weapon in Entity.All.OfType<GunfightWeapon>() )
		{
			if ( !weapon.Parent.IsValid() )
			{
				weapon.Delete();
			}
		}
	}

	protected override void TickServer()
	{
		Global.TimeScale = 1;

		if ( State == GameState.GameWon )
		{
			float timeSince = TimeSinceStateChanged;
			Global.TimeScale = 1 - timeSince.Remap( 0, 5, 0f, 0.75f ).Clamp( 0, 0.75f );
		}
	}

	[ConCmd.Admin( "gunfight_debug_kc_wingame" )]
	public static void WinGameDebug()
	{
		( GamemodeSystem.Current as KillConfirmedGamemode )?.SetGameState( GameState.GameWon );
	}

	public enum GameState
	{
		WaitingForPlayers, // to round countdown
		RoundCountdown, // to round active
		RoundActive, // to game won
		GameWon
	}
}

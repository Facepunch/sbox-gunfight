using Sandbox.UI;
using System.ComponentModel.DataAnnotations;

namespace Facepunch.Gunfight;

[Display( Name = "Gunfight", Description = "Two teams are pitted against each other. Everyone has the same loadout. Respawns are OFF." )]
public partial class GunfightGamemode : Gamemode
{
	[Net] public GameState State { get; protected set; }
	[Net] public TimeSince TimeSinceStateChanged { get; protected set; }
	[Net] public TimeUntil TimeUntilNextState { get; protected set; }
	[Net] public Team WinningTeam { get; protected set; }
	[Net] public CapturePointEntity ActiveFlag { get; set; }

	public TimeSpan TimeRemaining => TimeSpan.FromSeconds( TimeUntilNextState );
	public string FormattedTimeRemaining => TimeRemaining.ToString( @"mm\:ss" );

	protected string CachedTimeRemaining { get; set; }

	public override List<Team> TeamSetup => new() { Team.BLUFOR, Team.OPFOR };
	public override Panel HudPanel => new GunfightGamemodePanel();

	public override bool AllowMovement => State != GameState.RoundCountdown;
	public override bool AllowDamage => State != GameState.RoundCountdown;
	public override bool AllowSpectating => true;

	// Stats
	protected int MinimumPlayers => 4;
	protected float RoundCountdownLength => 10f;
	protected float RoundLength => 40f;
	protected float FlagActiveLength => 10f;
	protected float RoundOverLength => 10f;
	protected float GameWonLength => 15f;

	public override void Spawn()
	{
		base.Spawn();
		RandomizeLoadout();
	}

	public override void AssignTeam( Client cl )
	{
		var teamComponent = cl.Components.GetOrCreate<TeamComponent>();
		teamComponent.Team = TeamSystem.GetLowestCount();

		UI.GunfightChatbox.AddChatEntry( To.Everyone, cl.Name, $"joined {teamComponent.Team.GetName()}", cl.PlayerId, null, false );
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
			{
				SetGameState( GameState.RoundCountdown );
			}
		}
	}

	protected static Loadout GetRandomLoadout()
	{
		var loadouts = Loadout.WithTag( "gunfight" ).ToList();
		var index = Rand.Int( 1, loadouts.Count() ) - 1;
		var loadout = loadouts[index];

		return loadout;
	}

	protected void RandomizeLoadout()
	{
		var oldLoadout = LoadoutSystem.MatchLoadout;
		
		Loadout newLoadout = null;
		while ( newLoadout == null )
		{
			var randomLoadout = GetRandomLoadout();
			if ( randomLoadout == oldLoadout ) continue;

			newLoadout = randomLoadout;
		}

		LoadoutSystem.MatchLoadout = newLoadout;
	}

	public override bool PlayerLoadout( GunfightPlayer player )
	{
		LoadoutSystem.GetLoadout( player.Client )?.Give( player );
		GunfightStatusPanel.RpcUpdate( To.Everyone );

		return true;
	}

	public override bool CanPlayerRegenerate( GunfightPlayer player )
	{
		return State != GameState.RoundActive;
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
			GameState.RoundOver => "Round Over",
			GameState.GameWon => $"{WinningTeam.GetName()} won",
			_ => $"{GetTimeLeftLabel()}"
		};
	}

	string GetFlagActiveTime()
	{
		if ( ActiveFlag.CurrentState == CapturePointEntity.CaptureState.None )
			return FormattedTimeRemaining;
		return "";
	}

	public override string GetTimeLeftLabel()
	{
		return State switch
		{
			GameState.RoundFlagActive => GetFlagActiveTime(),
			_ => FormattedTimeRemaining
		};
	}

	public void CreateFlag()
	{
		var markers = GamemodeMarker.WithTag( "capturepoint" ).ToList();
		if ( markers.Count > 0 )
		{
			var rand = Rand.Int( 0, markers.Count - 1 );
			var marker = markers[rand];

			// Make the flag
			ActiveFlag = new CapturePointEntity
			{
				Transform = marker.Transform,
				Identity = "Capture the flag"
			};
		}
		else
		{
			Log.Error( "Map doesn't seem to have capture points for Gunfight mode." );
		}
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

			UI.LoadoutPanel.RpcShow( To.Everyone );
		}
		else if ( after == GameState.RoundActive )
			TimeUntilNextState = RoundLength;
		else if ( after == GameState.RoundFlagActive )
		{
			CreateFlag();
			TimeUntilNextState = FlagActiveLength;
		}
		else if ( after == GameState.RoundOver )
		{
			CleanupMap();
			RpcRoundWonMessage( To.Everyone );
			TimeUntilNextState = RoundOverLength;
		}
		else if ( after == GameState.GameWon )
		{
			TimeUntilNextState = GameWonLength;
			ShowWinningTeam( To.Everyone, WinningTeam );
		}

		Event.Run( "gunfight.gamestate.changed", before, after );
	}

	[ClientRpc]
	public void RpcRoundWonMessage()
	{
		// Show the round won panel.
		GunfightRoundWonPanel.Show();
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
			// If the round ends, activate the flag since not enough players have died
			else if ( State == GameState.RoundActive )
				SetGameState( GameState.RoundFlagActive );
			// If the flag has been active for too long, end the round anyway without a winner.
			else if ( State == GameState.RoundFlagActive )
			{
				// Only end the round if there's nobody on the flag
				if ( ActiveFlag.CurrentState == CapturePointEntity.CaptureState.None )
					SetGameState( GameState.RoundOver );
			}
			// After the round ends, go back to countdown.
			else if ( State == GameState.RoundOver )
				SetGameState( GameState.RoundCountdown );
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

	protected void WinRound( Team team )
	{
		var scores = GunfightGame.Current.Scores;
		var newScore = scores.AddScore( team, 1 );

		UI.GunfightChatbox.AddInformation( To.Everyone, $"{team.GetName()} won the round!" );

		// Round ends!
		if ( newScore < scores.MaximumScore )
			SetGameState( GameState.RoundOver );
	}

	public override void OnScoreChanged( Team team, int score, bool maxReached = false )
	{
		if ( maxReached )
		{
			WinningTeam = team;
			SetGameState( GameState.GameWon );
		}
	}

	protected void CheckDeadPlayers()
	{
		var aliveBluePlayers = Team.BLUFOR.AlivePlayers();
		var aliveRedPlayers = Team.OPFOR.AlivePlayers();

		if ( aliveBluePlayers.Count() == 0 )
		{
			WinRound( Team.OPFOR );
		}
		else if ( aliveRedPlayers.Count() == 0 )
		{
			WinRound( Team.BLUFOR );
		}
	}

	public override void OnPlayerKilled( GunfightPlayer player, DamageInfo damageInfo, out LifeState lifeState )
	{
		if ( State == GameState.WaitingForPlayers )
		{
			lifeState = LifeState.Respawning;
			return;
		}

		// Do not allow automatic respawning
		lifeState = LifeState.Dead;
	}

	public override void PostPlayerKilled( GunfightPlayer player, DamageInfo lastDamage )
	{
		if ( State == GameState.RoundActive || State == GameState.RoundFlagActive )
		{
			CheckDeadPlayers();
		}

		GunfightStatusPanel.RpcUpdate( To.Everyone );
	}

	public override void CleanupMap()
	{
		// Delete the flag if it exists.
		ActiveFlag?.Delete();

		foreach( var weapon in Entity.All.OfType<GunfightWeapon>() )
		{
			if ( !weapon.Parent.IsValid() )
			{
				weapon.Delete();
			}
		}
	}

	public override void OnFlagCaptured( CapturePointEntity flag, Team team )
	{
		// Might not even need this, tbh
		if ( State != GameState.RoundFlagActive ) return;

		WinRound( team );
	}

	public enum GameState
	{
		WaitingForPlayers, // to round countdown
		RoundCountdown, // to round active
		RoundActive, // to either round over, or flag becomes active
		RoundFlagActive, // to round over
		RoundOver, // to round countdown
		GameWon
	}
}

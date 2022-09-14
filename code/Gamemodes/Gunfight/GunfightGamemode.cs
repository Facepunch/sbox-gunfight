using Sandbox.UI;
using System.ComponentModel.DataAnnotations;

namespace Facepunch.Gunfight;

[Display( Name = "Gunfight Gamemode" )]
public partial class GunfightGamemode : GamemodeEntity
{
	[Net] public GameState State { get; protected set; }
	[Net] public TimeSince TimeSinceStateChanged { get; protected set; }
	[Net] public TimeUntil TimeUntilNextState { get; protected set; }
	[Net] public Team WinningTeam { get; protected set; }

	public Loadout CurrentLoadout { get; set; }

	[Net] public CapturePointEntity ActiveFlag { get; set; }

	public TimeSpan TimeRemaining => TimeSpan.FromSeconds( TimeUntilNextState );
	public string FormattedTimeRemaining => TimeRemaining.ToString( @"mm\:ss" );

	// Stats
	protected int MinimumPlayers => 4;
	protected float RoundCountdownLength => 5f;
	protected float RoundLength => 40f;
	protected float FlagActiveLength => 10f;
	protected float RoundOverLength => 5f;
	protected float GameWonLength => 15f;

	public override Panel GetHudPanel() => new GunfightGamemodePanel();

	public override void OnClientJoined( Client cl )
	{
		base.OnClientJoined( cl );

		var teamComponent = cl.Components.GetOrCreate<TeamComponent>();
		teamComponent.Team = TeamSystem.GetLowestCount();

		ChatBox.AddInformation( ToExtensions.Team( teamComponent.Team ), $"{cl.Name} joined {teamComponent.Team.GetName()}" );

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

	protected void RandomizeLoadout()
	{
		var loadouts = Loadout.WithTag( "gunfight" ).ToList();
		var index = Rand.Int( 1, loadouts.Count() ) - 1;
		var loadout = loadouts[index];

		CurrentLoadout = loadout;
	}

	public override bool PlayerLoadout( GunfightPlayer player )
	{
		bool loadoutChosen = CurrentLoadout != null;
		if ( loadoutChosen )
		{
			CurrentLoadout.Give( player );
		}

		return loadoutChosen;
	}

	public override bool AllowMovement()
	{
		return State != GameState.RoundCountdown;
	}

	public override bool AllowDamage()
	{
		return State != GameState.RoundCountdown;
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

	public string GetGameStateLabel()
	{
		return State switch
		{
			GameState.WaitingForPlayers => $"Waiting for players",
			GameState.RoundCountdown => $"Prepare to fight",
			GameState.RoundActive => $"Eliminate the enemies",
			GameState.RoundFlagActive => $"",
			GameState.RoundOver => "Round over",
			GameState.GameWon => $"{WinningTeam.GetName()} won the match!",
			_ => "Gunfight"
		};
	}

	string GetFlagActiveTime()
	{
		if ( ActiveFlag.CurrentState == CapturePointEntity.CaptureState.None )
			return FormattedTimeRemaining;
		return "";
	}

	public string GetTimeLeftLabel()
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
			TimeUntilNextState = RoundOverLength;
		}
		else if ( after == GameState.GameWon )
		{
			TimeUntilNextState = GameWonLength;
			ChatBox.AddInformation( To.Everyone, $"{WinningTeam.GetName()} won the match!" );
		}

		Event.Run( "gunfight.gamestate.changed", before, after );
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

	public override void PostProcessTick( StandardPostProcess postProcess )
	{
		postProcess.ColorOverlay.Enabled = State == GameState.RoundCountdown;
		postProcess.ChromaticAberration.Enabled = State == GameState.RoundCountdown;
		if ( State == GameState.RoundCountdown )
		{
			postProcess.ColorOverlay.Color = new Color( 0, 0, 0.3f );
			postProcess.ColorOverlay.Mode = StandardPostProcess.ColorOverlaySettings.OverlayMode.Mix;
			postProcess.ColorOverlay.Amount = 0.1f;

			postProcess.ChromaticAberration.Offset = new Vector3( -0.0007f, -0.0007f, 0f );
		}
	}

	protected void WinRound( Team team )
	{
		var scores = GunfightGame.Current.Scores;
		var newScore = scores.AddScore( team, 1 );

		ChatBox.AddInformation( To.Everyone, $"{team.GetName()} won the round!" );

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
	}

	public override void CleanupMap()
	{
		// Delete the flag if it exists.
		ActiveFlag?.Delete();
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

using Sandbox.UI;
using System.ComponentModel.DataAnnotations;

namespace Facepunch.Gunfight;

[Display( Name = "Gunfight Gamemode" )]
public partial class GunfightGamemode : GamemodeEntity
{
	[Net] public GameState State { get; protected set; }
	[Net] public TimeSince TimeSinceStateChanged { get; protected set; }
	[Net] public TimeUntil TimeUntilNextState { get; protected set; }

	public TimeSpan TimeRemaining => TimeSpan.FromSeconds( TimeUntilNextState );
	public string FormattedTimeRemaining => TimeRemaining.ToString( @"mm\:ss" );

	// Stats
	protected int MinimumPlayers => 4;
	protected float RoundCountdownLength => 10f;
	protected float RoundLength => 120f;
	protected float FlagActiveLength => 30f;
	protected float FlagCaptureTime => 5f;
	protected float RoundOverLength => 10f;

	public override Panel GetHudPanel() => new GunfightGamemodePanel();

	public override void OnClientJoined( Client cl )
	{
		base.OnClientJoined( cl );

		var teamComponent = cl.Components.GetOrCreate<TeamComponent>();
		teamComponent.Team = TeamSystem.GetLowestCount();

		ChatBox.AddInformation( ToExtensions.Team( teamComponent.Team ), $"{cl.Name} joined {teamComponent.Team.GetName()}" );

		if ( State == GameState.WaitingForPlayers )
		{
			if ( PlayerCount >= MinimumPlayers )
			{
				SetGameState( GameState.RoundCountdown );
			}
		}
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
			GameState.RoundCountdown => $"Get ready: {FormattedTimeRemaining}",
			GameState.RoundActive => $"Eliminate: {FormattedTimeRemaining}",
			GameState.RoundFlagActive => $"Capture the flag: {FormattedTimeRemaining}",
			GameState.RoundOver => "Round over",
			GameState.GameWon => "Game over",
			_ => "Gunfight"
		};
	}

	protected void OnGameStateChanged( GameState before, GameState after )
	{
		TimeSinceStateChanged = 0;

		if ( after == GameState.RoundCountdown )
		{
			TimeUntilNextState = RoundCountdownLength;
			RespawnAllPlayers();
		}
		else if ( after == GameState.RoundActive )
			TimeUntilNextState = RoundLength;
		else if ( after == GameState.RoundFlagActive )
			TimeUntilNextState = FlagActiveLength;
		else if ( after == GameState.RoundOver )
			TimeUntilNextState = RoundOverLength;

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
				SetGameState( GameState.RoundOver );
			// After the round ends, go back to countdown.
			else if ( State == GameState.RoundOver )
				SetGameState( GameState.RoundCountdown );
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
		scores.AddScore( team, 1 );

		ChatBox.AddInformation( To.Everyone, $"{team.GetName()} won the round!" );

		// Round ends!
		SetGameState( GameState.RoundOver );
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

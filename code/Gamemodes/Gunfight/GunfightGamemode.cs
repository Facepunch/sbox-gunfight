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
	protected int MinimumPlayers => 2;
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

		ChatBox.AddInformation( ToExtensions.Team( teamComponent.Team ), $"{cl.Name} joined {TeamSystem.GetTeamName( teamComponent.Team )}" );

		if ( State == GameState.WaitingForPlayers )
		{
			if ( PlayerCount >= MinimumPlayers )
			{
				SetGameState( GameState.RoundCountdown );
			}
		}
	}

	public void SetGameState( GameState newState )
	{
		var old = State;
		State = newState;

		Log.Info( $"Game State Changed to {newState}" );

		OnGameStateChanged( old, State );
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
			TimeUntilNextState = RoundCountdownLength;
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

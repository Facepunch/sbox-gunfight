using Sandbox.UI;
using System.ComponentModel.DataAnnotations;

namespace Facepunch.Gunfight.War;

[Display( Name = "War", Description = "All-out war against two large teams. First team to drain the other team's tickets wins." )]
public partial class WarGamemode : Gamemode
{
	[Net] public GameState State { get; set; } = GameState.WaitingForPlayers;
	[Net] public RealTimeSince TimeSinceStateChanged { get; protected set; }
	[Net] public RealTimeUntil TimeUntilNextState { get; protected set; }
	public TimeSpan TimeRemaining => TimeSpan.FromSeconds( TimeUntilNextState );
	public string FormattedTimeRemaining => TimeRemaining.ToString( @"mm\:ss" );

	protected string CachedTimeRemaining { get; set; }
	public override void Spawn()
	{
		base.Spawn();

		LoadoutSystem.AllowCustomLoadouts = true;
	}

	[ConVar.Server( "gunfight_gamemode_war_maxscore" )]
	private static int MaximumScoreConfig { get; set; } = 1000;
	public override int MaximumScore => MaximumScoreConfig;
	public override Panel GetHudPanel() => new WarHud();
	public override List<Team> TeamSetup => new() { Team.BLUFOR, Team.OPFOR };

	public override bool AllowFriendlyFire()
	{
		return false;
	}

	public override bool AllowRespawning()
	{
		return true;
	}

	public override bool AllowSpectating()
	{
		return true;
	}

	public override bool CanPlayerRegenerate( GunfightPlayer player )
	{
		return true;
	}

	public override void AssignTeam( Client cl )
	{
		var teamComponent = cl.Components.GetOrCreate<TeamComponent>();
		teamComponent.Team = TeamSystem.GetLowestCount();

		UI.GunfightChatbox.AddChatEntry( To.Everyone, cl.Name, $"joined {teamComponent.Team.GetName()}", cl.PlayerId, null, false );
	}

	public override bool PlayerLoadout( GunfightPlayer player )
	{
		return base.PlayerLoadout( player );
	}

	public override void PostPlayerKilled( GunfightPlayer player, DamageInfo lastDamage )
	{
		base.PostPlayerKilled( player, lastDamage );
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
			_ => FormattedTimeRemaining
		};
	}

	public override void OnFlagCaptured( CapturePointEntity flag, Team team )
	{
		UI.GunfightChatbox.AddInformation( To.Everyone, $"{team.GetName()} captured {flag.NiceName}" );
	}

	public void SetGameState( GameState newState )
	{
		var old = State;
		State = newState;

		Log.Info( $"Game State Changed to {newState}" );

		OnGameStateChanged( old, newState );
	}

	protected void OnGameStateChanged( GameState before, GameState after )
	{
	}

	public enum GameState
	{
		WaitingForPlayers,
		Countdown,
		Active,
		End
	}
}

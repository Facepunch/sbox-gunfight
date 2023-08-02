namespace Facepunch.Gunfight;

public enum Team
{
	BLUFOR,
	OPFOR,
	Unassigned
}

public static class TeamExtensions
{
	public static To NetClients( this Team team )
	{
		return To.Multiple( team.AllClients().Select( x => x ) );
	}

	public static int Count( this Team team )
	{
		return AllClients( team ).Count();
	}

	public static IEnumerable<IClient> AllClients( this Team team )
	{
		return Game.Clients.Where( x => TeamSystem.GetTeam( x ) == team );
	}

	public static IEnumerable<GunfightPlayer> AllPlayers( this Team team )
	{
		return AllClients( team ).Select( x => x.Pawn as GunfightPlayer );
	}

	public static IEnumerable<GunfightPlayer> AlivePlayers( this Team team )
	{
		return team.AllPlayers().Where( x => x.LifeState == LifeState.Alive );
	}

	public static Team GetTeam( this IClient cl )
	{
		return TeamSystem.GetTeam( cl );
	}

	public static string GetLocation( this IClient cl )
	{
		var pawn = cl.Pawn as GunfightPlayer;

		if ( string.IsNullOrEmpty( pawn?.PlayerLocation ) )
			return "UNKNOWN";

		return pawn.PlayerLocation;
	}

	public static string GetName( this Team team )
	{
		return TeamSystem.GetTeamName( team );
	}

	public static Team GetOpponent( this Team team )
	{
		if ( team == Team.BLUFOR )
			return Team.OPFOR;

		if ( team == Team.OPFOR )
			return Team.BLUFOR;

		return Team.Unassigned;
	}

	public static string GetTag( this Team team )
	{
		return team switch {
			Team.BLUFOR => "blue",
			Team.OPFOR => "red",
			_ => "unknown"
		};
	}
}

public static class TeamSystem
{
	public static T ToEnum<T>( this string enumString )
	{
		return (T) Enum.Parse( typeof( T ), enumString );
	}

	public static Team MyTeam => Game.LocalClient.Components.Get<TeamComponent>()?.Team ?? Team.Unassigned;

	public enum FriendlyStatus
	{
		Friendly,
		Hostile,
		Neutral
	}

	public static FriendlyStatus GetFriendState( Team one, Team two )
	{
		if ( one == Team.Unassigned || two == Team.Unassigned )
			return FriendlyStatus.Neutral;

		if ( one != two )
			return FriendlyStatus.Hostile;

		return FriendlyStatus.Friendly;
	}

	public static FriendlyStatus GetFriendState( IClient one, IClient two )
	{
		var teamOne = one.GetTeam();
		var teamTwo = two.GetTeam();

		return GetFriendState( teamOne, teamTwo );
	}

	public static FriendlyStatus GetFriendState( IClient client, Team team )
	{
		return GetFriendState( GetTeam( client ), team );
	}

	public static Team GetLowestCount()
	{
		var bluforCount = Team.BLUFOR.Count();
		var opforCount = Team.OPFOR.Count();

		if ( opforCount < bluforCount )
			return Team.OPFOR;

		return Team.BLUFOR;
	}

	public static bool IsFriendly( Team one, Team two )
	{
		return GetFriendState( one, two ) == FriendlyStatus.Friendly;
	}

	public static bool IsHostile( Team one, Team two )
	{
		return GetFriendState( one, two ) == FriendlyStatus.Hostile;
	}

	public static Team GetEnemyTeam( Team team )
	{
		switch( team )
		{
			case Team.BLUFOR:
				return Team.OPFOR;
			case Team.OPFOR:
				return Team.BLUFOR;
		}

		return Team.Unassigned;
	}

	public static string GetTeamName( Team team )
	{
		switch( team )
		{
			case Team.BLUFOR:
				return "Task Force 99";
			case Team.OPFOR:
				return "Bialystok Marines";
		}

		return "Unassigned";
	}

	public static Team GetTeam( IClient cl )
	{
		return cl?.Components.Get<TeamComponent>()?.Team ?? Team.Unassigned;
	}

	[ConCmd.Server( "gunfight_team_join" )]
	public static void JoinTeam( string name )
	{
		var player = ConsoleSystem.Caller.Pawn as GunfightPlayer;
		var team = name.ToEnum<Team>();

		player.Team = team;
	}
}

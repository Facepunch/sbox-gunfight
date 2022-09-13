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

	public static IEnumerable<Client> AllClients( this Team team )
	{
		return Client.All.Where( x => TeamSystem.GetTeam( x ) == team );
	}

	public static IEnumerable<GunfightPlayer> AllPlayers( this Team team )
	{
		return AllClients( team ).Select( x => x.Pawn as GunfightPlayer );
	}

	public static Team GetTeam( this Client cl )
	{
		return TeamSystem.GetTeam( cl );
	}
}

public static class TeamSystem
{
	public static T ToEnum<T>( this string enumString )
	{
		return (T) Enum.Parse( typeof( T ), enumString );
	}

	public static Team MyTeam => Local.Client.Components.Get<TeamComponent>()?.Team ?? Team.Unassigned;

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

	public static FriendlyStatus GetFriendState( Client one, Client two )
	{
		var teamOne = one.GetTeam();
		var teamTwo = two.GetTeam();

		return GetFriendState( teamOne, teamTwo );
	}

	public static FriendlyStatus GetFriendState( Client client, Team team )
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

		return "N/A";
	}

	public static Team GetTeam( Client cl )
	{
		return cl.Components.Get<TeamComponent>()?.Team ?? Team.Unassigned;
	}

	[ConCmd.Server( "gunfight_team_join" )]
	public static void JoinTeam( string name )
	{
		var player = ConsoleSystem.Caller.Pawn as GunfightPlayer;
		var team = name.ToEnum<Team>();

		player.Team = team;
	}
}

using Sandbox.Menu;

namespace Facepunch.Gunfight.Utility;

public partial class MatchmakerUtility
{
	// Check to see if a lobby has a compatible map with our query
	static bool CompatibleMap( ILobby lobby, string[] maps = null )
	{
		if ( maps == null || maps.Length == 0 ) return true;
		if ( maps.Contains( lobby.Map ) ) return true;

		return false;
	}

	// Check to see if a lobby has a compatible gamemode with our query
	static bool CompatibleGameMode( ILobby lobby, string gamemode = null )
	{
		if ( string.IsNullOrEmpty( gamemode ) ) return true;
		if ( lobby.Data.TryGetValue( "gunfight-gamemode", out var foundMode ) &&
		     foundMode.Equals( gamemode ) ) return true;
		
		return false;
	}
	
	// Try to find a lobby based on a gamemode string and map list
	public static async Task<ILobby?> FindLobby( string gamemode = null, string[] maps = null, int reservedSlots = 1 )
	{
		await Game.Menu.QueryLobbiesAsync( null, reservedSlots );

		var lobbies = Game.Menu.Lobbies
			.Where( x => CompatibleGameMode( x, gamemode ) )
			.Where( x => CompatibleMap( x, maps ) );
		
		return lobbies.FirstOrDefault();
	}
}

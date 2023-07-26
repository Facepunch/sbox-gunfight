
namespace Facepunch.Gunfight.Utility;

public partial class MatchmakerUtility
{
	// Check to see if a lobby has a compatible map with our query
	static bool CompatibleMap( Sandbox.Services.ServerList.Entry server, string[] maps = null )
	{
		if ( maps == null || maps.Length == 0 ) return true;
		if ( maps.Contains( server.Map ) ) return true;

		return false;
	}

	// Check to see if a lobby has a compatible gamemode with our query
	static bool CompatibleGameMode( Sandbox.Services.ServerList.Entry server, string gamemode = null )
	{
		if ( string.IsNullOrEmpty( gamemode ) ) return true;
		
		// TODO - Return false,
		// Look for some kind of metadata on the server for telling which the gunfight mode is.
		
		return true;
	}
	
	static Sandbox.Services.ServerList serverList;

	public async static Task<Sandbox.Services.ServerList.Entry?> FindServer( string gamemode = null, string[] maps = null, int reservedSlots = 1 )
	{
		serverList?.Dispose();
		serverList = new Sandbox.Services.ServerList();

		// Look for our game
		serverList.AddFilter( "gametagsand", $"game:{Game.Menu.Package.FullIdent}" );

		// Search
		serverList.Query();

		while ( serverList.IsQuerying )
		{
			await Task.Delay( 100 );
		}

		Log.Info( $"We found {serverList.Count} servers" );

		if ( serverList.Count == 0 )
		{
			return null;
		}

		var compatibleServers = serverList
			.Where( x => x.SteamId != 0 )
			.Where( x => x.Players + reservedSlots <= x.MaxPlayers )
			.Where( x => CompatibleMap( x, maps ) )
			.Where( x => CompatibleGameMode( x, gamemode ) )
			.OrderByDescending( x => x.Players );

		Log.Info( $"We found {compatibleServers.Count()} COMPATIBLE servers" );
		
		if ( !compatibleServers.Any() )
		{
			return null;
		}

		return compatibleServers.First();
	}
}

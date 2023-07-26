using Sandbox.Menu;

namespace Facepunch.Gunfight.Utility;

public partial class MatchmakerUtility
{
	public static async Task<bool> Search( string mode = null, string[] maps = null, int reservedSlots = 1 )
	{
		var lobby = await FindLobby( mode, maps, reservedSlots );

		if ( lobby != null )
		{
			// Connect to lobby

			return true;
		}

		var server = await FindServer( mode, maps, reservedSlots );
		if ( server != null )
		{
			// Connect to server 

			return true;
		}

		return false;
	}
}

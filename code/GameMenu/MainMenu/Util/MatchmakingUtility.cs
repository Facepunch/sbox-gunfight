using Sandbox.Menu;
using Sandbox.Services;

namespace Facepunch.Gunfight;

public partial class MatchmakingSystem
{
	public static State CurrentState { get; set; } = State.Empty;
	public static TimeSince TimeSinceSearch { get; set; } = 0;

	public static async Task JoinServer( ServerList.Entry server )
	{
		CurrentState = State.Found;
		await Task.Delay( 1000 );

		CurrentState = State.Empty;
		// Join server
		Game.Menu.ConnectToServer( server.SteamId );
	}

	public static async Task JoinLobby( ILobby lobby )
	{
		CurrentState = State.Found;
		await Task.Delay( 1000 );

		var result = await lobby.JoinAsync();
		if ( !result )
		{
			CurrentState = State.Searching;
		}
		else
		{
			CurrentState = State.Empty;
		}
	}
	
	public static async Task<bool> Search( string mode = null, string[] maps = null, int reservedSlots = 1 )
	{
		var lastState = CurrentState;
		CurrentState = State.Searching;
		
		if ( lastState != State.NothingAvailable ) TimeSinceSearch = 0;

		var lobby = await FindLobby( mode, maps, reservedSlots );

		if ( lobby != null )
		{
			await JoinLobby( lobby );
			return true;
		}

		var server = await FindServer( mode, maps, reservedSlots );
		if ( server != null )
		{
			await JoinServer( server.Value );
			return true;
		}

		CurrentState = State.NothingAvailable;
		return false;
	}

	public enum State
	{
		Empty,
		//
		Searching,
		Found,
		NothingAvailable
	}
}

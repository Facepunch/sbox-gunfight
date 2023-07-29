using Sandbox.Menu;
using Sandbox.Services;

namespace Facepunch.Gunfight;

public partial class MatchmakingSystem
{
	public static State CurrentState { get; set; } = State.Empty;
	public static TimeSince TimeSinceSearch { get; set; } = 0;

	public static async Task<bool> JoinServer( ServerList.Entry server )
	{
		CurrentState = State.Found;
		await Task.Delay( 1000 );

		CurrentState = State.Empty;
		// Join server
		Game.Menu.ConnectToServer( server.SteamId );

		return true;
	}

	public static async Task<bool> JoinLobby( ILobby lobby )
	{
		CurrentState = State.Found;
		await Task.Delay( 1000 );

		var result = await lobby.JoinAsync();
		
		
		if ( !result )
		{
			Log.Trace( "Failed to join lobby" );
			CurrentState = State.Searching;
		}
		else
		{
			Log.Trace( $"Joined lobby successfully {lobby}" );
			CurrentState = State.Empty;

			MainMenu.MainMenu.CurrentLobby = lobby;
		}

		return result;
	}
	
	public static async Task<bool> Search( string mode = null, string[] maps = null, int reservedSlots = 1 )
	{
		var lastState = CurrentState;
		CurrentState = State.Searching;
		
		if ( lastState != State.NothingAvailable ) TimeSinceSearch = 0;

		var lobby = await FindLobby( mode, maps, reservedSlots );

		if ( lobby != null )
		{
			return await JoinLobby( lobby );
		}

		var server = await FindServer( mode, maps, reservedSlots );
		if ( server != null )
		{
			return await JoinServer( server.Value );
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

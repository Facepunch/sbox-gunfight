using System.Threading;
using Sandbox.Menu;
using Sandbox.Services;

namespace Facepunch.Gunfight;

public partial class MatchmakingSystem
{
	public static State CurrentState { get; set; } = State.Empty;
	public static TimeSince TimeSinceSearch { get; set; } = 0;
	
	public static CancellationTokenSource CancellationToken = new();

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
			MainMenu.MainMenu.CurrentLobby = new( lobby );
		}

		return result;
	}
	
	private static async Task<bool> Search( string mode = null, string[] maps = null, int reservedSlots = 1 )
	{
		var lastState = CurrentState;
		CurrentState = State.Searching;
		
		if ( lastState != State.NothingAvailable ) TimeSinceSearch = 0;

		var server = await FindServer( mode, maps, reservedSlots );
		CancellationToken.Token.ThrowIfCancellationRequested();

		if ( server != null )
		{
			return await JoinServer( server.Value );
		}
		
		var lobby = await FindLobby( mode, maps, reservedSlots );
		CancellationToken.Token.ThrowIfCancellationRequested();

		if ( lobby != null )
		{
			return await JoinLobby( lobby );
		}
		
		CurrentState = State.NothingAvailable;
		return false;
	}

	public static void StopMatchmaking()
	{
		CancellationToken.Cancel();
	}

	private static async Task<string> TryMatchmake( string mode = null, string[] maps = null, int reservedSlots = 1 )
	{
		while ( CurrentState != State.Found )
		{
			if ( TimeSinceSearch > 30 )
			{
				CurrentState = State.Found;
				await Task.Delay( 1000 );

				// Make the lobby
				var lobby = await Game.Menu.CreateLobbyAsync( 16, "gunfight", true );
				lobby.Map = Game.Random.FromArray( maps );
				lobby.State = "lobby";
				lobby.SetData( "convar.gunfight_gamemode", mode );
				lobby.Title = $"{lobby.Owner.Name}'s game";

				CurrentState = State.Empty;

				return "lobby";
			}
			
			var result = await Search( mode, maps, reservedSlots );
			CancellationToken.Token.ThrowIfCancellationRequested();
		}
		
		return MainMenu.MainMenu.CurrentLobby != null ? "lobby" : "server";
	}
	
	public static async Task<string> Matchmake( string mode = null, string[] maps = null, int reservedSlots = 1 )
	{
		TimeSinceSearch = 0;
		CancellationToken?.Cancel();
		CancellationToken = new();
		
		try
		{
			var result = await TryMatchmake( mode, maps, reservedSlots );
			return result;
		}
		catch ( Exception e )
		{
			Log.Info( $"Stopped matchmaking for reason {e}" );
			
			CurrentState = State.Empty;
			return null;
		}
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

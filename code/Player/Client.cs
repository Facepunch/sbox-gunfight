namespace Gunfight;

/// <summary>
/// Placed on the player (or maybe just in loose space, we'll see), the client component holds network info about a player, and serves as an easy way to iterate through players in a game.
/// </summary>
public sealed class Client : Component
{
	/// <summary>
	/// Get a list of all clients in the game's active scene.
	/// </summary>
	public static IEnumerable<Client> All => GameManager.ActiveScene.GetAllComponents<Client>();

	/// <summary>
	/// Gets a reference to the local client.
	/// </summary>
	public static Client Local => All.FirstOrDefault( x => x.IsMe );

	/// <summary>
	/// Are we connected to a server?
	/// </summary>
	public bool IsConnected { get; private set; } = false;

	/// <summary>
	/// Is this client me? (The local client)
	/// </summary>
	[Property] public bool IsMe { get; private set; } = false;

	/// <summary>
	/// Is this client hosting the current game session?
	/// </summary>
	[Property] public bool IsHost { get; private set; } = false;

	/// <summary>
	/// The client's SteamId
	/// </summary>
	[Property] public ulong SteamId { get; private set; } = 0;

	/// <summary>
	/// The client's DisplayName
	/// </summary>
	[Property] public string DisplayName { get; private set; } = "User";

	public void Setup( Connection channel )
	{
		IsConnected = true;
		SteamId = channel.SteamId;
		DisplayName = channel.DisplayName;
		IsHost = channel.IsHost;
		IsMe = Connection.Local == channel;

		Log.Info( $"Setup: {SteamId}" );
	}
}

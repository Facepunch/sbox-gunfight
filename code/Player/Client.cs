namespace Gunfight;

/// <summary>
/// Placed on the player (or maybe just in loose space, we'll see), the client component holds network info about a player, and serves as an easy way to iterate through players in a game.
/// </summary>
public sealed class Client : Component, Component.INetworkListener
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
	public bool IsConnected { get; set; } = false;

	/// <summary>
	/// Is this client me? (The local client)
	/// </summary>
	public bool IsMe { get; set; } = false;

	/// <summary>
	/// Is this client hosting the current game session?
	/// </summary>
	public bool IsHost { get; set; } = false;

	/// <summary>
	/// The client's SteamId
	/// </summary>
	public ulong SteamId { get; set; } = 0;

	/// <summary>
	/// The client's DisplayName
	/// </summary>
	public string DisplayName { get; set; } = "User";

	/// <summary>
	/// This method is called by having our component implement INetworkListener.
	/// </summary>
	/// <param name="channel"></param>
	public void OnActive( Connection channel )
	{
		IsConnected = true;
		SteamId = channel.SteamId;
		DisplayName = channel.DisplayName;
		IsHost = channel.IsHost;
		IsMe = Connection.Local == channel;
	}
}

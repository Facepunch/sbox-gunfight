using Sandbox.Network;

namespace Gunfight;

public sealed class GameNetworkManager : Component, Component.INetworkListener
{
	/// <summary>
	/// Which player prefab should we spawn?
	/// </summary>
	[Property] public GameObject PlayerPrefab { get; set; }

	/// <summary>
	/// Where should we put the player?
	/// TODO: gamemode / game state should control this
	/// </summary>
	[Property] public GameObject SpawnPoint { get; set; }

	/// <summary>
	/// Is this game multiplayer? If not, we won't create a lobby.
	/// </summary>
	[Property] public bool IsMultiplayer { get; set; } = true;

	protected override void OnStart()
	{
		if ( !IsMultiplayer ) return;

		//
		// Create a lobby if we're not connected
		//
		if ( !GameNetworkSystem.IsActive )
		{
			GameNetworkSystem.CreateLobby();
		}
	}

	/// <summary>
	/// Called when a network connection becomes active
	/// </summary>
	/// <param name="channel"></param>
	public void OnActive( Connection channel )
	{
		if ( !IsMultiplayer ) return;

		Log.Info( $"Player '{channel.DisplayName}' is becoming active" );

		var player = PlayerPrefab.Clone( SpawnPoint.Transform.World );
		player.NetworkSpawn( channel );

		var playerComponent = player.Components.Get<PlayerController>();
		if ( playerComponent.IsValid() )
		{
			playerComponent.NetPossess();
		}
	}
}

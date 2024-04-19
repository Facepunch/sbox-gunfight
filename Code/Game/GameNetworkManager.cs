using Sandbox.Network;
using System.Threading.Channels;

namespace Gunfight;

public sealed class GameNetworkManager : Component, Component.INetworkListener
{
	[Property] public GameObject PlayerPrefab { get; set; }
	[Property] public GameObject SpawnPoint { get; set; }

	/// <summary>
	/// Is this game multiplayer?
	/// </summary>
	[Property] public bool IsMultiplayer { get; set; } = true;

	[Property] public bool IsDebugging { get; set; } = true;

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

using Sandbox.Network;

namespace Gunfight;

public sealed class GameNetworkManager : Component, Component.INetworkListener
{
	[Property] public GameObject PlayerPrefab { get; set; }
	[Property] public GameObject SpawnPoint { get; set; }

	protected override void OnStart()
	{
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
		Log.Info( $"Player '{channel.DisplayName}' is becoming active" );

		var player = SceneUtility.Instantiate( PlayerPrefab, SpawnPoint.Transform.World );

		var cl = player.Components.Create<Client>();
		cl.Setup( channel );

		player.Network.Spawn( channel );
	}
}

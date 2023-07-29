using Facepunch.Gunfight.Proto;
using Sandbox.Menu;

namespace Facepunch.Gunfight;

public partial class GunfightLobby
{
	void OnNetworkMessage( ILobby.NetworkMessage msg )
	{
		var msgType = msg.Data.Read<MessageType>();
		
		switch ( msgType )
		{
			case MessageType.ReadyState:
			{
				OnReady( msg.Source, msg.Data.Read<bool>() );
				break;
			}
		}
	}
	
	public void Tick()
	{
		if ( _lobby == null )
			return;

		if ( !_lobby.IsMember )
		{
			_lobby = null;
			return;
		}
		
		_lobby.ReceiveMessages( OnNetworkMessage );
	}
}


using Facepunch.Gunfight.Proto;

namespace Facepunch.Gunfight;

public partial class GunfightLobby
{
	/// <summary>
	/// Sets our ready up state.
	/// </summary>
	/// <param name="isReady"></param>
	public void SetReady( bool isReady )
	{
		var b = ByteStream.Create( 1 );
		b.Write( MessageType.ReadyState );
		b.Write( isReady );
		_lobby.BroadcastMessage( b );
	}

	/// <summary>
	/// Am I ready?
	/// </summary>
	public bool IsLocalPlayerReady => IsReady( new Friend( Game.SteamId ) );

	private Dictionary<Friend, bool> ReadyStates = new();
	void OnReady( Friend friend, bool readyState )
	{
		ReadyStates[friend] = readyState;
	}

	public bool IsReady( Friend friend )
	{
		if ( ReadyStates.TryGetValue( friend, out var ready ) )
		{
			return ready;
		}

		return false;
	}
}

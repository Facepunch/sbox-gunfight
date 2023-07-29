using System.Collections.Immutable;
using System.Net.Sockets;
using Facepunch.Gunfight.MainMenu;
using Facepunch.Gunfight.Proto;
using Sandbox.Menu;

namespace Facepunch.Gunfight;

public partial class GunfightLobby : IValid
{
	public static readonly List<GunfightLobby> All = new();
	
	private ILobby _lobby;

	// IValid
	bool IValid.IsValid => _lobby.Id != 0 && _lobby.IsMember;

	public GunfightLobby( ILobby lobby )
	{
		_lobby = lobby;
		
		All.Add( this );
		Sandbox.Event.Register( this );
		
		Log.Info( $"Creating GunfightLobby {lobby}" );
	}

	~GunfightLobby()
	{
		Sandbox.Event.Unregister( this );
		All.Remove( this );
	}

	public static bool operator ==( GunfightLobby a, GunfightLobby b )
	{
		return a?._lobby == b?._lobby;
	}

	public static bool operator !=( GunfightLobby a, GunfightLobby b )
	{
		return a?._lobby != b?._lobby;
	}

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

	void OnNetworkMessage( ILobby.NetworkMessage msg )
	{
		Log.Info( "OnNetworkMessage" );
		
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
	
	#region Please don't look at this
		public Task<bool> JoinAsync() => _lobby.JoinAsync();
		public Friend Owner => _lobby.Owner;
		public int MemberCount => _lobby.MemberCount;
		public IEnumerable<Friend> Members => _lobby.Members;
		public int MaxMembers => _lobby.MaxMembers;
		public string Map { get => _lobby.Map; set => _lobby.Map = value; }
		public Action<Friend, string> OnChatMessage { get => _lobby.OnChatMessage; set => _lobby.OnChatMessage = value; }
		public void SendChat( string message ) => _lobby.SendChat( message );
		public void Leave() => _lobby.Leave();
		public void SetData( string key, string value ) => _lobby.SetData( key, value );
		public Task LaunchGameAsync() => _lobby.LaunchGameAsync();
		public ImmutableDictionary<string, string> Data { get => _lobby.Data; }
	#endregion

}

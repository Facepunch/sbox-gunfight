using System.Text.Json;
using Sandbox.Menu;

namespace Facepunch.Gunfight;

public partial class GunfightLobby
{
	/// <summary>
	/// Called when we receive a net message
	/// </summary>
	/// <param name="msg"></param>
	void OnNetworkMessage( ILobby.NetworkMessage msg )
	{
		var netIdentifier = msg.Data.Read<string>();
		var json = msg.Data.Read<string>();
		var objList = Json.Deserialize<Queue<object>>( json );

		foreach ( var pair in TypeLibrary.GetMethodsWithAttribute<NetMessage.OnReceivedAttribute>()
			         .Where( x => x.Attribute.Name.Equals( netIdentifier ) ) )
		{
			pair.Method?.Invoke( null, new object[] { NetMessage.Read( this, msg.Source, objList ) } );
		}
	}
	
	/// <summary>
	/// Called in Tick
	/// </summary>
	void TickNetwork()
	{
		_lobby.ReceiveMessages( OnNetworkMessage );
	}

	// Accessor to get rid of needing to pass the lobby everywhere
	private NetMessage StartNetMessage( string identifier, int size = 256 ) => NetMessage.Write( this, identifier, size );

	/// <summary>
	/// A net message. Either it's one we're writing to people, or it's one we're reading from the lobby itself.
	/// </summary>
	public sealed class NetMessage : IDisposable
	{
		private GunfightLobby _lobby { get; set; }
		private Queue<object> _data { get; set; } = new();
		private string _identifier { get; set; }
		private int _size { get; set; }
		private Friend _sender { get; set; }
		
		public Friend Sender => _sender;
		public GunfightLobby Lobby => _lobby;

		private NetMessage( GunfightLobby lobby, string identifier = null, int size = 256 )
		{
			_size = size;
			_lobby = lobby;
			_identifier = identifier;
		}

		public static NetMessage Read( GunfightLobby lobby, Friend sender, Queue<object> objects )
		{
			var msg = new NetMessage( lobby )
			{
				_sender = sender,
				_data = objects
			};

			return msg;
		}
		
		public static NetMessage Write( GunfightLobby lobby, string identifier, int size = 256 )
		{
			var msg = new NetMessage( lobby, identifier, size );
			return msg;
		}

		public void Write( object obj )
		{
			_data.Enqueue( obj );
		}
		
		public T Read<T>()
		{
			var jsonElement = (JsonElement)_data.Dequeue();
			return (T)jsonElement.Deserialize( typeof( T ) );
		}

		public void Dispose()
		{
			var bs = ByteStream.Create( _size );
			bs.Write( _identifier );
			bs.Write( Json.Serialize( _data ) );
			_lobby.BroadcastMessage( bs );
		}
		
		[AttributeUsage( AttributeTargets.Method, AllowMultiple = false )]
		public sealed class OnReceivedAttribute : Attribute
		{
			public readonly string Name;
			public OnReceivedAttribute( string name = null ) => this.Name = name;
		}
	}
}

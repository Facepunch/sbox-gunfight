using Facepunch.Gunfight.Proto;

namespace Facepunch.Gunfight;

public partial class GunfightLobby
{
	public bool HasReadyCountdown => IsAnyoneReady;
	public float ReadyCountdown => TimeUntilGameStart;
	
	private TimeUntil TimeUntilGameStart = 0;
	private bool IsAnyoneReady => ReadyStates.Any( x => x.Value == true );
	
	public Action<Friend, bool> OnReadyStateChanged { get; set; }
	
	/// <summary>
	/// Sets our ready up state.
	/// </summary>
	/// <param name="isReady"></param>
	public void SetReady( bool isReady )
	{
		using ( var msg = StartNetMessage( "SetReady" ) )
		{
			msg.Write( isReady );
		}
	}

	/// <summary>
	/// Am I ready?
	/// </summary>
	public bool IsLocalPlayerReady => IsReady( new Friend( Game.SteamId ) );

	public override bool Equals( object obj )
	{
		return _lobby.Equals( obj );
	}

	private Dictionary<Friend, bool> ReadyStates = new();
	void OnReady( Friend friend, bool readyState )
	{
		ReadyStates[friend] = readyState;

		if ( IsAnyoneReady )
		{
			TimeUntilGameStart = 12;
		}

		OnReadyStateChanged?.Invoke( friend, readyState );
	}

	public bool IsReady( Friend friend )
	{
		if ( ReadyStates.TryGetValue( friend, out var ready ) )
		{
			return ready;
		}

		return false;
	}

	private bool isStarting;
	async Task TickReadySystem()
	{
		if ( IsAnyoneReady && !isStarting )
		{
			if ( TimeUntilGameStart )
			{
				isStarting = true;
				await _lobby.LaunchGameAsync();
			}
		}
	}

	[NetMessage.OnReceived( "SetReady" )]
	public static void NetReceive_SetReady( NetMessage msg )
	{
		var sender = msg.Sender;
		var state = msg.Read<bool>();

		msg.Lobby.OnReady( sender, state );	
	}
}

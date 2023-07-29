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
}

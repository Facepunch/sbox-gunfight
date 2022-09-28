using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Facepunch.Gunfight;

public class Location : Panel
{
	public Label LocationName;
	public Location()
	{
		LocationName = Add.Label( "Penis Town", "local" );
	}

	public override void Tick()
	{
		var player = GunfightCamera.Target;
		if ( player == null ) return;

		LocationName.SetText( $"{player.PlayerLocation}" );
		// TODO - Setup
	}
}

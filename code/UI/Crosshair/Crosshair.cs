using Sandbox.UI;
using System.Threading;

namespace Facepunch.Gunfight.UI;

public class Crosshair : Panel
{
	public static Crosshair Instance;

	public Crosshair()
	{
		Instance = this;

		for ( int i = 0; i < 5; i++ )
		{
			var p = Add.Panel( "element" );
			p.AddClass( $"el{i}" );
		}

		Style.PixelSnap = 0;
	}

	public override void Tick()
	{
		var wpn = GunfightCamera.Target?.CurrentWeapon;
		if ( !wpn.IsValid() )
		{
			return;
		}

		SetClass( "fire", wpn.TimeSincePrimaryAttack < 0.2f );
	}
}

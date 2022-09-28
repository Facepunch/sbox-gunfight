namespace Facepunch.Gunfight;

internal class GunfightPlayerCamera : GunfightCamera
{
	public override void Update()
	{
		if ( Local.Pawn is GunfightPlayer player )
			Target = player;

		base.Update();
	}
}

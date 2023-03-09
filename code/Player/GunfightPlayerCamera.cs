namespace Facepunch.Gunfight;

internal class GunfightPlayerCamera : GunfightCamera
{
	public override void Update()
	{
		if ( Game.LocalPawn is GunfightPlayer player )
			Target = player;

		base.Update();
	}
}

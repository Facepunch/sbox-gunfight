using Sandbox.UI;

namespace Facepunch.Gunfight.ScreenShake;

public class Jump : CameraModifier
{
	float Pitch = 20f;
	float Time = 0.5f;

	TimeSince lifeTime = 0;

	public Jump( float pitch = 5f, float time = 0.5f )
	{
		Time = time;
		Pitch = pitch;
	}

	public override bool Update( ref CameraSetup cam )
	{
		var pl = GunfightCamera.Target;
		var ctrl = pl.Controller as PlayerController;
		if ( ctrl == null ) return false;

		cam.Rotation *= Rotation.From( Easing.BounceInOut( ctrl.JumpWindup.Fraction ) * Pitch, 0, 0 );
		var delta = ((float)ctrl.TimeSinceJumped).LerpInverse( 0, Time / 2, true );

		if ( !ctrl.GroundEntity.IsValid() )
		{
			cam.Rotation *= Rotation.From( delta * -Pitch, 0, 0 );
		}

		return lifeTime < Time;
	}
}

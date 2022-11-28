namespace Facepunch.Gunfight;

public class PlayerTransition : CameraModifier
{
	TimeUntil Completed = 1;
	Vector3 StartPos;

	public PlayerTransition( Vector3 startPos )
	{
		StartPos = startPos;
	}

	protected Rotation LookAt( Vector3 targetPosition, Vector3 position )
	{
		var targetDelta = (targetPosition - position);
		var direction = targetDelta.Normal;

		return Rotation.From( new Angles(
			((float)Math.Asin( direction.z )).RadianToDegree() * -1.0f,
			((float)Math.Atan2( direction.y, direction.x )).RadianToDegree(),
			0.0f ) );
	}

	public override bool Update( ref CameraSetup cam )
	{
		var delta = Easing.EaseIn( Completed.Fraction );
		var pawn = GunfightCamera.Target;
		var targetPos = pawn.EyePosition;

		cam.Position = StartPos.LerpTo( targetPos, delta );
		cam.Viewer = null;
		cam.Rotation = LookAt( targetPos, cam.Position );

		return !delta.AlmostEqual( 1f );
	}
}

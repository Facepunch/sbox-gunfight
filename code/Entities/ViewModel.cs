namespace Facepunch.Gunfight;

partial class ViewModel : BaseViewModel
{
	float walkBob = 0;

	public override void PostCameraSetup( ref CameraSetup camSetup )
	{
		base.PostCameraSetup( ref camSetup );

		AddCameraEffects( ref camSetup );
	}

	protected Rotation NewRotation;

	protected Vector3 Acceleration;
	private void AddCameraEffects( ref CameraSetup camSetup )
	{
		if ( Local.Pawn.LifeState == LifeState.Dead )
			return;

		//
		// Bob up and down based on our walk movement
		//
		var speed = Owner.Velocity.Length.LerpInverse( 0, 400 );
		var left = camSetup.Rotation.Left;
		var up = camSetup.Rotation.Up;

		if ( Owner.GroundEntity != null )
			walkBob += Time.Delta * 25.0f * speed;

		Position += up * MathF.Sin( walkBob ) * speed * -1;
		Position += left * MathF.Sin( walkBob * 0.5f ) * speed * -0.5f;	

		var mouseX = Input.MouseDelta.x * Time.Delta * 5f;
		var mouseY = Input.MouseDelta.y * Time.Delta * 5f;

		Acceleration += Vector3.Left * mouseX * 1f;
		Acceleration += Vector3.Up * mouseY * 2f;

		var rotationX = Rotation.FromAxis( Vector3.Right, Acceleration.z );
		var rotationY = Rotation.FromAxis( Vector3.Up, Acceleration.y );
		var targetRotation = rotationX * rotationY;

		NewRotation = Rotation.Slerp( NewRotation, targetRotation, Time.Delta * 10f );
		LocalRotation *= NewRotation;

		var uitx = new Sandbox.UI.PanelTransform();
		uitx.AddTranslateY( MathF.Sin( walkBob * 1.0f ) * speed * -4.0f );
		uitx.AddTranslateX( MathF.Sin( walkBob * 0.5f ) * speed * -3.0f );

		HudRootPanel.Current.Style.Transform = uitx;

		Acceleration = ApplyDamping( Acceleration, 50f );
	}

	private Vector3 ApplyDamping( Vector3 value, float damping )
	{
		var magnitude = value.Length;

		if ( magnitude != 0 )
		{
			var drop = magnitude * damping * Time.Delta;
			value *= Math.Max( magnitude - drop, 0 ) / magnitude;
		}

		return value;
	}
}

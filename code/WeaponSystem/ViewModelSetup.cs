namespace Facepunch.Gunfight;

public struct ViewModelSetup
{
	public ViewModelSetup() {}

	//// General
	public float OverallWeight { get; set; } = .5f;
	public float WeightReturnForce { get; set; } = 400f;
	public float WeightDamping { get; set; } = 18f;
	public float AccelerationDamping { get; set; } = 0.1f;
	public float VelocityScale { get; set; } = 10f;
	public float RotationalPivotForce { get; set; } = 10f;
	public float RotationalScale { get; set; } = 0.5f;


	//// Walking & Bob
	public Vector3 WalkCycleOffset { get; set; } = new( 0, -50f, 50f );
	public Vector2 BobAmount { get; set; } = new( 50f, 50f );

	//// Global
	public float GlobalLerpPower { get; set; } = 10f;
	public Vector3 GlobalPositionOffset { get; set; } = new( 0, -1f, 0 );
	public Angles GlobalAngleOffset { get; set; } = new( 0, 0, 0 );

	//// Aiming
	public Vector3 AimPositionOffset { get; set; } = new( -2, 2.67f, 1.9f );
	public Angles AimAngleOffset { get; set; } = new( 0, 0, 0 );

	//// Crouching
	public Vector3 CrouchPositionOffset { get; set; } = new( -80f, -50f, 15f );
	public Angles CrouchAngleOffset { get; set; } = new( 2f, 0f, -15f );

	//// Avoidance
	/// <summary>
	/// The max position offset when avoidance comes into play.
	/// Avoidance is when something is in the way of the weapon.
	/// </summary>
	public Vector3 AvoidancePositionOffset { get; set; } = new( -15f, 0, 0 );

	/// <summary>
	/// The max angle offset when avoidance comes into play.
	/// Avoidance is when something is in the way of the weapon.
	/// </summary>
	public Angles AvoidanceAngleOffset { get; set; } = new( 2, 0, -25f );

	//// Sprinting
	public Vector3 SprintPositionOffset { get; set; } = new( 0, 0, 0 );
	public Angles SprintAngleOffset { get; set; } = new( 0, 0, 0 );
}

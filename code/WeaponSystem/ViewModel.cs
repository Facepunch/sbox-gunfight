using Sandbox;
using System;

namespace Facepunch.Gunfight;

// TODO - Clean all this up, it's a fucking mess.
public partial class ViewModel : BaseViewModel
{
	public BaseWeapon Weapon { get; set; }
	protected WeaponDefinition WeaponDef => Weapon?.WeaponDefinition;
	protected ViewModelSetup Setup => WeaponDef?.ViewModelSetup ?? default;

	// Data
	float MouseScale => Setup.OverallWeight;
	float ReturnForce => Setup.WeightReturnForce;
	float Damping => Setup.WeightDamping;
	float AccelDamping => Setup.AccelerationDamping; 
	float PivotForce => Setup.RotationalPivotForce;
	float VelocityScale => Setup.VelocityScale;
	float RotationScale => Setup.RotationalScale;
	Vector3 WalkCycleOffsets => Setup.WalkCycleOffset;
	float ForwardBobbing => Setup.BobAmount.x;
	float SideWalkOffset => Setup.BobAmount.y;
	Vector3 GlobalPositionOffset => Setup.GlobalPositionOffset;
	Angles GlobalAngleOffset => Setup.GlobalAngleOffset;
	Vector3 CrouchPositionOffset => Setup.CrouchPositionOffset;
	Angles CrouchAnglesOffset => Setup.CrouchAngleOffset;
	Angles AvoidanceAngleOffset => Setup.AvoidanceAngleOffset;
	Vector3 AvoidancePositionOffset => Setup.AvoidancePositionOffset;
	Angles SprintAngleOffset => Setup.SprintAngleOffset;
	Vector3 SprintPositionOffset => Setup.SprintPositionOffset;

	// Utility
	float DeltaTime => Time.Delta;

	// Fields
	Vector3 SmoothedVelocity;
	Vector3 velocity;
	Vector3 acceleration;

	float VelocityClamp => 3f;
	float walkBob = 0;
	float upDownOffset = 0;
	float avoidance = 0;
	float burstSprintLerp = 0;
	float sprintLerp = 0;
	float aimLerp = 0;
	float crouchLerp = 0;

	public override void PostCameraSetup( ref CameraSetup camSetup )
	{
		AddCameraEffects( ref camSetup );
	}


	protected float MouseDeltaLerpX;
	protected float MouseDeltaLerpY;
	private void AddCameraEffects( ref CameraSetup camSetup )
	{
		if ( !Owner.IsValid() )
		{
			Delete();
			return;
		}

		SmoothedVelocity += (Owner.Velocity - SmoothedVelocity) * 5f * DeltaTime;

		var speed = Owner.Velocity.Length.LerpInverse( 0, 750 );
		var bobSpeed = SmoothedVelocity.Length.LerpInverse( -100, 500 );
		var left = camSetup.Rotation.Left;
		var up = camSetup.Rotation.Up;
		var forward = camSetup.Rotation.Forward;
		var owner = Owner as Player;
		var controller = owner.Controller as PlayerController;
		var avoidanceTrace = Trace.Ray( camSetup.Position, camSetup.Position + forward * 50f )
						.UseHitboxes()
						.Ignore( Owner )
						.Ignore( this )
						.Run();

		var sprint = controller.IsSprinting;
		var burstSprint = false;
		var aim = controller.IsAiming;
		var crouched = controller?.Duck?.IsActive ?? false;

		var avoidanceVal = avoidanceTrace.Hit ? (1f - avoidanceTrace.Fraction) : 0;
		avoidanceVal *= 1 - ( aimLerp * 0.8f );

		LerpTowards( ref avoidance, avoidanceVal, 10f );
		LerpTowards( ref sprintLerp, sprint && !burstSprint ? 1 : 0, 10f );
		LerpTowards( ref burstSprintLerp, burstSprint ? 1 : 0, 8f );

		LerpTowards( ref aimLerp, aim && !sprint && !burstSprint ? 1 : 0, 7f );
		LerpTowards( ref crouchLerp, crouched && !aim ? 1 : 0, 7f );

		bobSpeed *= 1 - sprintLerp * 0.25f;
		bobSpeed *= 1 - burstSprintLerp * 0.15f;

		if ( Owner.GroundEntity != null && controller is not null /*&& !controller.Slide.IsActive*/ )
		{
			walkBob += DeltaTime * 30.0f * bobSpeed;
		}

		if ( Owner.Velocity.Length < 60 )
		{
			var step = MathF.Round( walkBob / 90 );

			walkBob += (step * 90 - walkBob) * 10f * DeltaTime;
		}

		walkBob %= 360;

		var mouseDeltaX = -Input.MouseDelta.x * DeltaTime * MouseScale;
		var mouseDeltaY = -Input.MouseDelta.y * DeltaTime * MouseScale;

		acceleration += Vector3.Left * mouseDeltaX * 0.5f * (1f - aimLerp * 2f);
		acceleration += Vector3.Up * mouseDeltaY * (1f - aimLerp * 2f);
		acceleration += -velocity * ReturnForce * DeltaTime;

		// Apply horizontal offsets based on walking direction
		var horizontalForwardBob = ( WalkCycle( 0.5f, 3f ) * speed * WalkCycleOffsets.x * ( 1 - aimLerp ) ) * DeltaTime;

		acceleration += forward.WithZ( 0 ).Normal.Dot( Owner.Velocity.Normal ) * Vector3.Forward * ForwardBobbing * horizontalForwardBob;

		// Apply left bobbing and up/down bobbing
		acceleration += Vector3.Left * WalkCycle( 0.5f, 2f ) * speed * WalkCycleOffsets.y * (1 + sprintLerp) * (1 - aimLerp) * DeltaTime;
		acceleration += Vector3.Up * WalkCycle( 0.5f, 2f, true ) * speed * WalkCycleOffsets.z * (1 - aimLerp) * DeltaTime;

		acceleration += left.WithZ( 0 ).Normal.Dot( Owner.Velocity.Normal ) * Vector3.Left * speed * SideWalkOffset * DeltaTime * (1 - aimLerp);

		velocity += acceleration * DeltaTime;

		ApplyDamping( ref acceleration, AccelDamping );
		ApplyDamping( ref velocity, Damping * (1 + aimLerp) );

		velocity = velocity.Normal * Math.Clamp( velocity.Length, 0, VelocityClamp );

		Rotation desiredRotation = Local.Pawn.EyeRotation;
		desiredRotation *= Rotation.FromAxis( Vector3.Up, velocity.y * RotationScale * (1 - aimLerp) );
		desiredRotation *= Rotation.FromAxis( Vector3.Forward, -velocity.y * RotationScale * (1 - aimLerp) );
		desiredRotation *= Rotation.FromAxis( Vector3.Right, velocity.z * RotationScale * (1 - aimLerp) );

		//Rotation = desiredRotation;

		Transform aimPointW = GetAttachment( "aim", true ) ?? new();
		Transform aimPointL = GetAttachment( "aim", false ) ?? new();

		var eyePos = camSetup.Position;
		var gunTrPos = aimPointW.Position;

		//DebugOverlay.Sphere( aimPointW.Position, 1f, Color.Green );

		var angleDiff = aimPointW.Rotation.Angles();

		var diff = aimPointW.Position - CurrentView.Position;
		diff += CurrentView.Rotation.Forward * -15f;

		if ( aim )
		{
			Position -= diff;
			//Rotation = aimPointW.Rotation;
		}
		else
		{
			Position = camSetup.Position;
			Rotation = camSetup.Rotation;

			// Global
			Rotation *= Rotation.From( GlobalAngleOffset );
			Position += forward * (velocity.x * VelocityScale + GlobalPositionOffset.x);
			Position += left * (velocity.y * VelocityScale + GlobalPositionOffset.y);
			Position += up * (velocity.z * VelocityScale + GlobalPositionOffset.z + upDownOffset);

			Position += (desiredRotation.Forward - Owner.EyeRotation.Forward) * -PivotForce;

			// Crouching
			Rotation *= Rotation.From( CrouchAnglesOffset * crouchLerp );
			ApplyPositionOffset( CrouchPositionOffset, crouchLerp, camSetup );

			// Avoidance
			Rotation *= Rotation.From( AvoidanceAngleOffset * avoidance );
			ApplyPositionOffset( AvoidancePositionOffset, avoidance, camSetup );
			//Position += forward * avoidance * -5f;

			// Sprinting
			Rotation *= Rotation.From( SprintAngleOffset * sprintLerp );
			ApplyPositionOffset( SprintPositionOffset, sprintLerp, camSetup );
		}
	}

	protected void ApplyPositionOffset( Vector3 offset, float delta, CameraSetup camSetup )
	{
		var left = camSetup.Rotation.Left;
		var up = camSetup.Rotation.Up;
		var forward = camSetup.Rotation.Forward;

		Position += forward * offset.x * delta;
		Position += left * offset.y * delta;
		Position += up * offset.z * delta;
	}

	private float WalkCycle( float speed, float power, bool abs = false )
	{
		var sin = MathF.Sin( walkBob * speed );
		var sign = Math.Sign( sin );

		if ( abs )
		{
			sign = 1;
		}

		return MathF.Pow( sin, power ) * sign;
	}

	public void ApplyImpulse( Vector3 impulse )
	{
		acceleration += impulse;
	}

	private void LerpTowards( ref float value, float desired, float speed )
	{
		var delta = (desired - value) * speed * DeltaTime;
		var deltaAbs = MathF.Min( MathF.Abs( delta ), MathF.Abs( desired - value ) ) * MathF.Sign( delta );

		if ( MathF.Abs( desired - value ) < 0.001f )
		{
			value = desired;

			return;
		}

		value += deltaAbs;
	}

	private void ApplyDamping( ref Vector3 value, float damping )
	{
		var magnitude = value.Length;

		if ( magnitude != 0 )
		{
			var drop = magnitude * damping * DeltaTime;
			value *= Math.Max( magnitude - drop, 0 ) / magnitude;
		}
	}
}

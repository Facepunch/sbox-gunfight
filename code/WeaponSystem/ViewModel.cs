using Sandbox;
using System;

namespace Facepunch.Gunfight;

// TODO - Clean all this up, it's a fucking mess.
public partial class ViewModel : BaseViewModel
{
	public BaseWeapon Weapon { get; set; }
	protected WeaponDefinition WeaponDef => Weapon?.WeaponDefinition;
	protected ViewModelSetup Setup => WeaponDef?.ViewModelSetup ?? default;

	float TimeSincePrimaryAttack => ( Weapon as GunfightWeapon ).TimeSincePrimaryAttack;
	float PrimaryAttackReturn => 0.2f;

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
	Vector3 SlidePositionOffset => Setup.SlidePositionOffset;
	Angles SlideAngleOffset => Setup.SlideAngleOffset;
	Angles AvoidanceAngleOffset => Setup.AvoidanceAngleOffset;
	Vector3 AvoidancePositionOffset => Setup.AvoidancePositionOffset;
	Angles SprintAngleOffset => Setup.SprintAngleOffset;
	Vector3 SprintPositionOffset => Setup.SprintPositionOffset;
	Vector3 BurstSprintPositionOffset => Setup.GetSprintPosOffset( true );
	Angles BurstSprintAngleOffset => Setup.GetSprintAngleOffset( true );

	// Utility
	float DeltaTime => Time.Delta;

	// Fields
	Vector3 SmoothedVelocity;
	Vector3 velocity;
	Vector3 acceleration;
	Vector2 LerpRecoil;

	float VelocityClamp => 20f;
	float walkBob = 0;
	float upDownOffset = 0;
	float avoidance = 0;
	float burstSprintLerp = 0;
	float sprintLerp = 0;
	float aimLerp = 0;
	float crouchLerp = 0;
	float airLerp = 0;
	float slideLerp = 0;
	float sideLerp = 0;
	float speedLerp = 0;
	float vaultLerp = 0;

	protected float MouseDeltaLerpX;
	protected float MouseDeltaLerpY;

	Vector3 positionOffsetTarget = Vector3.Zero;
	Rotation rotationOffsetTarget = Rotation.Identity;

	Vector3 realPositionOffset;
	Rotation realRotationOffset;

	public Vector3 GetAimOffset()
	{
		return Setup.AimPositionOffset;
	}

	public Angles GetAimAngle()
	{
		return Setup.AimAngleOffset;
	}

	public override void PlaceViewmodel()
	{
		if ( !Owner.IsValid() )
		{
			Delete();
			return;
		}

		var owner = Owner as GunfightPlayer;
		var controller = owner.Controller;

		if ( controller == null )
			return;

		var frac = controller.IsAiming ? 1 : 0;
		LerpTowards( ref aimLerp, frac, controller.IsAiming ? 30f : 10f );

		SmoothedVelocity += (Owner.Velocity - SmoothedVelocity) * 5f * DeltaTime;

		var isGrounded = Owner.GroundEntity != null;
		var weapon = Weapon as GunfightWeapon;
		var speed = Owner.Velocity.Length.LerpInverse( 0, 750 );
		var sideSpeed = Owner.Velocity.Length.LerpInverse( 0, 350 );
		var bobSpeed = SmoothedVelocity.Length.LerpInverse( -250, 700 );
		var left = Camera.Rotation.Left;
		var up = Camera.Rotation.Up;
		var forward = Camera.Rotation.Forward;
		var avoidanceTrace = Trace.Ray( Camera.Position, Camera.Position + forward * 50f )
					.WithoutTags( "trigger" )
					.Ignore( Owner )
					.Ignore( this )
					.Run();

		var sprint = controller.IsSprinting;
		var burstSprint = false;
		var aim = controller.IsAiming;
		var crouched = controller?.Duck?.IsActive ?? false;
		var sliding = controller?.Slide?.IsActive ?? false;
		var vaulting = controller?.Vault?.IsActive ?? false;

		var avoidanceVal = avoidanceTrace.Hit ? (1f - avoidanceTrace.Fraction) : 0;
		avoidanceVal *= 1 - ( aimLerp * 0.8f );

		LerpTowards( ref avoidance, avoidanceVal, 10f );
		LerpTowards( ref sprintLerp, sprint && !burstSprint && !sliding ? 1 : 0, 10f );
		LerpTowards( ref burstSprintLerp, burstSprint && !sliding ? 1 : 0, 8f );

		//LerpTowards( ref aimLerp, aim && !sprint && !burstSprint ? 1 : 0, 30f );
		LerpTowards( ref crouchLerp, crouched && !aim && !sliding ? 1 : 0, 7f );
		LerpTowards( ref slideLerp, sliding ? TimeSincePrimaryAttack.Remap( 0, 0.2f, 0, 1 ).Clamp( 0, 1 ) : 0, 7f );
		LerpTowards( ref airLerp, ( isGrounded ? 0 : 1 ) * ( 1 - aimLerp ), 10f );
		LerpTowards( ref speedLerp, ( aim || sliding || sprint ) ? 0.0f : speed, 10f );
		LerpTowards( ref vaultLerp, ( vaulting ) ? 1.0f : 0.0f , 10f );

		var leftAmt = left.WithZ( 0 ).Normal.Dot( Owner.Velocity.Normal );
		LerpTowards( ref sideLerp, leftAmt * ( 1 - aimLerp ), 5f );

		bobSpeed *= 1 - sprintLerp * 0.25f;
		bobSpeed *= 1 - burstSprintLerp * 2f;

		if ( isGrounded && !sliding && controller is not null /*&& !controller.Slide.IsActive*/ )
		{
			walkBob += DeltaTime * 30.0f * bobSpeed;
		}

		walkBob %= 360;

		var mouseDeltaX = -Input.MouseDelta.x * DeltaTime * MouseScale;
		var mouseDeltaY = -Input.MouseDelta.y * DeltaTime * MouseScale;

		acceleration += Vector3.Left * mouseDeltaX * 0.5f * (1f - aimLerp);
		acceleration += Vector3.Up * mouseDeltaY * (1f - aimLerp);
		acceleration += -velocity * ReturnForce * DeltaTime;

		// Apply horizontal offsets based on walking direction
		var horizontalForwardBob = WalkCycle( 0.5f, 3f ) * speed * WalkCycleOffsets.x * DeltaTime;

		acceleration += forward.WithZ( 0 ).Normal.Dot( Owner.Velocity.Normal ) * Vector3.Forward * ForwardBobbing * horizontalForwardBob;

		// Apply left bobbing and up/down bobbing
		acceleration += Vector3.Left * WalkCycle( 0.5f, 2f ) * speed * WalkCycleOffsets.y * (1 + sprintLerp) * (1 - aimLerp) * DeltaTime;
		acceleration += Vector3.Up * WalkCycle( 0.5f, 2f, true ) * speed * WalkCycleOffsets.z * (1 - aimLerp) * DeltaTime;

		acceleration += left.WithZ( 0 ).Normal.Dot( Owner.Velocity.Normal ) * Vector3.Left * speed * SideWalkOffset * DeltaTime * (1 - aimLerp);

		velocity += acceleration * DeltaTime;

		ApplyDamping( ref acceleration, AccelDamping );
		ApplyDamping( ref velocity, Damping * (1 + aimLerp) );

		velocity = velocity.Normal * Math.Clamp( velocity.Length, 0, VelocityClamp );

		Position = Camera.Position;
		Rotation = Camera.Rotation;

		positionOffsetTarget = Vector3.Zero;
		rotationOffsetTarget = Rotation.Identity;

		{
			// Global
			rotationOffsetTarget *= Rotation.From( GlobalAngleOffset );
			positionOffsetTarget += forward * (velocity.x * VelocityScale + GlobalPositionOffset.x);
			positionOffsetTarget += left * (velocity.y * VelocityScale + GlobalPositionOffset.y);
			positionOffsetTarget += up * (velocity.z * VelocityScale + GlobalPositionOffset.z + upDownOffset);

			// Crouching
			rotationOffsetTarget *= Rotation.From( CrouchAnglesOffset * crouchLerp );
			ApplyPositionOffset( CrouchPositionOffset, crouchLerp );

			// Avoidance
			rotationOffsetTarget *= Rotation.From( AvoidanceAngleOffset * avoidance );
			ApplyPositionOffset( AvoidancePositionOffset, avoidance );
			//Position += forward * avoidance * -5f;

			// Sprinting
			rotationOffsetTarget *= Rotation.From( SprintAngleOffset * sprintLerp );
			ApplyPositionOffset( SprintPositionOffset, sprintLerp );

			// Sprinting
			rotationOffsetTarget *= Rotation.From( BurstSprintAngleOffset * burstSprintLerp );
			ApplyPositionOffset( BurstSprintPositionOffset, burstSprintLerp );

			// Sprinting cycle
			float cycle = Time.Now * 10.0f;
			Camera.Rotation *= Rotation.From( 
				new Angles( 
					MathF.Abs( MathF.Sin( cycle ) * 2.0f ),
					MathF.Cos( cycle ), 
					0 
				) * sprintLerp * 0.35f );

			// Apply the same offset as above for a nicer sprint bob
			float sprintBob = MathF.Pow( MathF.Sin( cycle ) * 0.5f + 0.5f, 2.0f );
			float sprintBob2 = MathF.Pow( MathF.Cos( cycle ) * 0.5f + 0.5f, 3.0f );
			rotationOffsetTarget *= Rotation.From( SprintAngleOffset * sprintLerp * sprintBob * 0.2f );
			ApplyPositionOffset( -SprintPositionOffset * sprintBob2 * 0.3f, sprintLerp );

			Camera.FieldOfView += 5f * sprintLerp;

			// Vertical Look
			var lookDownDot = Camera.Rotation.Forward.Dot( Vector3.Down );
			if ( MathF.Abs( lookDownDot ) > 0.5f )
			{
				var offset = lookDownDot < 0 ? -1 : 1;
				var f = lookDownDot - 0.5f * offset;
				var positionScale = f.Remap( 0f, 0.5f, 0f, 10f ) * ( 1 - aimLerp );

				rotationOffsetTarget *= Rotation.From( new Angles( -positionScale, 0, -positionScale ) );
			}
		}

		// Vaulting
		rotationOffsetTarget *= Rotation.From( new Angles(40,0,0) * vaultLerp );

		// Sliding
		var slideRotationOffset = Rotation.From( Angles.Zero.WithRoll( leftAmt ) * slideLerp * -15.0f );

		if( !aim )
		{
			rotationOffsetTarget *= Rotation.From( SlideAngleOffset * slideLerp );
			ApplyPositionOffset( SlidePositionOffset, slideLerp );
		}
		
		rotationOffsetTarget *= slideRotationOffset;
		
		Camera.Rotation *= slideRotationOffset;
		Camera.FieldOfView += 5f * slideLerp;

		// Recoil
		LerpRecoil = LerpRecoil.LerpTo( weapon.WeaponSpreadRecoil, Time.Delta * 5f );
		rotationOffsetTarget *= Rotation.From( new Angles( -LerpRecoil.y, -LerpRecoil.x, 0 ) );

		// In air
		rotationOffsetTarget *= Rotation.From( new Angles( -2, 0, -7f ) * airLerp );
		ApplyPositionOffset( new Vector3( 0, 0, 0.5f ), airLerp );

		// Aim
		rotationOffsetTarget *= Rotation.From( GetAimAngle() * aimLerp );
		ApplyPositionOffset( GetAimOffset(), aimLerp );

		// Move your view a bit down as you move like in CSGO
		positionOffsetTarget += new Vector3( 0, 0, -2.5f ) * speedLerp;

		rotationOffsetTarget *= Rotation.From( 0f, sideLerp * -5f, sideLerp * -5f );

		realRotationOffset = rotationOffsetTarget;
		realPositionOffset = positionOffsetTarget;

		Rotation *= realRotationOffset;
		Position += realPositionOffset;

		Camera.FieldOfView -= 10f * aimLerp;
		Camera.Main.SetViewModelCamera( 90f, 1, 2048 );
	}

	protected void ApplyPositionOffset( Vector3 offset, float delta )
	{
		var left = Camera.Rotation.Left;
		var up = Camera.Rotation.Up;
		var forward = Camera.Rotation.Forward;

		positionOffsetTarget += forward * offset.x * delta;
		positionOffsetTarget += left * offset.y * delta;
		positionOffsetTarget += up * offset.z * delta;
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

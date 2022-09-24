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

	// Utility
	float DeltaTime => Time.Delta;

	// Fields
	Vector3 SmoothedVelocity;
	Vector3 velocity;
	Vector3 acceleration;
	Vector2 LerpRecoil;

	float VelocityClamp => 3f;
	float walkBob = 0;
	float upDownOffset = 0;
	float avoidance = 0;
	float burstSprintLerp = 0;
	float sprintLerp = 0;
	float aimLerp = 0;
	float crouchLerp = 0;
	float airLerp = 0;
	float slideLerp = 0;

	public override void PostCameraSetup( ref CameraSetup camSetup )
	{
		AddCameraEffects( ref camSetup );
	}

	protected float MouseDeltaLerpX;
	protected float MouseDeltaLerpY;

	Vector3 positionOffsetTarget = Vector3.Zero;
	Rotation rotationOffsetTarget = Rotation.Identity;

	Vector3 realPositionOffset;
	Rotation realRotationOffset;

	public Transform GetAimAttachment( bool worldspace = true )
	{
		var wpn = Weapon as GunfightWeapon;

		foreach( var attachment in wpn.Attachments )
		{
			if ( !string.IsNullOrEmpty( attachment.AimAttachment ) )
			{
				var style = attachment.AimAttachmentStyle;
				if ( style == AimAttachmentStyle.OnViewModel )
					return GetAttachment( attachment.AimAttachment, worldspace ) ?? new();
				else
					return attachment.GetAttachment( attachment.AimAttachment, worldspace ) ?? new();
			}
		}

		return wpn.GetAttachment( "aim", worldspace ) ?? new();
	}

	private void AddCameraEffects( ref CameraSetup camSetup )
	{
		if ( !Owner.IsValid() )
		{
			Delete();
			return;
		}

		var owner = Owner as Player;
		var controller = owner.Controller as PlayerController;

		if ( controller == null )
			return;

		SmoothedVelocity += (Owner.Velocity - SmoothedVelocity) * 5f * DeltaTime;

		var isGrounded = Owner.GroundEntity != null;
		var weapon = Weapon as GunfightWeapon;
		var speed = Owner.Velocity.Length.LerpInverse( 0, 750 );
		var bobSpeed = SmoothedVelocity.Length.LerpInverse( -100, 500 );
		var left = camSetup.Rotation.Left;
		var up = camSetup.Rotation.Up;
		var forward = camSetup.Rotation.Forward;
		var avoidanceTrace = Trace.Ray( camSetup.Position, camSetup.Position + forward * 50f )
						.UseHitboxes()
						.Ignore( Owner )
						.Ignore( this )
						.Run();

		var sprint = controller.IsSprinting;
		var burstSprint = false;
		var aim = controller.IsAiming;
		var crouched = controller?.Duck?.IsActive ?? false;
		var sliding = controller?.Slide?.IsActive ?? false;

		var avoidanceVal = avoidanceTrace.Hit ? (1f - avoidanceTrace.Fraction) : 0;
		avoidanceVal *= 1 - ( aimLerp * 0.8f );

		LerpTowards( ref avoidance, avoidanceVal, 10f );
		LerpTowards( ref sprintLerp, sprint && !burstSprint ? 1 : 0, 10f );
		LerpTowards( ref burstSprintLerp, burstSprint ? 1 : 0, 8f );

		LerpTowards( ref aimLerp, aim && !sprint && !burstSprint ? 1 : 0, 14f );
		LerpTowards( ref crouchLerp, crouched && !aim && !sliding ? 1 : 0, 7f );
		LerpTowards( ref slideLerp, sliding ? TimeSincePrimaryAttack.Remap( 0, 0.2f, 0, 1 ).Clamp( 0, 1 ) : 0, 7f );
		LerpTowards( ref airLerp, isGrounded ? 0 : 1, 10f );

		bobSpeed *= 1 - sprintLerp * 0.25f;
		bobSpeed *= 1 - burstSprintLerp * 0.15f;

		if ( isGrounded && !sliding && controller is not null /*&& !controller.Slide.IsActive*/ )
		{
			walkBob += DeltaTime * 30.0f * bobSpeed;
		}

		walkBob %= 360;

		var mouseDeltaX = -Input.MouseDelta.x * DeltaTime * MouseScale;
		var mouseDeltaY = -Input.MouseDelta.y * DeltaTime * MouseScale;

		acceleration += Vector3.Left * mouseDeltaX * 0.5f * (1f - aimLerp * 2f);
		acceleration += Vector3.Up * mouseDeltaY * (1f - aimLerp * 2f);
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

		var aimPointW = GetAimAttachment( true );
		var aimPointL = GetAimAttachment( false );

		var eyePos = camSetup.Position;
		var diff = aimPointW.Position - eyePos;

		Position = camSetup.Position;
		Rotation = camSetup.Rotation;

		positionOffsetTarget = Vector3.Zero;
		rotationOffsetTarget = Rotation.Identity;

		{
			positionOffsetTarget -= diff * aimLerp;
			rotationOffsetTarget *= aimPointL.Rotation * aimLerp;
			// Global
			rotationOffsetTarget *= Rotation.From( GlobalAngleOffset );
			positionOffsetTarget += forward * (velocity.x * VelocityScale + GlobalPositionOffset.x);
			positionOffsetTarget += left * (velocity.y * VelocityScale + GlobalPositionOffset.y);
			positionOffsetTarget += up * (velocity.z * VelocityScale + GlobalPositionOffset.z + upDownOffset);

			// Crouching
			rotationOffsetTarget *= Rotation.From( CrouchAnglesOffset * crouchLerp );
			ApplyPositionOffset( CrouchPositionOffset, crouchLerp, camSetup );

			// Avoidance
			rotationOffsetTarget *= Rotation.From( AvoidanceAngleOffset * avoidance );
			ApplyPositionOffset( AvoidancePositionOffset, avoidance, camSetup );
			//Position += forward * avoidance * -5f;

			// Sprinting
			rotationOffsetTarget *= Rotation.From( SprintAngleOffset * sprintLerp );
			ApplyPositionOffset( SprintPositionOffset, sprintLerp, camSetup );

			// Vertical Look
			var lookDownDot = camSetup.Rotation.Forward.Dot( Vector3.Down );
			if ( MathF.Abs( lookDownDot ) > 0.5f )
			{
				var offset = lookDownDot < 0 ? -1 : 1;
				var f = lookDownDot - 0.5f * offset;
				var positionScale = f.Remap( 0f, 0.5f, 0f, 10f );

				rotationOffsetTarget *= Rotation.From( new Angles( -positionScale, 0, -positionScale ) );
			}
		}

		// Sliding
		rotationOffsetTarget *= Rotation.From( SlideAngleOffset * slideLerp );
		ApplyPositionOffset( SlidePositionOffset, slideLerp, camSetup );
		camSetup.Rotation *= Rotation.From( new Angles( -1f, 0, -3f ) * slideLerp );

		// Recoil
		LerpRecoil = LerpRecoil.LerpTo( weapon.WeaponSpreadRecoil, Time.Delta * 5f );
		rotationOffsetTarget *= Rotation.From( new Angles( -LerpRecoil.y, -LerpRecoil.x, 0 ) );

		// In air
		rotationOffsetTarget *= Rotation.From( new Angles( -2, 0, -7f ) * airLerp );
		ApplyPositionOffset( new Vector3( 0, 0, 0.5f ), airLerp, camSetup );

		realRotationOffset = Rotation.Lerp( realRotationOffset, rotationOffsetTarget, Time.Delta * 20f );
		realPositionOffset = realPositionOffset.LerpTo( positionOffsetTarget, Time.Delta * 20f );

		Rotation *= realRotationOffset;
		Position += realPositionOffset;

		camSetup.FieldOfView -= 10f * aimLerp;
		// TODO - Set this up in the viewmodel itself
		camSetup.ViewModel.FieldOfView = 75f;
	}

	public void Initialize()
	{
		SetupAttachments();
	}

	protected void ApplyPositionOffset( Vector3 offset, float delta, CameraSetup camSetup )
	{
		var left = camSetup.Rotation.Left;
		var up = camSetup.Rotation.Up;
		var forward = camSetup.Rotation.Forward;

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

	public void SetupAttachments()
	{
		var wpn = Weapon as GunfightWeapon;
		foreach( var attachment in wpn.Attachments )
		{
			OnAttachmentAdded( attachment );
		}
	}

	public void OnAttachmentAdded( WeaponAttachment attachment )
	{
		Log.Info( $"[ViewModel] Recognized Attachment added: {attachment}" );
		attachment.Mirror( this );
	}

	public void OnAttachmentRemoved( WeaponAttachment attachment )
	{
		Log.Info( $"[ViewModel] Recognized Attachment removed: {attachment}" );
	}
}

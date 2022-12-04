
namespace Facepunch.Gunfight;

[Library]
public partial class PlayerController : BasePlayerController
{
	[Net] public float SprintSpeed { get; set; } = 250.0f;
	[Net] public float BurstSprintSpeed { get; set; } = 300f;
	[Net] public float WalkSpeed { get; set; } = 120.0f;
	[Net] public float DefaultSpeed { get; set; } = 175.0f;
	[Net] public float Acceleration { get; set; } = 8.0f;
	[Net] public float AirAcceleration { get; set; } = 4.0f;
	[Net] public float StopSpeed { get; set; } = 100.0f;
	[Net] public float GroundAngle { get; set; } = 46.0f;
	[Net] public float StepSize { get; set; } = 16f;
	[Net] public float MaxNonJumpVelocity { get; set; } = 140.0f;
	[Net] public float BodyGirth { get; set; } = 32.0f;
	[Net] public float BodyHeight { get; set; } = 72.0f;
	[Net] public float Gravity { get; set; } = 700.0f;
	[Net] public float AirControl { get; set; } = 30.0f;

	[Net, Predicted] public TimeUntil JumpWindup { get; set; }
	[Net, Predicted] public bool JumpWinding { get; set; }
	[Net, Predicted] public TimeSince TimeSinceJumped { get; set; }

	[Net, Predicted] public bool IsAiming { get; set; }
	[Net, Predicted] public TimeUntil AimFireDelay { get; set; } = 0;
	[Net, Predicted] public bool Swimming { get; set; }
	[Net, Predicted] public bool cachedSprint { get; set; }
	[Net, Predicted] public TimeSince SinceStoppedSprinting { get; set; }

	public SlideMechanic Slide => GetMechanic<SlideMechanic>();
	public DuckMechanic Duck => GetMechanic<DuckMechanic>();
	public VaultMoveMechanic Vault => GetMechanic<VaultMoveMechanic>();
	public CoverAimMechanic CoverAim => GetMechanic<CoverAimMechanic>();

	public List<BaseMoveMechanic> Mechanics = new();
	public BaseMoveMechanic CurrentMechanic => Mechanics.FirstOrDefault( x => x.IsActive );

	public PlayerController()
	{
		SinceStoppedSprinting = -1;

		Mechanics.Add( new CoverAimMechanic( this ) );
		Mechanics.Add( new VaultMoveMechanic( this ) );
		Mechanics.Add( new SlideMechanic( this ) );
		Mechanics.Add( new DuckMechanic( this ) );
	}

	public T GetMechanic<T>() where T : BaseMoveMechanic
	{
		return Mechanics.FirstOrDefault( x => x is T ) as T;
	}

	/// <summary>
	/// This is temporary, get the hull size for the player's collision
	/// </summary>
	public override BBox GetHull()
	{
		var girth = BodyGirth * 0.5f;
		var mins = new Vector3( -girth, -girth, 0 );
		var maxs = new Vector3( +girth, +girth, BodyHeight );

		return new BBox( mins, maxs );
	}


	// Duck body height 32
	// Eye Height 64
	// Duck Eye Height 28

	protected Vector3 mins;
	protected Vector3 maxs;

	public virtual void SetBBox( Vector3 mins, Vector3 maxs )
	{
		if ( this.mins == mins && this.maxs == maxs )
			return;

		this.mins = mins;
		this.maxs = maxs;
	}

	/// <summary>
	/// Update the size of the bbox. We should really trigger some shit if this changes.
	/// </summary>
	public virtual void UpdateBBox()
	{
		var girth = BodyGirth * 0.5f;
		var mins = new Vector3( -girth, -girth, 0 ) * Pawn.Scale;
		var maxs = new Vector3( +girth, +girth, BodyHeight ) * Pawn.Scale;

		CurrentMechanic?.UpdateBBox( ref mins, ref maxs, Pawn.Scale );

		SetBBox( mins, maxs );
	}

	protected float SurfaceFriction;

	// Accessors
	protected GunfightPlayer Player => Pawn as GunfightPlayer;
	protected GunfightWeapon Weapon => Player?.ActiveChild as GunfightWeapon;

	public override void FrameSimulate()
	{
		base.FrameSimulate();
		EyeRotation = Input.Rotation;
	}

	protected void OnStoppedSprinting()
	{
		SinceStoppedSprinting = 0;
	}

	protected float GetEyeHeight()
	{
		return CurrentMechanic?.GetEyeHeight() ?? 64;
	}

	protected float GetGroundFriction()
	{
		return CurrentMechanic?.GetGroundFriction() ?? 8f;
	}

	[Net, Predicted] public TimeSince SinceBurstActivated { get; set; } = -5;
	[Net, Predicted] public TimeSince SinceBurstEnded { get; set; } = -5;
	public float BurstStaminaDuration => 2f;

	protected bool CanAim()
	{
		if ( !Weapon.IsValid() ) return false;
		if ( Weapon.WeaponDefinition.AimingDisabled ) return false;
		if ( !GroundEntity.IsValid() ) return false;
		if ( IsSprinting ) return false;
		if ( Slide.IsActive ) return false;

		return true;
	}

	public override void Simulate()
	{
		EyeLocalPosition = Vector3.Up * (GetEyeHeight() * Pawn.Scale);
		UpdateBBox();

		EyeLocalPosition += TraceOffset;
		EyeRotation = Input.Rotation;

		if ( Input.Down( InputButton.SecondaryAttack ) )
		{
			if ( !IsAiming )
			{
				IsAiming = true;
				AimFireDelay = Weapon.GetAimTime();
			}
		}
		else
		{
			IsAiming = false;
		}

		CheckLadder();
		Swimming = Pawn.WaterLevel > 0.6f;

		if ( IsSprinting && Input.Pressed( InputButton.Run ) && SinceBurstEnded > 5f )
		{
			if ( Input.Forward > 0.5f )
			{
				IsBurstSprinting = !IsBurstSprinting;

				if ( IsBurstSprinting )
					SinceBurstActivated = 0;
			}
		}

		if ( IsBurstSprinting && SinceBurstActivated > BurstStaminaDuration )
		{
			IsBurstSprinting = false;
			SinceBurstEnded = 0;
		}

		if ( Input.Pressed( InputButton.Run ) )
		{
			if ( !IsSprinting )
				IsSprinting = true;
			else if ( IsSprinting && Input.Forward < 0.5f )
				IsSprinting = false;
		}

		if ( !IsBurstSprinting && IsSprinting && Velocity.Length < 40 || Input.Forward < 0.5f )
			IsSprinting = false;

		if ( Input.Down( InputButton.PrimaryAttack ) || Input.Down( InputButton.SecondaryAttack) )
			IsSprinting = false;

		if ( !IsSprinting )
			IsBurstSprinting = false;

		//
		// Start Gravity
		//
		if ( !Swimming && !IsTouchingLadder )
		{
			Velocity -= new Vector3( 0, 0, Gravity * 0.5f ) * Time.Delta;
			Velocity += new Vector3( 0, 0, BaseVelocity.z ) * Time.Delta;

			BaseVelocity = BaseVelocity.WithZ( 0 );
		}

		if ( CanJump() && Input.Pressed( InputButton.Jump ) && !JumpWinding )
		{
			JumpWinding = true;
			JumpWindup = 0.125f;
			_ = new ScreenShake.Jump();
		}

		if ( JumpWindup && JumpWinding )
			CheckJumpButton();

		// Fricion is handled before we add in any base velocity. That way, if we are on a conveyor,
		//  we don't slow when standing still, relative to the conveyor.
		bool bStartOnGround = GroundEntity != null;
		Vector3 cachedVelocity = Velocity;
		if ( bStartOnGround )
		{
			Velocity = Velocity.WithZ( 0 );

			if ( GroundEntity != null )
			{
				ApplyFriction( GetGroundFriction() * SurfaceFriction );
			}
		}

		//
		// Work out wish velocity.. just take input, rotate it to view, clamp to -1, 1
		//
		WishVelocity = new Vector3( Input.Forward, Input.Left * ( IsSprinting ? 0.5f : 1f ), 0 );
		var inSpeed = WishVelocity.Length.Clamp( 0, 1 );
		WishVelocity *= Input.Rotation.Angles().WithPitch( 0 ).ToRotation();

		if ( !Swimming && !IsTouchingLadder )
		{
			WishVelocity = WishVelocity.WithZ( 0 );
		}

		WishVelocity = WishVelocity.Normal * inSpeed;

		var isSprintingLastFrame = cachedSprint;
		cachedSprint = IsSprinting;

		if ( isSprintingLastFrame && !IsSprinting )
		{
			// Just stopped sprinting, let's let the controller know this.
			OnStoppedSprinting();
		}

		WishVelocity *= GetWishSpeed();

		if ( SinceLastFall < FallRecoveryTime )
		{
			float sinceFall = SinceLastFall;
			var speedMult = sinceFall.Remap( 0, FallRecoveryTime, 0.2f, 1 );
			WishVelocity *= speedMult;
		}

		bool bStayOnGround = false;
		if ( Swimming )
		{
			ApplyFriction( 1 );
			WaterMove();
		}
		else if ( IsTouchingLadder )
		{
			SetTag( "climbing" );
			LadderMove();
		}
		else if ( GroundEntity != null )
		{
			bStayOnGround = true;
			WalkMove();
		}
		else
		{
			AirMove();
		}

		CategorizePosition( bStayOnGround );

		// FinishGravity
		if ( !Swimming && !IsTouchingLadder )
		{
			Velocity -= new Vector3( 0, 0, Gravity * 0.5f ) * Time.Delta;
		}


		if ( GroundEntity != null )
		{
			Velocity = Velocity.WithZ( 0 );

			if ( !bStartOnGround )
			{
				OnHitGround( cachedVelocity );
			}
		}

		if ( Debug )
		{
			DebugOverlay.Box( Position + TraceOffset, mins, maxs, Color.Red );
			DebugOverlay.Box( Position, mins, maxs, Color.Blue );

			var lineOffset = 0;
			if ( Host.IsServer ) lineOffset = 15;

			DebugOverlay.ScreenText( $"        Host: {Host.Name}", lineOffset + 0 );
			DebugOverlay.ScreenText( $"        Velocity: {Velocity}", lineOffset + 1 );
			DebugOverlay.ScreenText( $"    BaseVelocity: {BaseVelocity}", lineOffset + 2 );
			DebugOverlay.ScreenText( $"    GroundEntity: {GroundEntity} [{GroundEntity?.Velocity}]", lineOffset + 3 );
			DebugOverlay.ScreenText( $" SurfaceFriction: {SurfaceFriction}", lineOffset + 4 );
			DebugOverlay.ScreenText( $"    WishVelocity: {WishVelocity}", lineOffset + 5 );
			DebugOverlay.ScreenText( $"    Slide: {Slide?.IsActive ?? false}", lineOffset + 6 );
			DebugOverlay.ScreenText( $"    Duck: {Duck?.IsActive ?? false}", lineOffset + 7 );
		}

		if ( Host.IsServer )
		{
			if ( IsSprinting || Slide.IsActive )
			{
				var trForward = Trace.Ray( Pawn.EyePosition, Pawn.EyePosition + Pawn.EyeRotation.Forward * 50f ).WithTag( "solid" ).Radius( 5f ).Ignore( Pawn ).Run();

				if ( trForward.Entity is DoorEntity door && ( door.State == DoorEntity.DoorState.Closed || door.State == DoorEntity.DoorState.Closing ) )
				{
					door.Speed = 500f;
					door.Open( Pawn );
				
					SendDoorSlamEffect( To.Single( Pawn.Client ) );

					_ = ResetDoor( door );
				}
			}
		}


		SimulateMechanics();
	}

	static TimeSince LastDoorSlam = 5f;
	[ClientRpc]
	public static void SendDoorSlamEffect()
	{
		if ( LastDoorSlam < 0.5f ) return;

		LastDoorSlam = 0;

		new ScreenShake.Pitch( 0.5f, 3f * 10f );
	}

	protected async Task ResetDoor( DoorEntity door )
	{
		await GameTask.DelaySeconds( 0.2f );
		door.Speed = 100f;
	}

	protected void SimulateMechanics()
	{
		foreach ( var mechanic in Mechanics )
		{
			if ( mechanic.IsActive || mechanic.AlwaysSimulate )
			{
				mechanic.PreSimulate();
			}
		}

		var current = CurrentMechanic;
		// Try to find a mechanic to activate
		if ( current == null )
		{
			current = Mechanics.FirstOrDefault( x => x.Try() );
		}

		bool hasSimulated = false;
		if ( current != null && current.TakesOverControl )
		{
			current.Simulate();
			hasSimulated = true;
		}

		foreach ( var mechanic in Mechanics )
		{
			if ( mechanic.IsActive || mechanic.AlwaysSimulate )
			{
				if ( !hasSimulated )
				{
					mechanic.Simulate();
				}

				mechanic.PostSimulate();
			}
		}
	}

	[Net, Predicted] public TimeSince SinceLastHitGround { get; set; }
	[Net, Predicted] public TimeSince SinceLastFall { get; set; }
	[Net] public float FallRecoveryTime { get; set; } = 1.2f;

	private void OnHitGround( Vector3 velocity )
	{
		var velocityLength = MathF.Abs( velocity.z ).LerpInverse( 0, 700f, true );
		var smallFall = velocityLength < 0.7f;

		SinceLastHitGround = 0;
		
		Pawn.PlaySound( "sounds/player/foley/gear/player.walk.gear.sound" );

		if ( smallFall )
		{
			new ScreenShake.Pitch( 0.5f, 3f * velocityLength );
			return;
		}

		SinceLastFall = 0;

		// Play the heavy land sound, on top of the light one.
		Pawn.PlaySound( "sounds/player/foley/gear/player.heavy_land.gear.sound" );

		new ScreenShake.Pitch( 1f, 10f * velocityLength );
	}

	[Net, Predicted]
	protected bool _IsSprinting { get; set; }

	public bool IsSprinting { get => _IsSprinting; protected set { if ( _IsSprinting && !value ) SinceSprintStopped = 0; _IsSprinting = value; } }
	
	[Net, Predicted] public TimeSince SinceSprintStopped { get; set; }
	[Net, Predicted] public bool IsBurstSprinting { get; protected set; }

	public virtual float GetWishSpeed()
	{
		var mechanicSpeed = CurrentMechanic?.GetWishSpeed();
		if ( mechanicSpeed != null ) return mechanicSpeed.Value;

		if ( IsBurstSprinting ) return BurstSprintSpeed;
		if ( IsSprinting ) return SprintSpeed;
		if ( Input.Down( InputButton.Walk ) ) return WalkSpeed;

		return DefaultSpeed;
	}

	public virtual void WalkMove()
	{
		var wishdir = WishVelocity.Normal;
		var wishspeed = WishVelocity.Length;
		
		WishVelocity = WishVelocity.WithZ( 0 );
		WishVelocity = WishVelocity.Normal * wishspeed;

		Velocity = Velocity.WithZ( 0 );
		Accelerate( wishdir, wishspeed, 0, Acceleration );
		Velocity = Velocity.WithZ( 0 );

		// Add in any base velocity to the current velocity.
		Velocity += BaseVelocity;

		try
		{
			if ( Velocity.Length < 1.0f )
			{
				Velocity = Vector3.Zero;
				return;
			}

			// first try just moving to the destination
			var dest = (Position + Velocity * Time.Delta).WithZ( Position.z );

			var pm = TraceBBox( Position, dest );

			if ( pm.Fraction == 1 )
			{
				Position = pm.EndPosition;
				StayOnGround();
				return;
			}

			StepMove();
		}
		finally
		{

			// Now pull the base velocity back out. Base velocity is set if you are on a moving object, like a conveyor (or maybe another monster?)
			Velocity -= BaseVelocity;
		}

		StayOnGround();
	}

	public virtual void StepMove()
	{
		MoveHelper mover = new MoveHelper( Position, Velocity );
		mover.Trace = mover.Trace.Size( mins, maxs ).Ignore( Pawn );
		mover.MaxStandableAngle = GroundAngle;
		mover.TryMoveWithStep( Time.Delta, StepSize );

		Position = mover.Position;
		Velocity = mover.Velocity;
	}

	public virtual void Move()
	{
		MoveHelper mover = new MoveHelper( Position, Velocity );
		mover.Trace = mover.Trace.Size( mins, maxs ).Ignore( Pawn );
		mover.MaxStandableAngle = GroundAngle;

		mover.TryMove( Time.Delta );

		Position = mover.Position;
		Velocity = mover.Velocity;
	}

	/// <summary>
	/// Add our wish direction and speed onto our velocity
	/// </summary>
	public virtual void Accelerate( Vector3 wishdir, float wishspeed, float speedLimit, float acceleration )
	{
		if ( speedLimit > 0 && wishspeed > speedLimit )
			wishspeed = speedLimit;

		// See if we are changing direction a bit
		var currentspeed = Velocity.Dot( wishdir );

		// Reduce wishspeed by the amount of veer.
		var addspeed = wishspeed - currentspeed;

		// If not going to add any speed, done.
		if ( addspeed <= 0 )
			return;

		// Determine amount of acceleration.
		var accelspeed = acceleration * Time.Delta * wishspeed * SurfaceFriction;

		// Cap at addspeed
		if ( accelspeed > addspeed )
			accelspeed = addspeed;

		Velocity += wishdir * accelspeed;
	}

	/// <summary>
	/// Remove ground friction from velocity
	/// </summary>
	public virtual void ApplyFriction( float frictionAmount = 1.0f )
	{
		// Calculate speed
		var speed = Velocity.Length;
		if ( speed < 0.1f ) return;

		// Bleed off some speed, but if we have less than the bleed
		//  threshold, bleed the threshold amount.
		float control = (speed < StopSpeed) ? StopSpeed : speed;

		// Add the amount to the drop amount.
		var drop = control * Time.Delta * frictionAmount;

		// scale the velocity
		float newspeed = speed - drop;
		if ( newspeed < 0 ) newspeed = 0;

		if ( newspeed != speed )
		{
			newspeed /= speed;
			Velocity *= newspeed;
		}
	}

	protected bool CanJump()
	{
		if ( Slide.TimeSinceActivate < 0.3f )
			return false;
		if ( GroundEntity == null )
			return false;

		return true;
	}

	public virtual void CheckJumpButton()
	{
		if ( !CanJump() ) return;

		JumpWinding = false;
		TimeSinceJumped = 0;
		
		// If we are in the water most of the way...
		if ( Swimming )
		{
			// swimming, not jumping
			ClearGroundEntity();
			Velocity = Velocity.WithZ( 100 );

			return;
		}

		ClearGroundEntity();

		// TODO - Look at these bad boys... find out what they do and remove the magic.
		float flGroundFactor = 1.0f;
		float flMul = 268.3281572999747f * 1.0f;
		float startz = Velocity.z;

		if ( Duck.IsActive )
			flMul *= 0.2f;

		Velocity = Velocity.WithZ( startz + flMul * flGroundFactor );
		Velocity -= new Vector3( 0, 0, Gravity * 0.5f ) * Time.Delta;
		
		Pawn.PlaySound( "sounds/player/foley/gear/player.jump.gear.sound" );
		AddEvent( "jump" );
	}

	public virtual void AirMove()
	{
		var wishdir = WishVelocity.Normal;
		var wishspeed = WishVelocity.Length;

		Accelerate( wishdir, wishspeed, AirControl, AirAcceleration );

		Velocity += BaseVelocity;
		Move();
		Velocity -= BaseVelocity;
	}

	public virtual void WaterMove()
	{
		var wishdir = WishVelocity.Normal;
		var wishspeed = WishVelocity.Length;
		wishspeed *= 0.8f;

		Accelerate( wishdir, wishspeed, 100, Acceleration );

		Velocity += BaseVelocity;
		Move();
		Velocity -= BaseVelocity;
	}

	bool IsTouchingLadder = false;
	Vector3 LadderNormal;

	public virtual void CheckLadder()
	{
		var wishvel = new Vector3( Input.Forward, Input.Left, 0 );
		wishvel *= Input.Rotation.Angles().WithPitch( 0 ).ToRotation();
		wishvel = wishvel.Normal;

		if ( IsTouchingLadder )
		{
			if ( Input.Pressed( InputButton.Jump ) )
			{
				Velocity = LadderNormal * 100.0f;
				IsTouchingLadder = false;

				return;

			}
			else if ( GroundEntity != null && LadderNormal.Dot( wishvel ) > 0 )
			{
				IsTouchingLadder = false;

				return;
			}
		}

		const float ladderDistance = 1.0f;
		var start = Position;
		Vector3 end = start + (IsTouchingLadder ? (LadderNormal * -1.0f) : wishvel) * ladderDistance;

		var pm = Trace.Ray( start, end )
					.Size( mins, maxs )
					.WithTag( "ladder" )
					.Ignore( Pawn )
					.Run();

		IsTouchingLadder = false;

		if ( pm.Hit )
		{
			IsTouchingLadder = true;
			LadderNormal = pm.Normal;
		}
	}

	public virtual void LadderMove()
	{
		var velocity = WishVelocity;
		float normalDot = velocity.Dot( LadderNormal );
		var cross = LadderNormal * normalDot;
		Velocity = (velocity - cross) + (-normalDot * LadderNormal.Cross( Vector3.Up.Cross( LadderNormal ).Normal ));

		Move();
	}


	public virtual void CategorizePosition( bool bStayOnGround )
	{
		SurfaceFriction = 1.0f;

		// Doing this before we move may introduce a potential latency in water detection, but
		// doing it after can get us stuck on the bottom in water if the amount we move up
		// is less than the 1 pixel 'threshold' we're about to snap to.	Also, we'll call
		// this several times per frame, so we really need to avoid sticking to the bottom of
		// water on each call, and the converse case will correct itself if called twice.
		//CheckWater();

		var point = Position - Vector3.Up * 2;
		var vBumpOrigin = Position;

		//
		//  Shooting up really fast.  Definitely not on ground trimed until ladder shit
		//
		bool bMovingUpRapidly = Velocity.z > MaxNonJumpVelocity;
		bool bMoveToEndPos = false;

		if ( GroundEntity != null ) // and not underwater
		{
			bMoveToEndPos = true;
			point.z -= StepSize;
		}
		else if ( bStayOnGround )
		{
			bMoveToEndPos = true;
			point.z -= StepSize;
		}

		if ( bMovingUpRapidly || Swimming ) // or ladder and moving up
		{
			ClearGroundEntity();
			return;
		}

		var pm = TraceBBox( vBumpOrigin, point, 4.0f );

		if ( pm.Entity == null || Vector3.GetAngle( Vector3.Up, pm.Normal ) > GroundAngle )
		{
			ClearGroundEntity();
			bMoveToEndPos = false;

			if ( Velocity.z > 0 )
				SurfaceFriction = 0.25f;
		}
		else
		{
			UpdateGroundEntity( pm );
		}

		if ( bMoveToEndPos && !pm.StartedSolid && pm.Fraction > 0.0f && pm.Fraction < 1.0f )
		{
			Position = pm.EndPosition;
		}

	}

	/// <summary>
	/// We have a new ground entity
	/// </summary>
	public virtual void UpdateGroundEntity( TraceResult tr )
	{
		GroundNormal = tr.Normal;

		// VALVE HACKHACK: Scale this to fudge the relationship between vphysics friction values and player friction values.
		// A value of 0.8f feels pretty normal for vphysics, whereas 1.0f is normal for players.
		// This scaling trivially makes them equivalent.  REVISIT if this affects low friction surfaces too much.
		SurfaceFriction = tr.Surface.Friction * 1.25f;
		if ( SurfaceFriction > 1 ) SurfaceFriction = 1;

		GroundEntity = tr.Entity;

		if ( GroundEntity != null )
		{
			BaseVelocity = GroundEntity.Velocity;
		}
	}

	/// <summary>
	/// We're no longer on the ground, remove it
	/// </summary>
	public virtual void ClearGroundEntity()
	{
		if ( GroundEntity == null ) return;

		GroundEntity = null;
		GroundNormal = Vector3.Up;
		SurfaceFriction = 1.0f;
	}

	/// <summary>
	/// Traces the bbox and returns the trace result.
	/// LiftFeet will move the start position up by this amount, while keeping the top of the bbox at the same 
	/// position. This is good when tracing down because you won't be tracing through the ceiling above.
	/// </summary>
	public override TraceResult TraceBBox( Vector3 start, Vector3 end, Vector3 mins, Vector3 maxs, float liftFeet = 0.0f )
	{
		if ( liftFeet > 0 )
		{
			start += Vector3.Up * liftFeet;
			maxs = maxs.WithZ( maxs.z - liftFeet );
		}

		var tr = Trace.Ray( start + TraceOffset, end + TraceOffset )
					.Size( mins, maxs )
					.WithAnyTags( "solid", "playerclip", "passbullets" )
					.WithoutTags( "player" )
					.Ignore( Pawn )
					.Run();

		tr.EndPosition -= TraceOffset;
		return tr;
	}

	/// <summary>
	/// Traces the current bbox and returns the result.
	/// liftFeet will move the start position up by this amount, while keeping the top of the bbox at the same
	/// position. This is good when tracing down because you won't be tracing through the ceiling above.
	/// </summary>
	public override TraceResult TraceBBox( Vector3 start, Vector3 end, float liftFeet = 0.0f )
	{
		return TraceBBox( start, end, mins, maxs, liftFeet );
	}

	/// <summary>
	/// Try to keep a walking player on the ground when running down slopes etc
	/// </summary>
	public virtual void StayOnGround()
	{
		var start = Position + Vector3.Up * 2;
		var end = Position + Vector3.Down * StepSize;

		// See how far up we can go without getting stuck
		var trace = TraceBBox( Position, start );
		start = trace.EndPosition;

		// Now trace down from a known safe position
		trace = TraceBBox( start, end );

		if ( trace.Fraction <= 0 ) return;
		if ( trace.Fraction >= 1 ) return;
		if ( trace.StartedSolid ) return;
		if ( Vector3.GetAngle( Vector3.Up, trace.Normal ) > GroundAngle ) return;

		Position = trace.EndPosition;
	}
}

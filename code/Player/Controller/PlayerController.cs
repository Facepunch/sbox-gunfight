
namespace Facepunch.Gunfight;

[Library]
public partial class PlayerController : PawnController
{
	public float SprintSpeed => 250.0f;
	public float WalkSpeed => 120.0f;
	public float DefaultSpeed => 175.0f;
	public float Acceleration => 6.0f;
	public float StopSpeed => 100.0f;
	public float GroundAngle => 46.0f;
	public float StepSize => 16f;
	public float MaxNonJumpVelocity => 140.0f;
	public float BodyGirth => 32.0f;
	public float BodyHeight => 72.0f;
	
	// TODO - Expose this to game / gamemodes
	public float Gravity => 700.0f;
	public float AirControl => 10.0f;
	public float AirAcceleration => 50.0f;

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
	public ClimbMechanic Climb => GetMechanic<ClimbMechanic>();
	public CoverAimMechanic CoverAim => GetMechanic<CoverAimMechanic>();

	public List<BaseMoveMechanic> Mechanics = new();
	public BaseMoveMechanic CurrentMechanic => Mechanics.FirstOrDefault( x => x.IsActive );

	public PlayerController()
	{
		SinceStoppedSprinting = -1;

		Mechanics.Add( new CoverAimMechanic( this ) );
		// Mechanics.Add( new VaultMoveMechanic( this ) );
		Mechanics.Add( new ClimbMechanic( this ) );
		Mechanics.Add( new SlideMechanic( this ) );
		Mechanics.Add( new DuckMechanic( this ) );
		Mechanics.Add( new UnstuckMechanic( this ) );

		if ( Game.IsEditor )
		{
			Mechanics.Add( new NoclipMechanic( this ) );
		}
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
		var maxs = new Vector3( +girth, +girth, CurrentMechanic?.GetEyeHeight() ?? 72f );

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
		var maxs = new Vector3( +girth, +girth, CurrentEyeHeight + 6f ) * Pawn.Scale;

		CurrentMechanic?.UpdateBBox( ref mins, ref maxs, Pawn.Scale );

		SetBBox( mins, maxs );
	}

	protected float SurfaceFriction;

	// Accessors
	public GunfightPlayer Player => Pawn as GunfightPlayer;
	public GunfightWeapon Weapon => Player?.ActiveChild as GunfightWeapon;

	public override void FrameSimulate()
	{
		SimulateEyes();
	}

	protected void OnStoppedSprinting()
	{
		SinceStoppedSprinting = 0;
	}

	public virtual float GetEyeHeight()
	{
		return CurrentMechanic?.GetEyeHeight() ?? 64;
	}

	protected float GetGroundFriction()
	{
		return CurrentMechanic?.GetGroundFriction() ?? 4f;
	}

	protected bool CanAim()
	{
		if ( !Weapon.IsValid() ) return false;
		if ( Weapon.TimeSinceDeployed < 0.3f ) return false;
		if ( Weapon.WeaponDefinition.AimingDisabled ) return false;
		if ( !GroundEntity.IsValid() ) return false;
		if ( IsSprinting ) return false;
		if ( Slide.IsActive ) return false;

		return true;
	}

	[Net, Predicted] public float CurrentEyeHeight { get; set; } = 64f;
	protected void SimulateEyes()
	{
		var target = GetEyeHeight();
		// Magic number :sad:
		var trace = TraceBBox( Position, Position, 0, 10f );
		if ( trace.Hit && target > CurrentEyeHeight )
		{
			// We hit something, that means we can't increase our eye height because something's in the way.
		}
		else
		{
			CurrentEyeHeight = target;
		}

		Player.EyeRotation = Player.LookInput.ToRotation();
		Player.EyeLocalPosition = Vector3.Up * CurrentEyeHeight;
	}

	[ConVar.Client( "gunfight_aim_debug" )]
	public static bool UseAimDebug { get; set; }

	public override void Simulate()
	{
		SimulateEyes();

		UpdateBBox();

		if ( Weapon.IsValid() && Input.Down( "Attack2" ) && CanAim() || UseAimDebug )
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

		SimulateMechanics();

		if ( GetMechanic<NoclipMechanic>()?.IsActive ?? false ) return;

		CheckLadder();
		Swimming = Pawn.GetWaterLevel() > 0.6f;

		IsSprinting = Input.Down( "Run" );

		if ( IsSprinting && Velocity.Length < 40 || Player.MoveInput.x < 0.5f )
			IsSprinting = false;

		if ( Input.Down( "Attack1" ) || Input.Down( "Attack2" ) )
			IsSprinting = false;

		if ( Duck.IsActive || Slide.IsActive )
			IsSprinting = false;

		//
		// Start Gravity
		//
		if ( !Swimming && !IsTouchingLadder )
		{
			Velocity -= new Vector3( 0, 0, Gravity * 0.5f ) * Time.Delta;
			Velocity += new Vector3( 0, 0, BaseVelocity.z ) * Time.Delta;

			BaseVelocity = BaseVelocity.WithZ( 0 );
		}

		if ( CanJump() && Input.Pressed( "Jump" ) && !JumpWinding )
		{
			JumpWinding = true;
			JumpWindup = 0;
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
		WishVelocity = new Vector3( Player.MoveInput.x, Player.MoveInput.y * ( IsSprinting ? 0.5f : 1f ), 0 );
		var inSpeed = WishVelocity.Length.Clamp( 0, 1 );
		WishVelocity *= Player.LookInput.WithPitch( 0 ).ToRotation();

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
			var speedMult = sinceFall.Remap( 0, FallRecoveryTime, 0.5f, 1 );
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
			if ( Game.IsServer ) lineOffset = 15;

			DebugOverlay.ScreenText( $"        Velocity: {Velocity}", lineOffset + 1 );
			DebugOverlay.ScreenText( $"    BaseVelocity: {BaseVelocity}", lineOffset + 2 );
			DebugOverlay.ScreenText( $"    GroundEntity: {GroundEntity} [{GroundEntity?.Velocity}]", lineOffset + 3 );
			DebugOverlay.ScreenText( $" SurfaceFriction: {SurfaceFriction}", lineOffset + 4 );
			DebugOverlay.ScreenText( $"    WishVelocity: {WishVelocity}", lineOffset + 5 );
			DebugOverlay.ScreenText( $"    Slide: {Slide?.IsActive ?? false}", lineOffset + 6 );
			DebugOverlay.ScreenText( $"    Duck: {Duck?.IsActive ?? false}", lineOffset + 7 );
		}

		if ( Game.IsServer )
		{
			if ( IsSprinting || Slide.IsActive )
			{
				var trForward = Trace.Ray( Pawn.AimRay.Position, Pawn.AimRay.Position + Pawn.AimRay.Forward * 50f ).WithTag( "solid" ).Radius( 5f ).Ignore( Pawn ).Run();

				if ( trForward.Entity is DoorEntity door && ( door.State == DoorEntity.DoorState.Closed || door.State == DoorEntity.DoorState.Closing ) )
				{
					door.Speed = 500f;
					door.Open( Pawn );
					door.PlaySound( "sounds/world/impact/door.metal.slam.sound" );
				
					SendDoorSlamEffect( To.Single( Pawn.Client ) );

					_ = ResetDoor( door );
				}
			}
		}
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

		TakeFallDamage();

		new ScreenShake.Pitch( 1f, 10f * velocityLength );
	}

	void TakeFallDamage()
	{
		if ( Game.IsServer )
		{
			var jumpFromPos = JumpPosition;
			var positionNow = Position;

			// jumped up somehow?
			if ( positionNow.z >= jumpFromPos.z ) return;

			var fallDamageScale = 125;
			var zDist = MathF.Abs( positionNow.z - jumpFromPos.z );

			var scale = zDist.LerpInverse( 0, 500f, true );
			if ( scale < 0.35f ) return;

			Player.TakeDamage( DamageInfo.Generic( fallDamageScale * scale ) );
		}
	}

	[Net, Predicted]
	protected bool _IsSprinting { get; set; }

	public bool IsSprinting { get => _IsSprinting; protected set { if ( _IsSprinting && !value ) SinceSprintStopped = 0; _IsSprinting = value; } }
	
	[Net, Predicted] public TimeSince SinceSprintStopped { get; set; }

	public virtual float GetWishSpeed()
	{
		var mechanicSpeed = CurrentMechanic?.GetWishSpeed();
		if ( mechanicSpeed != null ) return mechanicSpeed.Value;

		if ( IsSprinting ) return SprintSpeed;
		if ( Input.Down( "Walk" ) ) return WalkSpeed;

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
		if ( Slide.TimeSinceActivate < 0.5f )
			return false;
		if ( GroundEntity == null )
			return false;

		return true;
	}

	Vector3 JumpPosition;

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

		JumpPosition = Position;

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

		Weapon?.ViewModelEntity?.SetAnimParameter( "b_jump", true );

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
		var wishvel = new Vector3( Player.MoveInput.x, Player.MoveInput.y, 0 );
		wishvel *= Player.LookInput.WithPitch( 0 ).ToRotation();
		wishvel = wishvel.Normal;

		if ( IsTouchingLadder )
		{
			if ( Input.Pressed( "Jump" ) )
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
	public virtual TraceResult TraceBBox( Vector3 start, Vector3 end, Vector3 mins, Vector3 maxs, float liftFeet = 0.0f, float liftHead = 0.0f )
	{
		if ( liftFeet > 0 )
		{
			start += Vector3.Up * liftFeet;
			maxs = maxs.WithZ( maxs.z - liftFeet );
		}

		if ( liftHead > 0 )
		{
			end += Vector3.Up * liftHead;
		}

		var tr = Trace.Ray( start, end )
					.Size( mins, maxs )
					.WithAnyTags( "solid", "playerclip", "passbullets" )
					.Ignore( Player )
					.Run();

		return tr;
	}

	/// <summary>
	/// This calls TraceBBox with the right sized bbox. You should derive this in your controller if you 
	/// want to use the built in functions
	/// </summary>
	public virtual TraceResult TraceBBox( Vector3 start, Vector3 end, float liftFeet = 0.0f, float liftHead = 0.0f )
	{
		var hull = GetHull();
		return TraceBBox( start, end, hull.Mins, hull.Maxs, liftFeet, liftHead );
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

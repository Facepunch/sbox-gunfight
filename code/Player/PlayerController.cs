namespace Gunfight;

public sealed class PlayerController : Component
{
	/// <summary>
	/// A reference to the player's body (the GameObject)
	/// </summary>
	[Property] public GameObject Body { get; set; }

	/// <summary>
	/// A reference to the animation helper (normally on the Body GameObject)
	/// </summary>
	[Property] public AnimationHelper AnimationHelper { get; set; }

	/// <summary>
	/// The current gravity. Make this a gamerule thing later?
	/// </summary>
	[Property] public Vector3 Gravity { get; set; } = new Vector3( 0, 0, 800 );

	/// <summary>
	/// The current character controller for this player.
	/// </summary>
	[Property] public CharacterController CharacterController { get; set; }

	/// <summary>
	/// The current camera controller for this player.
	/// </summary>
	[Property] public CameraController CameraController { get; set; }

	/// <summary>
	/// Finds the first enabled weapon on our player.
	/// Make this quicker and not fetching components every time.
	/// </summary>
	public Weapon Weapon => Components.Get<Weapon>( FindMode.EnabledInSelfAndDescendants );

	/// <summary>
	/// Finds the first <see cref="SkinnedModelRenderer"/> on <see cref="Body"/>
	/// </summary>
	public SkinnedModelRenderer BodyRenderer => Body.Components.Get<SkinnedModelRenderer>();

	/// <summary>
	/// An accessor to get the camera controller's aim ray.
	/// </summary>
	public Ray AimRay => CameraController.AimRay;

	/// <summary>
	/// The current holdtype for the player.
	/// </summary>
	[Property] AnimationHelper.HoldTypes CurrentHoldType { get; set; } = AnimationHelper.HoldTypes.None;

	/// <summary>
	/// Are we running?
	/// </summary>
	[Property, System.ComponentModel.ReadOnly( true )] public bool IsRunning { get; private set; }

	// Properties used only in this component.
	Vector3 WishVelocity;
	Angles EyeAngles;
	bool IsDucking;
	
	protected override void OnUpdate()
	{
		var cc = CharacterController;

		if ( Weapon.IsValid() )
		{
			CurrentHoldType = Weapon.GetHoldType();
		}

		// Eye input
		if ( !IsProxy && cc != null )
		{
			EyeAngles.pitch += Input.MouseDelta.y * 0.1f;
			EyeAngles.yaw -= Input.MouseDelta.x * 0.1f;
			EyeAngles.roll = 0;

			// we're a shooter game!
			EyeAngles.pitch = EyeAngles.pitch.Clamp( -90, 90 );

			var cam = CameraController.Camera;
			var lookDir = EyeAngles.ToRotation();

			cam.Transform.Rotation = lookDir;

			IsRunning = false;
			IsDucking = false;

			if ( Input.Down( "Run" ) )
				IsRunning = true;
			else if ( Input.Down( "Duck" ) )
				IsDucking = true;
		}

		float rotateDifference = 0;

		// rotate body to look angles
		if ( Body is not null )
		{
			var targetAngle = new Angles( 0, EyeAngles.yaw, 0 ).ToRotation();

			rotateDifference = Body.Transform.Rotation.Distance( targetAngle );

			if ( rotateDifference > 50.0f || ( cc != null && cc.Velocity.Length > 10.0f ) )
			{
				Body.Transform.Rotation = Rotation.Lerp( Body.Transform.Rotation, targetAngle, Time.Delta * 10.0f );
			}
		}

		if ( AnimationHelper is not null && cc is not null )
		{
			AnimationHelper.WithVelocity( cc.Velocity );
			AnimationHelper.WithWishVelocity( WishVelocity );
			AnimationHelper.IsGrounded = cc.IsOnGround;
			AnimationHelper.FootShuffle = rotateDifference;
			AnimationHelper.WithLook( EyeAngles.Forward, 1, 1, 1.0f );
			AnimationHelper.MoveStyle = IsRunning ? AnimationHelper.MoveStyles.Run : AnimationHelper.MoveStyles.Walk;
			AnimationHelper.DuckLevel = IsDucking ? 100 : 0;
			AnimationHelper.HoldType = CurrentHoldType;
		}
	}

	/// <summary>
	/// A network message that lets other users that we've triggered a jump.
	/// </summary>
	[Broadcast] public void OnJump() => AnimationHelper?.TriggerJump();

	protected override void OnFixedUpdate()
	{
		if ( IsProxy )
			return;

		BuildWishVelocity();

		var cc = CharacterController;
		if ( cc == null )
			return;

		if ( cc.IsOnGround && Input.Down( "Jump" ) )
		{
			float flGroundFactor = 1.0f;
			float flMul = 268.3281572999747f * 1.2f;

			cc.Punch( Vector3.Up * flMul * flGroundFactor );

			OnJump();
		}

		if ( cc.IsOnGround )
		{
			cc.Velocity = cc.Velocity.WithZ( 0 );
			cc.Accelerate( WishVelocity );
			cc.ApplyFriction( 4.0f );
		}
		else
		{
			cc.Velocity -= Gravity * Time.Delta * 0.5f;
			cc.Accelerate( WishVelocity.ClampLength( 50 ) );
			cc.ApplyFriction( 0.1f );
		}

		cc.Move();

		if ( !cc.IsOnGround )
		{
			cc.Velocity -= Gravity * Time.Delta * 0.5f;
		}
		else
		{
			cc.Velocity = cc.Velocity.WithZ( 0 );
		}
	}

	public void BuildWishVelocity()
	{
		var rot = EyeAngles.WithPitch( 0f ).ToRotation();

		WishVelocity = 0;

		if ( Input.Down( "Forward" ) ) WishVelocity += rot.Forward;
		if ( Input.Down( "Backward" ) ) WishVelocity += rot.Backward;
		if ( Input.Down( "Left" ) ) WishVelocity += rot.Left;
		if ( Input.Down( "Right" ) ) WishVelocity += rot.Right;

		WishVelocity = WishVelocity.WithZ( 0 );

		if ( !WishVelocity.IsNearZeroLength ) WishVelocity = WishVelocity.Normal;

		if ( Input.Down( "Run" ) ) WishVelocity *= 320.0f;
		else if ( Input.Down( "Duck" ) ) WishVelocity *= 85f;
		else WishVelocity *= 110.0f;
	}

	public void Write( ref ByteStream stream )
	{
		stream.Write( IsRunning );
		stream.Write( EyeAngles );
	}

	public void Read( ByteStream stream )
	{
		IsRunning = stream.Read<bool>();
		EyeAngles = stream.Read<Angles>();
	}
}

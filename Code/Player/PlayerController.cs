using System.Text.Json.Serialization;

namespace Gunfight;

public partial class PlayerController : Component, IPawn
{
	/// <summary>
	/// A reference to the player's body (the GameObject)
	/// </summary>
	[Property] public GameObject Body { get; set; }

	/// <summary>
	/// A reference to the player's head (the GameObject)
	/// </summary>
	[Property] public GameObject Head { get; set; }

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
	[RequireComponent] public CharacterController CharacterController { get; set; }

	/// <summary>
	/// The current camera controller for this player.
	/// </summary>
	[RequireComponent] public CameraController CameraController { get; set; }

	/// <summary>
	/// A reference to the View Model's camera. This will be disabled by the View Model.
	/// </summary>
	[Property] public CameraComponent ViewModelCamera { get; set; }

	/// <summary>
	/// A <see cref="GameObject"/> that will hold our ViewModel.
	/// </summary>
	[Property] public GameObject ViewModelGameObject { get; set; }

	/// <summary>
	/// Get a quick reference to the real Camera GameObject.
	/// </summary>
	public GameObject CameraGameObject => CameraController.Camera.GameObject;

	/// <summary>
	/// Finds the first <see cref="SkinnedModelRenderer"/> on <see cref="Body"/>
	/// </summary>
	public SkinnedModelRenderer BodyRenderer => Body.Components.Get<SkinnedModelRenderer>();

	/// <summary>
	/// An accessor to get the camera controller's aim ray.
	/// </summary>
	public Ray AimRay => CameraController.AimRay;

	// TODO: move this into something that isn't on the player, this should be on an animator being fed info like the weapon
	[Property] AnimationHelper.HoldTypes CurrentHoldType { get; set; } = AnimationHelper.HoldTypes.None;

	// TODO: move this into something that isn't on the player, this is shit
	[Property, ReadOnly, JsonIgnore] public bool IsAiming { get; private set; }

	/// <summary>
	/// GameObject with the player's HUD. We'll only turn it on if we're the local connection.
	/// </summary>
	[Property] public GameObject HUDGameObject { get; set; }

	/// <summary>
	/// Called when the player jumps.
	/// </summary>
	[Property] public Action OnJump { get; set; }

	private Weapon currentWeapon;
	/// <summary>
	/// What weapon are we using?
	/// </summary>
	public Weapon CurrentWeapon
	{
		get => currentWeapon;
		set
		{
			currentWeapon = value;

			if ( ( this as IPawn ).IsPossessed )
			{
				CreateViewModel();
			}
		}
	}

	private void ClearViewModel()
	{
		if ( CurrentWeapon.IsValid() )
		{
			CurrentWeapon?.ClearViewModel( this );
		}
	}

	private void CreateViewModel()
	{
		if ( CurrentWeapon.IsValid() )
		{
			CurrentWeapon.CreateViewModel( this );
		}
	}

	// Properties used only in this component.
	Vector3 WishVelocity;
	public Angles EyeAngles;

	public bool IsGrounded { get; set; }

	protected float GetEyeHeightOffset()
	{
		if ( CurrentEyeHeightOverride is not null ) return CurrentEyeHeightOverride.Value;
		return 0f;
	}

	float SmoothEyeHeight = 0f;

	protected override void OnAwake()
	{
		baseAcceleration = CharacterController.Acceleration;

		// If we're the local connection, turn the HUD on
		if ( !IsProxy )
		{
			HUDGameObject.Enabled = true;
		}
	}

	protected override void OnUpdate()
	{
		var cc = CharacterController;

		if ( CurrentWeapon.IsValid() )
		{
			CurrentHoldType = CurrentWeapon.GetHoldType();
		}

		// Eye input
		if ( !IsProxy && cc != null )
		{
			var cameraGameObject = CameraController.Camera.GameObject;

			var eyeHeightOffset = GetEyeHeightOffset();

			SmoothEyeHeight = SmoothEyeHeight.LerpTo( eyeHeightOffset, Time.Delta * 10f );

			cameraGameObject.Transform.LocalPosition = Vector3.Zero.WithZ( SmoothEyeHeight );

			EyeAngles += Input.AnalogLook;

			// we're a shooter game!
			EyeAngles.pitch = EyeAngles.pitch.Clamp( -90, 90 );

			var cam = CameraController.Camera;
			var lookDir = EyeAngles.ToRotation();

			cam.Transform.Rotation = lookDir;

			IsAiming = Input.Down( "Attack2" );
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

		var wasGrounded = IsGrounded;
		IsGrounded = cc.IsOnGround;

		if ( wasGrounded != IsGrounded )
		{
			GroundedChanged();
		}

		if ( AnimationHelper is not null && cc is not null )
		{
			AnimationHelper.WithVelocity( cc.Velocity );
			AnimationHelper.WithWishVelocity( WishVelocity );
			AnimationHelper.IsGrounded = IsGrounded;
			AnimationHelper.FootShuffle = rotateDifference;
			AnimationHelper.WithLook( EyeAngles.Forward, 1, 1, 1.0f );
			AnimationHelper.MoveStyle = HasTag( "sprint" ) ? AnimationHelper.MoveStyles.Run : AnimationHelper.MoveStyles.Walk;
			AnimationHelper.DuckLevel = HasTag( "crouch" ) ? 100 : 0;
			AnimationHelper.HoldType = CurrentHoldType;
			AnimationHelper.SkidAmount = HasTag( "slide" ) ? 1 : 0;
		}
	}

	private void GroundedChanged()
	{
		var nowOffGround = IsGrounded == false;
	}

	/// <summary>
	/// A network message that lets other users that we've triggered a jump.
	/// </summary>
	[Broadcast]
	public void BroadcastPlayerJumped()
	{
		AnimationHelper?.TriggerJump();
		OnJump?.Invoke();
	}

	/// <summary>
	/// Get the current friction.
	/// </summary>
	/// <returns></returns>
	private float GetFriction()
	{
		if ( !CharacterController.IsOnGround ) return 0.1f;
		if ( CurrentFrictionOverride is not null ) return CurrentFrictionOverride.Value;

		return 4.0f;
	}

	private float baseAcceleration = 10;
	private void ApplyAccceleration()
	{
		if ( CurrentAccelerationOverride is not null )
		{
			CharacterController.Acceleration = CurrentAccelerationOverride.Value;
		}
		else
		{
			CharacterController.Acceleration = baseAcceleration;
		}
	}

	protected override void OnFixedUpdate()
	{
		if ( IsProxy )
			return;

		var cc = CharacterController;
		if ( cc == null )
			return;

		BuildWishInput();

		// Wish direction could change here
		OnUpdateMechanics();

		BuildWishVelocity();

		if ( cc.IsOnGround && Input.Down( "Jump" ) )
		{
			float flGroundFactor = 1.0f;
			float flMul = 268.3281572999747f * 1.2f;

			cc.Punch( Vector3.Up * flMul * flGroundFactor );

			BroadcastPlayerJumped();
		}

		ApplyAccceleration();

		if ( cc.IsOnGround )
		{
			cc.Velocity = cc.Velocity.WithZ( 0 );
			cc.Accelerate( WishVelocity );
		}
		else
		{
			cc.Velocity -= Gravity * Time.Delta * 0.5f;
			cc.Accelerate( WishVelocity.ClampLength( 50 ) );
		}

		cc.ApplyFriction( GetFriction() );
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

	protected float GetWishSpeed()
	{
		if ( CurrentSpeedOverride is not null ) return CurrentSpeedOverride.Value;

		// Default speed
		return 110.0f;
	}

	public Vector3 WishMove;

	public void BuildWishInput()
	{
		WishMove = 0;
		WishMove += Input.AnalogMove;
	}

	public void BuildWishVelocity()
	{
		WishVelocity = 0;
		
		var rot = EyeAngles.WithPitch( 0f ).ToRotation();
		var wishDirection = WishMove * rot;
		wishDirection = wishDirection.WithZ( 0 );

		WishVelocity = wishDirection * GetWishSpeed();
	}

	public void Write( ref ByteStream stream )
	{
		stream.Write( EyeAngles );
	}

	public void Read( ByteStream stream )
	{
		EyeAngles = stream.Read<Angles>();
	}
}

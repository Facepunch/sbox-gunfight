namespace Gunfight;

/// <summary>
/// A weapon's viewmodel. It's responsibility is to listen to events from a weapon.
/// It should only exist on the client for the currently possessed pawn.
/// </summary>
public partial class ViewModel : Component
{
	/// <summary>
	/// A reference to the <see cref="Weapon"/> we want to listen to.
	/// </summary>
	public Weapon Weapon { get; set; }

	/// <summary>
	/// A reference to the viewmodel's arms.
	/// </summary>
	[Property] public SkinnedModelRenderer Arms { get; set; }

	/// <summary>
	/// Look up the tree to find the camera.
	/// </summary>
	CameraController CameraController => PlayerController.CameraController;

	/// <summary>
	/// Looks up the tree to find the player controller.
	/// </summary>
	PlayerController PlayerController => Weapon.PlayerController;

	[Property] public SkinnedModelRenderer ModelRenderer { get; set; }

	/// <summary>
	/// The View Model camera 
	/// </summary>
	public CameraComponent ViewModelCamera { get; set; }

	protected override void OnStart()
	{
		ModelRenderer.Set( "b_deploy", true );

		// Register an event.
		PlayerController.OnJump += OnPlayerJumped;
	}

	void OnPlayerJumped()
	{
		ModelRenderer.Set( "b_jump", true );
	}

	void ApplyAnimationTransform()
	{
		var bone = Weapon.ViewModel.ModelRenderer.SceneModel.GetBoneLocalTransform( "camera" );
		var camera = Weapon.PlayerController.CameraGameObject;
		camera.Transform.LocalPosition += bone.Position;
		camera.Transform.LocalRotation *= bone.Rotation;
	}

	private Vector3 lerpedWishLook;

	private Vector3 localPosition;
	private Rotation localRotation;

	private Vector3 lerpedLocalPosition;
	private Rotation lerpedlocalRotation;

	protected void ApplyVelocity()
	{
		var moveVel = PlayerController.CharacterController.Velocity;
		var moveLen = moveVel.Length;
		if ( PlayerController.HasTag( "slide" ) ) moveLen = 0;

		var wishLook = PlayerController.WishMove.Normal * 1f;
		if ( Weapon?.Tags.Has( "aiming" ) ?? false ) wishLook = 0;

		lerpedWishLook = lerpedWishLook.LerpTo( wishLook, Time.Delta * 5.0f );

		localRotation *= Rotation.From( 0, -lerpedWishLook.y * 3f, 0 );
		localPosition += -lerpedWishLook;

		ModelRenderer.Set( "move_groundspeed", moveLen );
	}

	private float FieldOfViewOffset = 0f;
	private float TargetFieldOfView = 90f;

	void AddFieldOfViewOffset( float degrees )
	{
		FieldOfViewOffset -= degrees;
	}

	void ApplyStates()
	{
		if ( PlayerController.HasTag( "slide" ) )
		{
			localPosition += Vector3.Backward * 2f;
			localRotation *= Rotation.From( 10, 25, -5 );
		}

		if ( Weapon.Tags.Has( "aiming" ) )
		{
			var aimFn = Weapon.GetFunction<AimWeaponFunction>();
			localPosition += aimFn.AimOffset;
			localRotation *= aimFn.AimAngles.ToRotation();

			CameraController.AddFieldOfViewOffset( 5 );
			AddFieldOfViewOffset( 40 );
		}
		else // While not aiming
		{
			var shootFn = Weapon.GetFunction<ShootWeaponFunction>();

			if ( PlayerController.HasTag( "crouch" ) && shootFn.TimeSinceShoot > 0.25f )
			{
				localPosition += Vector3.Right * -2f;
				localPosition += Vector3.Up * -1f;
				localRotation *= Rotation.From( 0, -2, -18 );
			}
		}
	}

	void ApplyAnimationParameters()
	{
		ModelRenderer.Set( "b_sprint", PlayerController.HasTag( "sprint" ) );
		ModelRenderer.Set( "b_grounded", PlayerController.IsGrounded );

		// Ironsights
		ModelRenderer.Set( "ironsights", Weapon.Tags.Has( "aiming" ) ? 2 : 0 );
		ModelRenderer.Set( "ironsights_fire_scale", Weapon.Tags.Has( "aiming" ) ? 0.2f : 0f );

		// Handedness
		ModelRenderer.Set( "b_twohanded", true );

		// Weapon state
		ModelRenderer.Set( "b_empty", !Weapon.Components.Get<AmmoContainer>( FindMode.EnabledInSelfAndDescendants )?.HasAmmo ?? false );
	}

	protected override void OnUpdate()
	{
		// Reset every frame
		localRotation = Rotation.Identity;
		localPosition = Vector3.Zero;

		ApplyVelocity();
		ApplyStates();
		ApplyAnimationParameters();
		ApplyAnimationTransform();

		var baseFov = Preferences.FieldOfView;

		TargetFieldOfView = TargetFieldOfView.LerpTo( baseFov + FieldOfViewOffset, Time.Delta * 10f );
		FieldOfViewOffset = 0;
		ViewModelCamera.FieldOfView = TargetFieldOfView;

		lerpedlocalRotation = Rotation.Lerp( lerpedlocalRotation, localRotation, Time.Delta * 5f );
		lerpedLocalPosition = lerpedLocalPosition.LerpTo( localPosition, Time.Delta * 7f );

		Transform.LocalRotation = lerpedlocalRotation;
		Transform.LocalPosition = lerpedLocalPosition;
	}
}

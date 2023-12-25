using System.Numerics;

namespace Gunfight;

/// <summary>
/// A weapon's viewmodel. It's responsibility is to listen to events from a weapon.
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
	CameraController CameraController => Components.Get<CameraController>( FindMode.InAncestors );

	/// <summary>
	/// Looks up the tree to find the player controller.
	/// </summary>
	PlayerController PlayerController => Weapon.PlayerController;

	[Property] public SkinnedModelRenderer ModelRenderer { get; set; }

	/// <summary>
	/// The View Model camera - we'll turn this off if running as Proxy
	/// </summary>
	public CameraComponent ViewModelCamera { get; set; }

	protected override void OnStart()
	{
		if ( IsProxy )
		{
			// Disable ourselves if we're proxy. We don't want to see viewmodels of other people's stuff.
			// We might be spectating in the future - so work that out...
			ViewModelCamera.Enabled = false;
			Enabled = false;

			return;
		}

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
		var att = Weapon.ViewModel.ModelRenderer.SceneModel.GetBoneLocalTransform( "camera" );
		var attWorld = Weapon.ViewModel.ModelRenderer.SceneModel.GetBoneWorldTransform( "camera" );

		Log.Trace( "local: " + att.Position );
		Log.Trace( "world: " + attWorld.Position );
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
		if ( PlayerController.IsAiming ) wishLook = 0;

		lerpedWishLook = lerpedWishLook.LerpTo( wishLook, Time.Delta * 5.0f );

		localRotation *= Rotation.From( 0, -lerpedWishLook.y * 3f, 0 );
		localPosition += -lerpedWishLook;

		ModelRenderer.Set( "move_groundspeed", moveLen );
	}

	void ApplyStates()
	{
		if ( PlayerController.HasTag( "slide" ) )
		{
			localPosition += Vector3.Backward * 2f;
			localRotation *= Rotation.From( 10, 25, -5 );
		}
	}

	void ApplyAnimationParameters()
	{
		ModelRenderer.Set( "b_sprint", PlayerController.HasTag( "sprint" ) );
		ModelRenderer.Set( "b_grounded", PlayerController.IsGrounded );

		// Ironsights
		ModelRenderer.Set( "ironsights", PlayerController.IsAiming ? 2 : 0 );
		ModelRenderer.Set( "ironsights_fire_scale", PlayerController.IsAiming ? 0.3f : 1f );

		// Weapon state
		ModelRenderer.Set( "b_empty", !Weapon.Components.Get<AmmoContainer>()?.HasAmmo ?? false );
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

		lerpedlocalRotation = Rotation.Lerp( lerpedlocalRotation, localRotation, Time.Delta * 10f );
		lerpedLocalPosition = lerpedLocalPosition.LerpTo( localPosition, Time.Delta * 10f );

		Transform.LocalRotation = lerpedlocalRotation;
		Transform.LocalPosition = lerpedLocalPosition;
	}
}

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
		//var att = Weapon.ViewModel.Arms.GetBoneTransform( "camera", false );

		//if ( att is Transform transform )
		//{
		//	var cameraGameObject = Weapon.PlayerController.CameraGameObject;
		//	cameraGameObject.Transform.LocalPosition += transform.Position;
		//	cameraGameObject.Transform.LocalRotation *= transform.Rotation;
		//}
	}

	private Vector3 lerpedWishLook;
	protected void ApplyVelocity()
	{
		var moveVel = PlayerController.CharacterController.Velocity;
		var moveLen = moveVel.Length;
		if ( PlayerController.HasTag( "slide" ) ) moveLen = 0;

		var wishLook = PlayerController.WishMove.Normal * 1f;
		if ( PlayerController.IsAiming ) wishLook = 0;

		lerpedWishLook = lerpedWishLook.LerpTo( wishLook, Time.Delta * 5.0f );

		Transform.LocalRotation = Rotation.From( 0, -lerpedWishLook.y * 3f, 0 );
		Transform.LocalPosition = -lerpedWishLook;

		ModelRenderer.Set( "move_groundspeed", moveLen );
	}

	protected override void OnUpdate()
	{
		ApplyVelocity();


		ModelRenderer.Set( "b_sprint", PlayerController.HasTag( "sprint" ) );
		ModelRenderer.Set( "b_grounded", PlayerController.IsGrounded );

		// Ironsights
		ModelRenderer.Set( "ironsights", PlayerController.IsAiming ? 2 : 0 );
		ModelRenderer.Set( "ironsights_fire_scale", PlayerController.IsAiming ? 0.3f : 1f );

		// Weapon state
		ModelRenderer.Set( "b_empty", !Weapon.Components.Get<AmmoContainer>()?.HasAmmo ?? false );

		ApplyAnimationTransform();
	}
}

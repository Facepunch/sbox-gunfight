namespace Gunfight;

/// <summary>
/// A weapon's viewmodel. It's responsibility is to listen to events from a weapon.
/// </summary>
public partial class ViewModel : Component
{
	/// <summary>
	/// A reference to the <see cref="Weapon"/> we want to listen to.
	/// </summary>
	[Property] public Weapon Weapon { get; set; }

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
	[Property] public CameraComponent ViewModelCamera { get; set; }

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

	protected override void OnDestroy()
	{
		PlayerController.OnJump -= OnPlayerJumped;
	}

	void OnPlayerJumped()
	{
		ModelRenderer.Set( "b_jump", true );
	}

	protected override void OnUpdate()
	{
		ModelRenderer.Set( "move_groundspeed", PlayerController.CharacterController.Velocity.Length );
		ModelRenderer.Set( "b_sprint", PlayerController.IsRunning );
		ModelRenderer.Set( "b_grounded", PlayerController.IsGrounded );

		// Ironsights
		ModelRenderer.Set( "ironsights", PlayerController.IsAiming ? 2 : 1 );
		ModelRenderer.Set( "ironsights_fire_scale", PlayerController.IsAiming ? 0.3f : 1f );
	}
}

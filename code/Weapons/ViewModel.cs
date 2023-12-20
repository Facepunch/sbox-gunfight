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
	/// Look up the tree to find the camera.
	/// </summary>
	CameraController CameraController => Components.Get<CameraController>( FindMode.InAncestors );

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
		}
	}
}

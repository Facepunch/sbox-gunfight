namespace Gunfight;

public sealed class CameraController : Component
{
	/// <summary>
	/// A reference to the camera component we're going to be doing stuff with.
	/// </summary>
	[Property] public CameraComponent Camera { get; set; }

	protected override void OnAwake()
	{
		// Make sure the camera is disabled if we're not actively in charge of it.
		// Note: let's figure out spectator stuff in a nice way
		Camera.Enabled = !IsProxy;

		// If the camera is enabled, let's get rid of the player's body, otherwise it's gonna be in the way.
		if ( Camera.Enabled )
		{
			var playerController = Components.Get<PlayerController>();
			if ( playerController == null ) throw new ComponentNotFoundException( "CameraController - couldn't find PlayerController component." );

			// Disable the player's body so it doesn't render.
			playerController.Body.Enabled = false;
		}
	}
}

namespace Gunfight;

public sealed class CameraController : Component
{
	/// <summary>
	/// A reference to the camera component we're going to be doing stuff with.
	/// </summary>
	[Property] public CameraComponent camera { get; set; }

	protected override void OnAwake()
	{
		// Make sure the camera is disabled if we're not actively in charge of it.
		// Note: let's figure out spectator stuff in a nice way
		camera.Enabled = !IsProxy;
	}
}

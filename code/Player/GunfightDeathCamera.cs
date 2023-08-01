namespace Facepunch.Gunfight;

// TODO - Set up this
public partial class GunfightDeathCamera : GunfightCamera
{
	public GunfightDeathCamera() { }

	public GunfightDeathCamera( Entity entity )
	{
		FocusEntity = entity;
	}

	Entity FocusEntity { get; set; }

	Vector3 FocusPoint => FocusEntity?.AimRay.Position ?? Camera.Position;
	Rotation FocusRotation => Rotation.LookAt( FocusEntity?.AimRay.Forward ?? Vector3.Forward );

	public override void Update()
	{
		if ( CameraOverride != null ) { base.Update(); return; }

		var player = Game.LocalClient;
		if ( player == null ) return;

		Camera.Position = FocusPoint + GetViewOffset();
		Camera.Rotation = Rotation.LookAt( -FocusRotation.Forward );

		Camera.FirstPersonViewer = null;
	}

	protected virtual Vector3 GetViewOffset()
	{
		return FocusRotation.Forward * 100f;
	}
}

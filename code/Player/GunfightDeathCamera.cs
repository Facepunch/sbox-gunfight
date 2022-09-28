namespace Facepunch.Gunfight;

public partial class GunfightDeathCamera : CameraMode
{
	public GunfightDeathCamera() { }

	public GunfightDeathCamera( Entity entity )
	{
		FocusEntity = entity;
	}

	[Net] Entity FocusEntity { get; set; }

	Vector3 FocusPoint => FocusEntity?.EyePosition ?? CurrentView.Position;
	Rotation FocusRotation => FocusEntity?.Rotation ?? CurrentView.Rotation;

	public override void Activated()
	{
		base.Activated();

		FieldOfView = CurrentView.FieldOfView;
	}

	public override void Update()
	{
		var player = Local.Client;
		if ( player == null ) return;

		Position = FocusPoint + GetViewOffset();
		Rotation = Rotation.LookAt( -FocusRotation.Forward );

		Viewer = null;
	}

	public virtual Vector3 GetViewOffset()
	{
		return FocusRotation.Forward * 100f;
	}
}

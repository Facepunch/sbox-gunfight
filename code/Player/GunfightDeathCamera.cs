namespace Facepunch.Gunfight;

public partial class GunfightDeathCamera : GunfightCamera
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
		if ( CameraOverride != null ) { base.Update(); return; }

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

	protected void Spectate()
	{
		Local.Pawn.Components.Add( new GunfightSpectatorCamera() );
	}

	public override void BuildInput()
	{
		base.BuildInput();

		var gamemode = GamemodeSystem.Current;

		if ( Input.Pressed( InputButton.Jump ) && gamemode.IsValid() && gamemode.AllowSpectating )
		{
			Spectate();
		}
	}
}

namespace Facepunch.Gunfight;

public partial class GunfightSpectatorPlayer : AnimatedEntity
{
	public GunfightCamera PlayerCamera { get; set; } = new GunfightSpectatorCamera();

	public override void FrameSimulate( IClient cl )
	{
		PlayerCamera?.Update();
	}
	public override void BuildInput()
	{
		PlayerCamera?.BuildInput();
	}
}

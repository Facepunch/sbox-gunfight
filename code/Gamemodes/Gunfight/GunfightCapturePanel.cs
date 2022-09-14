using Sandbox.UI;

namespace Facepunch.Gunfight;

[UseTemplate]
public partial class GunfightCapturePanel : Panel
{
	protected GunfightGamemode Gamemode => GamemodeSystem.Current as GunfightGamemode;
	public CapturePointEntity Flag => Gamemode?.ActiveFlag;
	public float CaptureAmount => Flag?.Captured ?? 0f;

	// @ref
	public Panel Bar { get; set; }

	public void Update()
	{
		BindClass( "show", () => Flag.IsValid() );
	}

	protected override void PostTemplateApplied()
	{
		base.PostTemplateApplied();
		Update();
	}

	public override void Tick()
	{
		if ( !Flag.IsValid() ) return;

		Bar.Style.Width = Length.Fraction( CaptureAmount );
	}
}

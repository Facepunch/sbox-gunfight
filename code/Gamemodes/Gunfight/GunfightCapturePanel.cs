using Sandbox.UI;

namespace Facepunch.Gunfight;

[UseTemplate]
public partial class GunfightCapturePanel : Panel
{
	protected GunfightGamemode Gamemode => GamemodeSystem.Current as GunfightGamemode;
	public CapturePointEntity Flag => Gamemode?.ActiveFlag ?? ( Local.Pawn as GunfightPlayer )?.CapturePoint;
	public float CaptureAmount => Flag?.Captured ?? 0f;

	// @ref
	public Panel Bar { get; set; }

	bool ShouldShow()
	{
		if ( !Flag.IsValid() ) return false;
		if ( Flag.CurrentState == CapturePointEntity.CaptureState.None && Flag.Team == TeamSystem.MyTeam ) return false;

		return true;
	}

	public void Update()
	{
		BindClass( "show", ShouldShow );
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

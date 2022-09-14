using Sandbox.UI;

namespace Facepunch.Gunfight;

[UseTemplate]
public partial class GunfightRoundWonPanel : Panel
{
	protected GunfightGamemode Gamemode => GamemodeSystem.Current as GunfightGamemode;

	// @text
	public string MyScore => $"{GunfightGame.Current.Scores.GetScore( TeamSystem.MyTeam )}";
	// @text
	public string EnemyScore => $"{GunfightGame.Current.Scores.GetScore( TeamSystem.GetEnemyTeam( TeamSystem.MyTeam ) )}";

	public void Update()
	{
		SetClass( "show", true );
	}

	protected override void PostTemplateApplied()
	{
		base.PostTemplateApplied();
		Update();
	}

	public static async void ShowFor( float seconds )
	{
		var pnl = GunfightGame.Current.Hud.RootPanel.AddChild<GunfightRoundWonPanel>();
		await GameTask.DelaySeconds( seconds );
		pnl?.Delete();
	}

	[ConCmd.Client( "gunfight_debug_show_wonpanel" )]
	public static void Show()
	{
		ShowFor( 5f );
	}
}

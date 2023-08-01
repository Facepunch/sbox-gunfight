namespace Facepunch.Gunfight.UI;

public partial class LoadoutPanel
{
	public static LoadoutPanel Current;

	[ConCmd.Client( "gunfight_debug_loadoutpanel" )]
	public static void Show()
	{
		if ( Current is not null ) Current.Delete( true );

		Current = GunfightGame.Current.Hud.RootPanel.AddChild<LoadoutPanel>();
	}

	[ClientRpc]
	public static void RpcShow()
	{
		// LoadoutPanel.Show();
	}
}

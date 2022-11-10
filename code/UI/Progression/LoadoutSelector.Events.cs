namespace Facepunch.Gunfight.UI;

public partial class LoadoutSelector
{
    public static LoadoutSelector  Current;
    
    [ClientRpc]
    public static void Show()
    {
        if ( Current != null ) Current.Delete( true );

        var rootPanel = GunfightGame.Current.Hud.RootPanel;
        Current = rootPanel.AddChild<LoadoutSelector>();
    }

    [ConCmd.Client( "gunfight_debug_ui_loadoutselector" )]
    public static void DebugShow()
    {
        Show();
    }
}
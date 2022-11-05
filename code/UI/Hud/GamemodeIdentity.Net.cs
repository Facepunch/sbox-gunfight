using Sandbox;
using Sandbox.UI;

namespace Facepunch.Gunfight.UI;

public partial class GamemodeIdentity
{
    protected static async Task Decay( Panel panel, int seconds = 5 )
    {
        await GameTask.DelaySeconds( seconds );
        panel.Delete();
    }

    [ClientRpc]
    public static void RpcShow( int seconds = 5 )
    {
        Host.AssertClient();

        var panel = GunfightGame.Current.Hud.RootPanel.AddChild<GamemodeIdentity>();
        _ = Decay( panel, seconds );
    }

    [ConCmd.Admin( "gunfight_debug_ui_ident" )]
    public static void Debug()
    {
        RpcShow( To.Everyone );
    }
}
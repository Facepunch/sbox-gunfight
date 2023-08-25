namespace Facepunch.Gunfight.UI;

public partial class UIGlobals
{
    [ConVar.Client( "gunfight_menu_scale" )]
    public static float MenuScale { get; set; } = 1f;

	[ConVar.Client( "gunfight_game_scale" )]
    public static float GameScale { get; set; } = 0.8f;
}
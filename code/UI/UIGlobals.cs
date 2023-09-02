namespace Facepunch.Gunfight.UI;

public partial class UIGlobals
{
    public static float MenuScale => 0.85f;

	[ConVar.Client( "gunfight_game_scale" )]
    public static float GameScale { get; set; } = 0.8f;
}

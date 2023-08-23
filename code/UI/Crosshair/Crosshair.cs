namespace Facepunch.Gunfight.UI;

public partial class Crosshair
{
    [ConVar.Client( "gunfight_crosshairdot" )]
    public static bool UseDot { get; set; } = true;
}

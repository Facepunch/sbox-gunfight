namespace Facepunch.Gunfight.UI;

public partial class ExperienceHints 
{
    [ClientRpc]
    public static void RpcSend( int xp, string text, int lifetime = 5 )
    {
        Current?.AddHint( xp, text, lifetime );
    }
}

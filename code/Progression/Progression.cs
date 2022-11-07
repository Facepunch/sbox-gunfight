namespace Facepunch.Gunfight;

public partial class Progression
{
    public static void GiveXP( Client cl, int amount, string reason )
    {
        // TODO - Do something with this
    }

    public static void GiveAward( Client cl, string awardTitle )
    {
        var award = Awards.Get( awardTitle );
		if ( award is null )
			return;

        award?.Invoke( null, new[] { cl.Pawn as GunfightPlayer } );

        Log.Info( $"Progression: Given award '{award.Title}' to {cl.Name}");

		RpcGiveAward( To.Single( cl ), award.Title );
    }
    
    [ClientRpc]
    public static void RpcGiveAward( string awardTitle )
    {
        // TODO - Hook up to UI
    }
}
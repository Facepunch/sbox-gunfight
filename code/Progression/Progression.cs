namespace Facepunch.Gunfight;

public partial class Progression
{
    public static void GiveXP( Client cl, int amount, string reason )
    {
        UI.ExperienceHints.RpcSend( To.Single( cl ), amount, reason );
    }

    public static void GiveScore( Client cl, int amount )
    {
        if ( amount < 1 ) return;

        cl.AddInt( "score", amount );
    }

    public static void GiveAward( Client cl, string awardTitle )
    {
        var award = Awards.Get( awardTitle );
		if ( award is null )
			return;

        award?.Invoke( null, new[] { cl.Pawn as GunfightPlayer } );

        var attr = award.GetCustomAttribute<AwardAttribute>();
        
        Log.Info( $"Progression: Given award '{award.Title}' to {cl.Name}");

        GiveScore( cl, attr.PointsGiven );
		RpcGiveAward( To.Single( cl ), award.Title );

        if ( attr.ShareXP )
            GiveXP( cl, attr.PointsGiven, attr.Description );
    }
    
    [ClientRpc]
    public static void RpcGiveAward( string awardTitle )
    {
        // TODO - Hook up to UI
    }
}
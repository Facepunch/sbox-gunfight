namespace Facepunch.Gunfight;

public partial class Progression
{
    public static void GiveXP( IClient cl, int amount, string reason )
    {
        UI.ExperienceHints.RpcSend( To.Single( cl ), amount, reason );
		RpcGiveXP( To.Single( cl ), amount, reason );
    }

	[ClientRpc]
	public static void RpcGiveXP( int amount, string reason )
	{
		Progression.Levelling.TotalExperience += amount;
	}

	/// <summary>
	/// Tell the server what your level is
	/// </summary>
	/// <param name="cl"></param>
	/// <param name="amount"></param>
	[ConCmd.Server( "gunfight_progression_broadcast_lvl" )]
	public static void BroadcastLevel( int level )
	{
		ConsoleSystem.Caller.Components.GetOrCreate<PlayerLevelComponent>().Level = level;
	}

    public static void GiveScore( IClient cl, int amount )
    {
        if ( amount < 1 ) return;

        cl.AddInt( "score", amount );
    }

    public static void GiveAward( IClient cl, string awardTitle )
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
        if ( awardTitle == "Kill" )
		{
			Sound.FromScreen( "sounds/guns/kill.sound" );
		}
		if ( awardTitle == "Kill Confirmed" )
		{
			Sound.FromEntity( "sounds/kc/coin.pickup.sound", Game.LocalPawn );
		}
    }
}

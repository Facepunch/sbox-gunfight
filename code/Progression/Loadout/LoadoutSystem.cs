namespace Facepunch.Gunfight;

public partial class LoadoutSystem
{
    public static Loadout MatchLoadout = null;
    public static bool AllowCustomLoadouts = false;
    public static string LoadoutTag = "";

    [ConCmd.Server( "gunfight_loadout_setpreference" )]
    public static void SetPreference( string name )
    {
		var first = Loadout.All.FirstOrDefault( x => x.ResourceName == name );
		if ( first == null )
			return;

        var cl = ConsoleSystem.Caller;
        
        cl.GetLoadoutComponent().Loadout = first;
		UI.NotificationManager.AddNotification( To.Single( cl ), UI.NotificationDockType.BottomMiddle, $"Your loadout will change when you next respawn.", 5 );
    }

    public static Loadout GetLoadout( Client cl )
    {
        if ( !AllowCustomLoadouts )
        {
            return MatchLoadout;
        }  

        return GetPreference( cl ) ?? MatchLoadout;
    }

    public static Loadout GetPreference( Client cl )
    {
        return cl.GetLoadoutComponent().Loadout;
    }
}
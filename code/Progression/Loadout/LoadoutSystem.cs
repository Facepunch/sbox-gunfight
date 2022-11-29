namespace Facepunch.Gunfight;

public partial class LoadoutSystem : Entity
{
    private static LoadoutSystem Instance;

    [Net] private Loadout matchLoadout { get; set; }
    [Net] private bool allowCustomLoadouts { get; set; }

    public static Loadout MatchLoadout
    {
        get => Instance.matchLoadout;
        set => Instance.matchLoadout = value;
    }
    
    public static bool AllowCustomLoadouts
    {
        get => Instance.allowCustomLoadouts;
        set => Instance.allowCustomLoadouts = value;
    }

    public override void Spawn()
    {
        Transmit = TransmitType.Always;
        Instance = this;
    }

    public override void ClientSpawn()
    {
        Instance = this;
    }

    [ConCmd.Server( "gunfight_loadout_setpreference" )]
    public static void SetPreference( string name )
    {
		var first = Loadout.All.FirstOrDefault( x => x.ResourceName == name );
		if ( first == null )
			return;

        var cl = ConsoleSystem.Caller;
        
        cl.GetLoadoutComponent().Loadout = first;
		UI.NotificationManager.AddNotification( To.Single( cl ), UI.NotificationDockType.BottomMiddle, $"Your loadout will be set to {first.LoadoutName} when you next respawn.", 5 );
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

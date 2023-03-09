namespace Facepunch.Gunfight;

public partial class LoadoutComponent : EntityComponent
{
    // Current loadout preference for this player
    [Net] public Loadout Loadout { get; set; }

    public void Give()
    {
        var player = Entity as GunfightPlayer;
        Loadout?.Give( player );
    }
}

public static class ClientExtensions
{
    public static LoadoutComponent GetLoadoutComponent( this IClient cl )
    {
        return cl.Components.GetOrCreate<LoadoutComponent>();
    }
}

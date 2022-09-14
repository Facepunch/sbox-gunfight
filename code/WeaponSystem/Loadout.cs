namespace Facepunch.Gunfight;

[GameResource( "Gunfight Loadout", "ldt", "A loadout resource for Gunfight" )]
public partial class Loadout : GameResource
{
	public static List<Loadout> All = new();

	/// <summary>
	/// Loadouts can have tags on them, which can be queried by gamemodes.
	/// </summary>
	public List<string> Tags { get; set; }

	// Data
	public WeaponDefinition PrimaryWeapon { get; set; }
	public WeaponDefinition SecondaryWeapon { get; set; }
	public WeaponDefinition MeleeWeapon { get; set; }
	public List<WeaponDefinition> Gadgets { get; set; }
	public Dictionary<AmmoType, int> Ammo { get; set; }

	protected override void PostLoad()
	{
		base.PostLoad();

		Log.Info( $"Registering loadout ({ResourcePath})" );

		if ( !All.Contains( this ) )
			All.Add( this );
	}

	/// <summary>
	/// Fetch loadouts from a specified tag
	/// </summary>
	/// <param name="tag"></param>
	/// <returns></returns>
	public static IEnumerable<Loadout> WithTag( string tag )
	{
		return All.Where( x => x.Tags.Contains( tag ) );
	}

	/// <summary>
	/// Applies the loadout onto a player
	/// </summary>
	/// <param name="player"></param>
	public void Give( GunfightPlayer player )
	{
		if ( PrimaryWeapon != null )
			player.GiveWeapon( PrimaryWeapon, true );
		if ( SecondaryWeapon != null )
			player.GiveWeapon( SecondaryWeapon );
		if ( MeleeWeapon != null )
			player.GiveWeapon( MeleeWeapon );

		foreach( var kv in Ammo )
		{
			player.GiveAmmo( kv.Key, kv.Value );
		}
	}

	public override string ToString() =>  $"GunfightLoadout[{ResourceName}]";
}

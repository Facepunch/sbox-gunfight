using System.ComponentModel;

namespace Facepunch.Gunfight;

[GameResource( "Weapon Definition", "wpn", "" )]
public partial class WeaponDefinition : GameResource
{
	public static GunfightWeapon CreateWeapon( WeaponDefinition def )
	{
		var weapon = new GunfightWeapon();
		weapon.WeaponDefinition = def;

		Log.Info( $"Created weapon: {def.WeaponName}" );

		return weapon;
	}

	public static WeaponDefinition Find( string search )
	{
		if ( Index.TryGetValue( search, out var weaponDef ) )
			return weaponDef;

		// Try to search via short name
		weaponDef = All.FirstOrDefault( x => x.WeaponShortName.ToLower() == search.ToLower() );
		if ( weaponDef != null ) return weaponDef;

		// Try to search via full name
		weaponDef = All.FirstOrDefault( x => x.WeaponName.ToLower() == search.ToLower() );
		if ( weaponDef != null ) return weaponDef;

		return null;
	}

	public static GunfightWeapon CreateWeapon( string identifier )
	{
		var def = Find( identifier );
		if ( def != null )
			return CreateWeapon( def );

		Log.Warning( $"Failed to find weapon with identifier: {identifier}" );

		return null;
	}

	public static Dictionary<string, WeaponDefinition> Index = new();
	public static List<WeaponDefinition> All = new();

	[Category( "Setup" )]
	public string WeaponName { get; set; } = "Weapon";

	[Category( "Setup" )]
	public string WeaponShortName { get; set; } = "";

	[Category( "Setup" )]
	public WeaponSlot Slot { get; set; } = WeaponSlot.Primary;

	[Category( "Setup" ), ResourceType( "vmdl" )]
	public string Model { get; set; }

	public Model CachedModel;

	[Category( "Setup" ), ResourceType( "vmdl" )]
	public string ViewModel { get; set; }

	public Model CachedViewModel;

	[Category( "Setup" ), ResourceType( "jpg" )]
	public string Icon { get; set; }

	[Category( "View Model" )]
	public ViewModelSetup ViewModelSetup { get; set; }

	[Category( "Shooting" )]
	public float BaseFireRate { get; set; } = 1f;

	[Category( "General" )]
	public HoldType HoldType { get; set; } = HoldType.Pistol;

	protected override void PostLoad()
	{
		base.PostLoad();

		Log.Info( $"Registering weapon definition ({ResourcePath}, {WeaponName})" );

		if ( !All.Contains( this ) )
			All.Add( this );

		if ( !Index.ContainsKey( ResourcePath ) )
			Index.Add( ResourcePath, this );

		if ( !string.IsNullOrEmpty( Model ) )
			CachedModel = Sandbox.Model.Load( Model );

		if ( !string.IsNullOrEmpty( ViewModel ) )
			CachedViewModel = Sandbox.Model.Load( ViewModel );
	}
}

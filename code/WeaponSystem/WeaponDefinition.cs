using System.ComponentModel;

namespace Facepunch.Gunfight;

[GameResource( "Weapon Definition", "wpn", "" )]
public partial class WeaponDefinition : GameResource
{
	public static GunfightWeapon CreateWeapon( WeaponDefinition def )
	{
		var weapon = new GunfightWeapon
		{
			WeaponDefinition = def
		};

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

	[Category( "Setup" )]
	public HoldType HoldType { get; set; } = HoldType.Pistol;

	[Category( "Setup" ), ResourceType( "vmdl" )]
	public string Model { get; set; }

	public Model CachedModel;

	[Category( "Setup" ), ResourceType( "vmdl" )]
	public string ViewModel { get; set; }

	public Model CachedViewModel;

	[Category( "Setup" ), ResourceType( "jpg" )]
	public string Icon { get; set; }

	[Category( "Shooting" )]
	public float BaseFireRate { get; set; } = 1f;

	[Category( "Shooting" )]
	public FireMode DefaultFireMode { get; set; } = FireMode.FullAuto;

	[Category( "Shooting" )]
	public List<FireMode> SupportedFireModes { get; set; }

	[ShowIf( "DefaultFireMode", FireMode.Burst )]
	public int BurstAmount { get; set; } = 3;

	[Category( "Shooting" ), ResourceType( "sound" )]
	public string FireSound { get; set; } = "";

	public ViewModelSetup ViewModelSetup { get; set; }

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
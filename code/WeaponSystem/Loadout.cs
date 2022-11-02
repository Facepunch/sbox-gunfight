using System.Text.Json.Serialization;

namespace Facepunch.Gunfight;

public struct LoadoutSlot
{
	[HideInEditor, JsonIgnore] public bool IsSet => !string.IsNullOrEmpty( WeaponName );
	[HideInEditor, JsonIgnore] public WeaponDefinition Definition => WeaponDefinition.Find( WeaponName );
	[HideInEditor, JsonIgnore] public string NiceName => Definition.WeaponName;
	[HideInEditor, JsonIgnore] public bool HasAttachments => Attachments != null && Attachments.Count > 0;

	public string WeaponName { get; set; }
	public List<string> Attachments { get; set; }
}

[GameResource( "Gunfight Loadout", "ldt", "A loadout resource for Gunfight", Icon = "checklist", IconBgColor = "#62945c", IconFgColor = "#335c2e" )]
public partial class Loadout : GameResource
{
	public static List<Loadout> All = new();

	/// <summary>
	/// A nice name for the loadout. Shown in UI.
	/// </summary>
	[Category( "Setup" )]
	public string LoadoutName { get; set; } = "My Loadout";

	/// <summary>
	/// Loadouts can have tags on them, which can be queried by gamemodes.
	/// </summary>
	[Category( "Setup" )]
	public List<string> Tags { get; set; }

	// Data
	public LoadoutSlot PrimaryWeapon { get; set; }
	public LoadoutSlot SecondaryWeapon { get; set; }

	[Category( "Setup" )]
	public Dictionary<AmmoType, int> Ammo { get; set; } = new();

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
		if ( PrimaryWeapon.IsSet )
			player.GiveWeapon( PrimaryWeapon.Definition, true, PrimaryWeapon.Attachments.ToArray() );
		if ( SecondaryWeapon.IsSet )
			player.GiveWeapon( SecondaryWeapon.Definition, false, SecondaryWeapon.Attachments.ToArray() );

		foreach( var kv in Ammo )
			player.GiveAmmo( kv.Key, kv.Value );
	}

	public override string ToString() =>  $"GunfightLoadout[{ResourceName}]";

	[ConCmd.Admin( "gunfight_giveloadout" )]
	public static void Cmd_GiveLoadout( string name )
	{
		Host.AssertServer();

		var player = ConsoleSystem.Caller.Pawn as GunfightPlayer;

		var first = All.FirstOrDefault( x => x.ResourceName == name );
		if ( first == null )
			return;

		first.Give( player );
	}
}

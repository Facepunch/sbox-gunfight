
[GameResource( "Weapon Data", "weapon", "A resource containing basic information about a weapon." )]
public partial class WeaponDataResource : GameResource
{
	public static HashSet<WeaponDataResource> All { get; set; } = new();

	public string Name { get; set; } = "My Weapon";
	public string Description { get; set; } = "";

	/// <summary>
	/// The prefab to create and attach to the player when spawning it in.
	/// </summary>
	public PrefabFile MainPrefab { get; set; }

	/// <summary>
	/// The prefab to create when making a viewmodel for this weapon.
	/// </summary>
	public PrefabFile ViewModelPrefab { get; set; }

	protected override void PostLoad()
	{
		if ( All.Contains( this ) )
		{
			Log.Warning( "Tried to add two of the same weapon (?)" );
			return;
		}

		All.Add( this );
	}
}

namespace Gunfight;

public sealed class PlayerLoadout : Component
{
	[RequireComponent] public PlayerController Player { get; set; }

	/// <summary>
	/// The weapon to create for this player.
	/// </summary>
	[Property] public WeaponDataResource Weapon { get; set; }

	/// <summary>
	/// A <see cref="GameObject"/> that will hold all of our weapons.
	/// </summary>
	[Property] public GameObject WeaponGameObject { get; set; }

	protected override void OnStart()
	{
		if ( IsProxy )
			return;

		GiveWeapon( Weapon );
	}
	
	void GiveWeapon( WeaponDataResource weapon, bool makeActive = true )
	{
		// If we're in charge, let's make some weapons.
		if ( Weapon == null )
		{
			Log.Warning( "A player loadout without a weapon? Nonsense." );
			return;
		}

		if ( !Weapon.MainPrefab.IsValid() )
		{
			Log.Error( "Weapon doesn't have a prefab?" );
			return;
		}

		// Create the weapon prefab and put it on the weapon gameobject.
		var weaponGameObject = Weapon.MainPrefab.Clone( new CloneConfig()
		{
			Transform = new Transform(),
			Parent = WeaponGameObject,
			StartEnabled = true,
		} );
		var weaponComponent = weaponGameObject.Components.Get<Weapon>();
		
		weaponGameObject.NetworkSpawn();

		Player.CurrentWeapon = weaponComponent;
	}
}

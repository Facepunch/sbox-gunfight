namespace Gunfight;

public sealed class PlayerLoadout : Component
{
	/// <summary>
	/// The weapon to create for this player.
	/// </summary>
	[Property] public WeaponDataResource Weapon { get; set; }

	/// <summary>
	/// A <see cref="GameObject"/> that will hold all of our weapons.
	/// </summary>
	[Property] public GameObject WeaponGameObject { get; set; }

	/// <summary>
	/// A <see cref="GameObject"/> that will hold our ViewModel.
	/// </summary>
	[Property] public GameObject ViewModelGameObject { get; set; }

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

		if ( Weapon.MainPrefab == null )
		{
			Log.Error( "Weapon doesn't have a prefab?" );
			return;
		}

		// Create the weapon prefab and put it on the weapon gameobject.
		var weaponGameObject = PrefabUtility.CreateGameObject( Weapon.MainPrefab, WeaponGameObject );
		var weaponComponent = weaponGameObject.Components.Get<Weapon>();

		// 
		if ( !makeActive ) 
			return;

		foreach ( var child in ViewModelGameObject.Children )
		{
			child.DestroyImmediate();
		}

		if ( Weapon.ViewModelPrefab != null )
		{
			// Create the weapon prefab and put it on the weapon gameobject.
			var viewModelGameObject = PrefabUtility.CreateGameObject( Weapon.ViewModelPrefab, ViewModelGameObject );
			var viewModelComponent = viewModelGameObject.Components.Get<ViewModel>();

			// Weapon needs to know about the ViewModel
			weaponComponent.ViewModel = viewModelComponent;
		}
	}
}

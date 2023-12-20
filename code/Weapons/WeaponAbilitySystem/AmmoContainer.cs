namespace Gunfight;

/// <summary>
/// An ammo container.
/// </summary>
public partial class AmmoContainer : Component
{
	private int ammo = 0;

	/// <summary>
	/// How much ammo are we holding?
	/// </summary>
	[Property] public int Ammo
	{
		get => ammo;
		set
		{
			ammo = value;
		}
	}

	/// <summary>
	/// Do we have any ammo?
	/// </summary>
	[Property] public bool HasAmmo => Ammo > 0;
}

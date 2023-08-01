namespace Facepunch.Gunfight;

public partial class GunfightPlayer
{
	[Net, Local] public IList<int> Ammo { get; set; }

	protected void ClearAmmo()
	{
		Ammo.Clear();
	}

	public int AmmoCount( AmmoType type )
	{
		var iType = (int)type;
		if ( Ammo == null ) return 0;
		if ( Ammo.Count <= iType ) return 0;

		return Ammo[(int)type];
	}

	public bool SetAmmo( AmmoType type, int amount )
	{
		var iType = (int)type;
		if ( !Game.IsServer ) return false;
		if ( Ammo == null ) return false;

		while ( Ammo.Count <= iType )
		{
			Ammo.Add( 0 );
		}

		Ammo[(int)type] = amount;
		return true;
	}

	public int GiveAmmo( AmmoType type, int amount )
	{
		if ( !Game.IsServer ) return 0;
		if ( Ammo == null ) return 0;
		if ( type == AmmoType.None ) return 0;

		var total = AmmoCount( type ) + amount;
		var max = MaxAmmo( type );

		if ( total > max ) total = max;
		var taken = total - AmmoCount( type );

		SetAmmo( type, total );
		return taken;
	}

	public int TakeAmmo( AmmoType type, int amount )
	{
		if ( Ammo == null ) return 0;

		var available = AmmoCount( type );
		amount = Math.Min( available, amount );

		SetAmmo( type, available - amount );
		return amount;
	}

	public int MaxAmmo( AmmoType ammo )
	{
		return ammo switch
		{
			AmmoType.Pistol => 64,
			AmmoType.SMG => 128,
			AmmoType.Rifle => 96,
			AmmoType.DMR => 60,
			AmmoType.Sniper => 20,
			AmmoType.Shotgun => 40,
			_ => 64
		};
	}
}

public enum AmmoType
{
	None,
	Pistol,
	SMG,
	Rifle,
	DMR,
	Sniper,
	Shotgun
}

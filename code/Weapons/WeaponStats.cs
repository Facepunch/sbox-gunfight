namespace Gunfight;

/// <summary>
/// A bunch of weapon stats. Add more at the bottom - do not add more inbetween otherwise 
/// you will break all all weapon stats.
/// </summary>
public enum WeaponStat
{
	/// <summary>
	/// The base damage for any weapon.
	/// </summary>
	BaseDamage,
	/// <summary>
	/// The aim down sights speed.
	/// </summary>
	[Title( "Aim Down Sights Speed" )]
	ADSSpeed,
	/// <summary>
	/// The fire rate for this weapon. (in RPM)
	/// </summary>
	FireRate,
	/// <summary>
	/// How long (in seconds) it takes to reload.
	/// </summary>
	ReloadSpeed,
	/// <summary>
	/// The horizontal recoil.
	/// </summary>
	HorizontalRecoil,
	/// <summary>
	/// The vertical recoil.
	/// </summary>
	VerticalRecoil,
	/// <summary>
	/// How much spread. (random in sphere)
	/// </summary>
	Spread
}

public struct WeaponStats
{
	[Category( "Damage" )]
	public float BaseDamage { get; set; }
	
	[Category( "Damage" )]
	public Curve BaseDamageFalloff { get; set; }

	[Category( "Timing" )]
	public float AimSpeed { get; set; }
	
	[Category( "Timing" )]
	public float FireRate { get; set; }

	[Category( "Timing" )]
	public float ReloadSpeed { get; set; }

	[Category( "Recoil" )]
	public float HorizontalRecoil { get; set; }

	[Category( "Recoil" )]
	public float VerticalRecoil { get; set; }
}

[GameResource( "Gunfight/Weapon Stats", "wpnstat", "", IconBgColor = "#E07058", Icon = "bar_chart" )]
public partial class WeaponStatsResource : GameResource
{
	public WeaponStats Stats { get; set; }
}

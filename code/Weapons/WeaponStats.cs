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

public struct StatEntry
{
	public WeaponStat Stat { get; set; }
	public float Value { get; set; }
}

public struct WeaponStats
{
	public WeaponStats()
	{
	}

	public List<StatEntry> Stats { get; set; } = new();

	/// <summary>
	/// Fetches a stat from <see cref="Stats"/> and returns a fallback value if it's not set by a developer.
	/// </summary>
	/// <param name="stat"></param>
	/// <param name="fallback"></param>
	/// <returns></returns>
	public float Get( WeaponStat stat, float fallback = 0.0f )
	{
		if ( Stats.FirstOrDefault( x => x.Stat == stat ) is StatEntry entry )
		{
			return entry.Value;
		}

		return fallback;
	}
}

[GameResource( "Gunfight/Weapon Stats", "wpnstat", "", IconBgColor = "#E07058", Icon = "bar_chart" )]
public partial class WeaponStatsResource : GameResource
{
	public WeaponStats Stats { get; set; }
}

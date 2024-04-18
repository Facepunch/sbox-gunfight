namespace Gunfight;

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
	public RangedFloat HorizontalSpread { get; set; }

	[Category( "Recoil" )]
	public RangedFloat VerticalSpread { get; set; }

	/// <summary>
	/// Combines one WeaponStats resource with another.
	/// </summary>
	/// <param name="b"></param>
	/// <returns></returns>
	public WeaponStats Combine( WeaponStats b )
	{
		var a = this;

		var hSpreadVec2 = a.HorizontalSpread.RangeValue + b.HorizontalSpread.RangeValue;
		var vSpreadVec2 = a.VerticalSpread.RangeValue + b.VerticalSpread.RangeValue;

		var newStats = new WeaponStats()
		{
			BaseDamage = a.BaseDamage + b.BaseDamage,
			AimSpeed = a.AimSpeed + b.AimSpeed,
			FireRate = a.FireRate + b.FireRate,
			ReloadSpeed = a.ReloadSpeed + b.ReloadSpeed,
			HorizontalSpread = new RangedFloat( hSpreadVec2.x, hSpreadVec2.y ),
			VerticalSpread = new RangedFloat( vSpreadVec2.x, vSpreadVec2.y )
		};

		// todo: figure out falloff combo

		return newStats;
	}

	public static WeaponStats operator+(WeaponStats a, WeaponStats b)
	{
		return a.Combine( b );
	}
}

[GameResource( "Gunfight/Weapon Stats", "wpnstat", "", IconBgColor = "#E07058", Icon = "bar_chart" )]
public partial class WeaponStatsResource : GameResource
{
	public WeaponStats Stats { get; set; }
}

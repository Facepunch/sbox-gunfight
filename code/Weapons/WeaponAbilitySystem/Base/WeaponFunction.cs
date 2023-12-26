namespace Gunfight;

[Icon( "track_changes" )]
public abstract class WeaponFunction : Component
{
	public virtual string Name => "Weapon Function";

	/// <summary>
	/// Find the weapon, it's going to be a component on the same GameObject.
	/// </summary>
	public Weapon Weapon => Components.Get<Weapon>( FindMode.EverythingInSelfAndAncestors );

	/// <summary>
	/// Gets a reference to the weapon's stats.
	/// </summary>
	public WeaponStats? Stats => Weapon.Stats; 

	/// <summary>
	/// A reference to a stats resource which will get combined with other stats resources and applied to the weapon.
	/// </summary>
	[Property, Category( "Base" )] public WeaponStatsResource StatsResource { get; set; }

	/// <summary>
	/// Called on start, or when a weapon gets a stats update.
	/// </summary>
	internal virtual void UpdateStats()
	{
	}

	protected override void OnAwake()
	{      
		// Call the weapon to update its stats 
		if ( StatsResource is not null )
		{
			Weapon.UpdateStats();
		}
		else
		{
			UpdateStats();
		}
	}
}

namespace Gunfight;

[Icon( "track_changes" )]
public abstract class WeaponFunction : Component
{
	public virtual string Name => "Weapon Function";

	/// <summary>
	/// Find the weapon, it's going to be a component on the same GameObject.
	/// </summary>
	public Weapon Weapon => Components.Get<Weapon>( FindMode.EverythingInSelfAndAncestors );

	protected void BindTag( string tag, Func<bool> predicate ) => Weapon.BindTag( tag, predicate );
}

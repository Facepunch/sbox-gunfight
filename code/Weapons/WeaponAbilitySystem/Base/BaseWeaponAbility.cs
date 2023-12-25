namespace Gunfight;

public abstract class BaseWeaponAbility : Component
{
	public virtual string Name => "Weapon Ability";

	/// <summary>
	/// Find the weapon, it's going to be a component on the same GameObject.
	/// </summary>
	public Weapon Weapon => Components.Get<Weapon>( FindMode.InSelf );

	/// <summary>
	/// Gets a reference to the weapon's stats.
	/// </summary>
	public WeaponStats? Stats => Weapon.Stats; 
}

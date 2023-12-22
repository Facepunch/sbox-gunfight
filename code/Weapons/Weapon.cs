namespace Gunfight;

/// <summary>
/// A weapon component.
/// </summary>
public partial class Weapon : Component
{
	/// <summary>
	/// A reference to the weapon's <see cref="WeaponDataResource"/>.
	/// </summary>
	[Property] public WeaponDataResource Resource { get; set; }

	/// <summary>
	/// A reference to the weapon's model renderer.
	/// </summary>
	[Property] public SkinnedModelRenderer ModelRenderer { get; set; }

	/// <summary>
	/// The default holdtype for this weapon.
	/// </summary>
	[Property] protected AnimationHelper.HoldTypes HoldType { get; set; } = AnimationHelper.HoldTypes.Rifle;

	private ViewModel viewModel;

	/// <summary>
	/// A reference to the weapon's <see cref="Gunfight.ViewModel"/> if it has one.
	/// </summary>
	[Property] public ViewModel ViewModel
	{
		get => viewModel;
		set
		{
			viewModel = value;
			// Let the ViewModel know about our weapon
			viewModel.Weapon = this;
			// Risky
			viewModel.ViewModelCamera = PlayerController.ViewModelCamera;
		}
	}

	/// <summary>
	/// Get the weapon's owner - namely the player controller
	/// </summary>
	public PlayerController PlayerController => Components.Get<PlayerController>( FindMode.EverythingInAncestors );

	/// <summary>
	/// How long it's been since we used this attack.
	/// </summary>
	protected TimeSince TimeSincePrimaryAttack { get; set; }

	/// <summary>
	/// How long it's been since we used this attack.
	/// </summary>
	protected TimeSince TimeSinceSecondaryAttack { get; set; }

	/// <summary>
	/// Allow weapons to override holdtypes at any notice.
	/// </summary>
	/// <returns></returns>
	public virtual AnimationHelper.HoldTypes GetHoldType()
	{
		return HoldType;
	}

	/// <summary>
	/// Called when trying to shoot the weapon with the "Attack1" input action.
	/// </summary>
	/// <returns></returns>
	public virtual bool PrimaryAttack()
	{
		return true;
	}

	/// <summary>
	/// Can we even use this attack?
	/// </summary>
	/// <returns></returns>
	public virtual bool CanPrimaryAttack()
	{
		return TimeSincePrimaryAttack > 0.1f;
	}

	/// <summary>
	/// Called when trying to shoot the weapon with the "Attack2" input action.
	/// </summary>
	/// <returns></returns>
	public virtual bool SecondaryAttack()
	{
		return false;
	}

	/// <summary>
	/// Can we even use this attack?
	/// </summary>
	/// <returns></returns>
	public virtual bool CanSecondaryAttack()
	{
		return false;
	}

	protected override void OnUpdate()
	{
	}
}
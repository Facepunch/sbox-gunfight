
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
	/// A list of stats for this weapon. The <see cref="Resource"/> will set this when instantiated.
	/// </summary>
	public WeaponStats Stats
	{
		get
		{
			var stats = Resource.StatsResource.Stats;
			var functions = Components.GetAll<WeaponFunction>( FindMode.EverythingInSelfAndDescendants );

			// First up, go and calculate all the stats.
			foreach ( var fn in functions.Where( x => x.StatsResource is not null ) )
			{
				stats = stats += fn.StatsResource.Stats;
			}

			return stats;
		}
	}

	/// <summary>
	/// Calls an update to all the weapon stats.
	/// </summary>
	/// <returns></returns>
	public void UpdateStats()
	{
		var functions = Components.GetAll<WeaponFunction>( FindMode.EverythingInSelfAndDescendants );

		// Then go back and tell every stat to update their stats.
		foreach ( var fn in functions )
		{
			fn.UpdateStats();
		}
	}

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
	/// Access a function.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <returns></returns>
	public T GetFunction<T>() where T : WeaponFunction
	{
		return Components.Get<T>( FindMode.EnabledInSelfAndDescendants );
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
	
	/// <summary>
	/// Creates a viewmodel for the player to use.
	/// </summary>
	/// <param name="player"></param>
	private void CreateViewModel( PlayerController player )
	{
		var res = Resource;

		foreach ( var child in player.ViewModelGameObject.Children )
		{
			child.DestroyImmediate();
		}

		if ( res.ViewModelPrefab != null )
		{
			// Create the weapon prefab and put it on the weapon gameobject.
			var viewModelGameObject = res.ViewModelPrefab.Clone( new CloneConfig()
			{
				Transform = new Transform(),
				Parent = player.ViewModelGameObject,
				StartEnabled = true,
			} );

			var viewModelComponent = viewModelGameObject.Components.Get<ViewModel>();

			// Weapon needs to know about the ViewModel
			ViewModel = viewModelComponent;
		}
	}

	/// <summary>
	/// Called when we're equipping a weapon from PlayerLoadout
	/// </summary>
	/// <param name="player"></param>
	internal void OnEquip( PlayerController player )
	{
		if ( IsProxy )
		{
			return;
		}

		CreateViewModel( player );
	}
}

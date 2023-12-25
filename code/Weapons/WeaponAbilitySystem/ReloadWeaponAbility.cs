namespace Gunfight;

public partial class ReloadWeaponAbility : InputActionWeaponAbility
{
	[Property] public float ReloadTime { get; set; } = 1.0f;
	[Property] public AmmoContainer AmmoContainer { get; set; }

	bool IsReloading;
	TimeUntil TimeUntilReload;

	protected override void OnWeaponAbility()
	{
		if ( CanReload() )
		{
			StartReload();
		}
	}

	protected override void OnUpdate()
	{
		if ( IsProxy ) return;

		if ( IsReloading && TimeUntilReload )
		{
			EndReload();
		}
	}

	bool CanReload()
	{
		return !IsReloading && AmmoContainer.IsValid();
	}

	void StartReload()
	{
		IsReloading = true;
		TimeUntilReload = ReloadTime;

		// Tags will be better so we can just react to stimuli.
		Weapon.ViewModel?.ModelRenderer.Set( "b_reload", true );
	}

	void EndReload()
	{
		IsReloading = false;

		// Refill the ammo container.
		AmmoContainer.Ammo = AmmoContainer.MaxAmmo;

		// Tags will be better so we can just react to stimuli.
		Weapon.ViewModel?.ModelRenderer.Set( "b_reload", false );
	}

	protected override void OnStart()
	{
		// Try to fetch relevant stats from the weapon 
		ReloadTime = Stats.Get( WeaponStat.ReloadSpeed, ReloadTime );
	}
}

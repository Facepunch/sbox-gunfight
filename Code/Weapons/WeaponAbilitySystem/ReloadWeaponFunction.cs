namespace Gunfight;

public partial class ReloadWeaponFunction : InputActionWeaponFunction
{
	[Property] public float ReloadTime { get; set; } = 1.0f;
	[Property] public AmmoContainer AmmoContainer { get; set; }

	bool IsReloading;
	TimeUntil TimeUntilReload;

	protected override void OnEnabled()
	{
		BindTag( "reloading", () => IsReloading );
	}

	protected override void OnFunctionExecute()
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

	internal override void UpdateStats()
	{
		// Try to fetch relevant stats from the weapon 
		ReloadTime = Stats.ReloadSpeed;
	}
}

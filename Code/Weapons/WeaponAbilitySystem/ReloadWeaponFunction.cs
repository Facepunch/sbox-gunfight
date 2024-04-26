namespace Gunfight;

public partial class ReloadWeaponFunction : InputActionWeaponFunction
{
	/// <summary>
	/// How long does it take to reload?
	/// </summary>
	[Property] public float ReloadTime { get; set; } = 1.0f;

	/// <summary>
	/// How long does it take to reload while empty?
	/// </summary>
	[Property] public float EmptyReloadTime { get; set; } = 2.0f;

	/// <summary>
	/// This is really just the magazine for the weapon. 
	/// </summary>
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

	float GetReloadTime()
	{
		if ( !AmmoContainer.HasAmmo ) return EmptyReloadTime;
		return ReloadTime;
	}

	void StartReload()
	{
		IsReloading = true;
		TimeUntilReload = GetReloadTime();

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
}

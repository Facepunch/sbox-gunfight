using System.ComponentModel;

namespace Gunfight;

[Title( "Shoot Weapon Ability" ), Icon( "track_changes" )]
public partial class ShootWeaponAbility : InputActionWeaponAbility
{
	public override string Name => "Shoot";

	[Property, Category( "Bullet" )] public float BaseDamage { get; set; } = 25.0f;
	[Property, Category( "Bullet" )] public float WeaponShootDelay { get; set; } = 0.2f;
	[Property, Category( "Bullet" )] public float DryFireDelay { get; set; } = 1f;
	[Property, Category( "Bullet" )] public float MaxRange { get; set; } = 1024000;
	[Property, Category( "Bullet" )] public Curve BaseDamageFalloff { get; set; } = new( new List<Curve.Frame>() { new( 0, 1 ), new( 1, 0 ) } );
	[Property, Category( "Bullet" )] public float BulletSize { get; set; } = 1.0f;

	[Property, Category( "Effects" )] public GameObject MuzzleFlash { get; set; }
	[Property, Category( "Effects" )] public GameObject BulletTrail { get; set; }
	[Property, Category( "Effects" )] public SoundEvent ShootSound { get; set; }
	[Property, Category( "Effects" )] public SoundEvent DryFireSound { get; set; }


	// Functionality
	[Property, ReadOnly( true ), Category( "Data" )] public TimeSince TimeSinceShoot { get; set; }

	[Property, Category( "Ammo" )] public AmmoContainer AmmoContainer { get; set; }
	[Property, Category( "Ammo" )] public bool RequiresAmmoContainer { get; set; } = false;

	/// <summary>
	/// Fetches the desired model renderer that we'll focus effects on like trail effects, muzzle flashes, etc.
	/// </summary>
	protected SkinnedModelRenderer EffectsRenderer 
	{
		get
		{
			if ( IsProxy || !Weapon.ViewModel.IsValid() )
			{
				return Weapon.ModelRenderer;
			}

			return Weapon.ViewModel.ModelRenderer;
		}
	}

	/// <summary>
	/// Do shoot effects
	/// </summary>
	protected void DoShootEffects()
	{
		if ( !EffectsRenderer.IsValid() )
		{
			return;
		}

		// Create a muzzle flash from a GameObject / prefab
		if ( MuzzleFlash.IsValid() )
		{
			SceneUtility.Instantiate( MuzzleFlash, EffectsRenderer.GetAttachment( "muzzle" ) ?? Weapon.Transform.World );
		}

		if ( ShootSound is not null )
		{
			var snd = Sound.Play( ShootSound, Weapon.Transform.Position );
			snd.ListenLocal = !IsProxy;

			Log.Trace( $"ShootWeaponAbility: ShootSound {ShootSound.ResourceName}" );
		}

		// Third person
		Weapon.PlayerController.BodyRenderer.Set( "b_attack", true );

		// First person
		Weapon.ViewModel?.ModelRenderer.Set( "b_attack", true );
	}

	/// <summary>
	/// Shoot the gun!
	/// </summary>
	public void Shoot()
	{
		TimeSinceShoot = 0;

		if ( AmmoContainer is not null )
		{
			AmmoContainer.Ammo--;
		}

		var tr = GetShootTrace();

		if ( !tr.Hit )
		{
			DoShootEffects();
			return;
		}

		DoShootEffects();

		// Inflict damage on whatever we find.
		var damageInfo = DamageInfo.Bullet( BaseDamage, Weapon.PlayerController.GameObject, Weapon.GameObject );
		tr.GameObject.TakeDamage( ref damageInfo );

		Log.Trace( $"ShootWeaponAbility: We hit {tr.GameObject}!" );
	}

	protected void DryShoot()
	{
		TimeSinceShoot = 0;

		DryShootEffects();
	}

	protected void DryShootEffects()
	{
		if ( DryFireSound is not null )
		{
			var snd = Sound.Play( DryFireSound, Weapon.Transform.Position );
			snd.ListenLocal = !IsProxy;

			Log.Trace( $"ShootWeaponAbility: ShootSound {DryFireSound.ResourceName}" );
		}

		// First person
		Weapon.ViewModel?.ModelRenderer.Set( "b_attack_dry", true );
	}

	protected virtual Ray WeaponRay => Weapon.PlayerController.AimRay;

	protected override void OnUpdate()
	{
		//var tr = GetShootTrace();

		//Gizmo.Draw.Line( tr.StartPosition, tr.EndPosition );
		//Gizmo.Draw.LineSphere( tr.StartPosition, 16 );
		//Gizmo.Draw.LineSphere( tr.EndPosition, 16 );
	}

	/// <summary>
	/// Runs a trace with all the data we have supplied it, and returns the result
	/// </summary>
	/// <returns></returns>
	protected virtual SceneTraceResult GetShootTrace()
	{
		var tr = Scene.Trace.Ray( WeaponRay, MaxRange )
			.Size( BulletSize )
			.WithAnyTags( "solid" )
			.UseHitboxes()
			.Run();

		return tr;
	}

	/// <summary>
	/// Can we shoot this gun right now?
	/// </summary>
	/// <returns></returns>
	public bool CanShoot()
	{
		// Delay checks
		if ( TimeSinceShoot < WeaponShootDelay )
		{
			return false;
		}

		// Ammo checks
		if ( RequiresAmmoContainer && ( AmmoContainer == null || !AmmoContainer.HasAmmo ) )
		{
			return false;
		}

		return true;
	}

	protected override void OnWeaponAbility()
	{
		if ( CanShoot() )
		{
			Shoot();
		}
		else
		{
			if ( TimeSinceShoot < DryFireDelay )
			{
				return;
			}

			DryShoot();
		}
	}
}
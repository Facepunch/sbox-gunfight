using Sandbox.UI;

namespace Facepunch.Gunfight;

public enum FireMode
{
	Semi,
	FullAuto,
	Burst
}

public partial class GunfightWeapon : BaseWeapon
{
	[Net, Predicted] public int AmmoClip { get; set; }
	[Net, Predicted] public TimeSince TimeSinceReload { get; set; }
	[Net, Predicted] public bool IsReloading { get; set; }
	[Net, Predicted] public TimeSince TimeSinceDeployed { get; set; }
	[Net, Predicted] public TimeSince TimeSincePrimaryAttack { get; set; }
	[Net, Predicted] protected int BurstCount { get; set; } = 0;
	[Net, Predicted] public FireMode CurrentFireMode { get; set; } = FireMode.Semi;
	[Net, Predicted] public TimeSince TimeSinceFireModeSwitch { get; set; }

	[Net, Predicted] public Vector2 Recoil { get; set; }

	public PickupTrigger PickupTrigger { get; protected set; }
	public CrosshairRender Crosshair { get; protected set; }

	protected GunfightPlayer Player => Owner as GunfightPlayer;
	protected PlayerController PlayerController => Player?.Controller as PlayerController;

	public float LowAmmoFraction => 0.2f;
	public bool IsLowAmmo() => (AmmoClip / (float)ClipSize) <= LowAmmoFraction;
	public bool IsUnlimitedAmmo() => AmmoClip == 0 && ClipSize == 0;

	public new string Name => WeaponDefinition?.WeaponName ?? base.Name;
	public string ShortName => WeaponDefinition.WeaponShortName;
	public bool IsSprinting => PlayerController?.IsSprinting ?? false;
	public bool IsAiming => PlayerController?.IsAiming ?? false;
	public float PrimaryFireRate => WeaponDefinition.BaseFireRate;
	public bool IsBurst => CurrentFireMode == FireMode.Burst;
	public int ClipSize => WeaponDefinition.ClipSize;
	public float ReloadTime => WeaponDefinition.ReloadTime;
	public AmmoType AmmoType => WeaponDefinition.AmmoType;
	public Vector2 RecoilDecay => WeaponDefinition.Recoil.Decay;
	public Vector2 BaseRecoilMinimum => WeaponDefinition.Recoil.BaseRecoilMinimum;
	public Vector2 BaseRecoilMaximum => WeaponDefinition.Recoil.BaseRecoilMaximum;
	public float BulletSpread => WeaponDefinition.BulletSpread;
	public float BulletForce => WeaponDefinition.BulletForce;
	public float BulletDamage => WeaponDefinition.BulletDamage;
	public float BulletSize => WeaponDefinition.BulletSize;
	public int BulletCount => WeaponDefinition.BulletCount;
	public float BulletRange => WeaponDefinition.BulletRange;
	public string GunIcon => WeaponDefinition.Icon;
	public float PostSprintAttackDelay => 0.15f;

	public void CycleFireMode()
	{
		if ( TimeSinceFireModeSwitch < 0.3f ) return;

		var curIndex = (int)CurrentFireMode;
		var length = Enum.GetNames( typeof( FireMode ) ).Length;
		curIndex++;

		var newIndex = (curIndex + length) % length;
		CurrentFireMode = (FireMode)newIndex;

		// TODO - Sound, animations (?)

		TimeSinceFireModeSwitch = 0;

		if ( Host.IsServer )
		{
			ChatBox.AddInformation( To.Single( Owner ), $"Fire Mode: {CurrentFireMode}" );
		}
	}

	public int AvailableAmmo()
	{
		var owner = Owner as GunfightPlayer;
		if ( owner == null ) return 0;
		return owner.AmmoCount( AmmoType );
	}

	public override void ActiveStart( Entity ent )
	{
		base.ActiveStart( ent );

		TimeSinceDeployed = 0;
		TimeSinceReload = ReloadTime;

		IsReloading = false;
	}

	public override void Spawn()
	{
		base.Spawn();

		PickupTrigger = new PickupTrigger();
		PickupTrigger.Parent = this;
		PickupTrigger.Position = Position;
	}

	public virtual void Reload()
	{
		if ( IsUnlimitedAmmo() ) return;
		if ( IsReloading ) return;

		if ( AmmoClip >= ClipSize ) return;

		TimeSinceReload = 0;

		if ( Owner is GunfightPlayer player )
		{
			if ( player.AmmoCount( AmmoType ) <= 0 )
				return;
		}

		IsReloading = true;

		(Owner as AnimatedEntity).SetAnimParameter( "b_reload", true );

		StartReloadEffects();
	}

	public virtual bool WantsReload()
	{
		return Input.Down( InputButton.Reload );
	
	}

	protected virtual void SimulateRecoil( Client owner )
	{
		Recoil -= RecoilDecay * Time.Delta;
		// Clamp down to zero
		Recoil = Recoil.Clamp( 0, float.MaxValue );
	}

	protected virtual void ApplyRecoil()
	{
		Rand.SetSeed( Time.Tick );
		var randX = Rand.Float( BaseRecoilMinimum.x, BaseRecoilMaximum.x );
		var randY = Rand.Float( BaseRecoilMinimum.y, BaseRecoilMaximum.y );

		// TODO - Remove magic numbers.
		var recoilScale = 1f;

		// Recoil gets decreased when aiming down the sights.
		if ( IsAiming )
		{
			recoilScale *= 0.8f;
		}

		// Recoil gets decreased when ducking.
		if ( PlayerController.Duck.IsActive )
		{
			recoilScale *= 0.8f;
		}
	
		// If you're moving at speed, apply more recoil.
		var speed = Player.Velocity.Length.LerpInverse( 0, 400, true );
		recoilScale += 0.5f * speed;

		Recoil += new Vector2( randX, randY ) * recoilScale;
	}

	public override void BuildInput( InputBuilder input )
	{
		input.ViewAngles.pitch -= Recoil.y * Time.Delta;
		input.ViewAngles.yaw -= Recoil.x * Time.Delta;
	}

	public override void Simulate( Client owner )
	{
		if ( TimeSinceDeployed < 0.6f )
			return;

		if ( Input.Pressed( InputButton.Menu ) )
		{
			CycleFireMode();
		}

		SimulateRecoil( owner );

		if ( WantsReload() )
		{
			Reload();
			return;
		}

		if ( Input.Pressed( InputButton.PrimaryAttack ) && IsReloading )
		{
			IsReloading = false;
			TimeSinceReload = 0;
		}

		//
		// Reload could have changed our owner
		//
		if ( !Owner.IsValid() )
			return;
		
		if ( CanPrimaryAttack() )
		{
			using ( LagCompensation() )
			{
				TimeSincePrimaryAttack = 0;
				AttackPrimary();
			}
		}

		if ( IsReloading && TimeSinceReload > ReloadTime )
		{
			OnReloadFinish();
		}
	}

	protected override void InitializeWeapon( WeaponDefinition def )
	{
		base.InitializeWeapon( def );

		AmmoClip = def.StandardClip;
		CurrentFireMode = def.DefaultFireMode;
		Crosshair = CrosshairRender.From( def.Crosshair );
	}

	public virtual void OnReloadFinish()
	{
		IsReloading = false;

		if ( Owner is GunfightPlayer player )
		{
			var single = WeaponDefinition.ReloadSingle;
			int amountToTake = single ? 1 : ClipSize - AmmoClip;
			var ammo = player.TakeAmmo( AmmoType, amountToTake );
			if ( ammo == 0 )
				return;

			AmmoClip += ammo;

			if ( AmmoClip == ClipSize )
			{
				ViewModelEntity?.SetAnimParameter( "reload_finished", true );
			}
			else
			{
				if ( single )
					Reload();
			}
		}
	}

	[ClientRpc]
	public virtual void StartReloadEffects()
	{
		ViewModelEntity?.SetAnimParameter( "reload", true );

		// TODO - player third person model reload
	}

	protected bool CanDefaultPrimaryAttack()
	{
		if ( IsSprinting ) return false;
		if ( IsReloading ) return false;
		if ( PlayerController.SinceStoppedSprinting <= PostSprintAttackDelay ) return false;

		return TimeSincePrimaryAttack >= PrimaryFireRate;
	}

	protected bool CanPrimaryAttackSemi()
	{
		return Input.Pressed( InputButton.PrimaryAttack );
	}

	protected bool CanPrimaryAttackBurst()
	{
		if ( !Input.Down( InputButton.PrimaryAttack ) )
		{
			BurstCount = 0;
			return false;
		}

		if ( BurstCount >= WeaponDefinition.BurstAmount )
			return false;

		return true;
	}
	public virtual bool CanPrimaryAttack()
	{
		if ( !CanDefaultPrimaryAttack() ) return false;

		var fireMode = CurrentFireMode;
		if ( fireMode == FireMode.Semi )
		{
			return CanPrimaryAttackSemi();
		}
		else if ( fireMode == FireMode.Burst )
		{
			return CanPrimaryAttackBurst();
		}

		return TimeSincePrimaryAttack >= PrimaryFireRate && Input.Down( InputButton.PrimaryAttack );
	}

	public virtual void AttackPrimary()
	{
		TimeSincePrimaryAttack = 0;

		if ( !IsUnlimitedAmmo() && !TakeAmmo( 1 ) )
		{
			if ( Input.Pressed( InputButton.PrimaryAttack ) )
			{
				DryFire();
			}
			return;
		}

		(Owner as AnimatedEntity)?.SetAnimParameter( "b_attack", true );

		//
		// Tell the clients to play the shoot effects
		//
		ShootEffects();
		Owner?.PlaySound( WeaponDefinition.FireSound );

		//
		// Shoot the bullets
		//
		ShootBullet( BulletSpread, BulletForce, BulletDamage, BulletSize, BulletCount );

		ApplyRecoil();

		if ( IsBurst )
			BurstCount++;
	}

	private void AmmoLowSound( float ammoPercent )
	{
		var snd = Sound.FromEntity( WeaponDefinition.DryFireSound, Owner );

		var percentZero = ammoPercent.Remap( 0, 0.2f, 0, 1 );
		snd.SetPitch( 1f - ( 0.05f * percentZero ) );
	}

	[ClientRpc]
	protected virtual void ShootEffects()
	{
		Host.AssertClient();

		ViewModelEntity?.SetAnimParameter( "fire", true );
		CrosshairLastShoot = 0;

		if ( IsLowAmmo() )
			AmmoLowSound( AmmoClip / (float)ClipSize );
	}

	/// <summary>
	/// Does a trace from start to end, does bullet impact effects. Coded as an IEnumerable so you can return multiple
	/// hits, like if you're going through layers or ricocet'ing or something.
	/// </summary>
	public virtual IEnumerable<TraceResult> TraceBullet( Vector3 start, Vector3 end, float radius = 2.0f )
	{
		bool underWater = Trace.TestPoint( start, "water" );

		var trace = Trace.Ray( start, end )
				.UseHitboxes()
				.WithAnyTags( "solid", "player", "npc" )
				.Ignore( this )
				.Size( radius );

		//
		// If we're not underwater then we can hit water
		//
		if ( !underWater )
			trace = trace.WithAnyTags( "water" );

		var tr = trace.Run();

		if ( tr.Hit )
			yield return tr;

		//
		// Another trace, bullet going through thin material, penetrating water surface?
		//
	}

	/// <summary>
	/// Shoot a single bullet
	/// </summary>
	public virtual void ShootBullet( float spread, float force, float damage, float bulletSize, int bulletCount = 1 )
	{
		//
		// Seed rand using the tick, so bullet cones match on client and server
		//
		Rand.SetSeed( Time.Tick );

		for ( int i = 0; i < bulletCount; i++ )
		{
			var forward = Owner.EyeRotation.Forward;
			forward += (Vector3.Random + Vector3.Random + Vector3.Random + Vector3.Random) * spread * 0.25f;
			forward = forward.Normal;

			//
			// ShootBullet is coded in a way where we can have bullets pass through shit
			// or bounce off shit, in which case it'll return multiple results
			//
			foreach ( var tr in TraceBullet( Owner.EyePosition, Owner.EyePosition + forward * BulletRange, bulletSize ) )
			{
				tr.Surface.DoBulletImpact( tr );

				if ( tr.Distance > 200 )
				{
					CreateTracerEffect( tr.EndPosition, tr.Distance );
				}

				if ( !IsServer ) continue;
				if ( !tr.Entity.IsValid() ) continue;

				var damageInfo = DamageInfo.FromBullet( tr.EndPosition, forward * 100 * force, damage )
					.UsingTraceResult( tr )
					.WithAttacker( Owner )
					.WithWeapon( this );

				tr.Entity.TakeDamage( damageInfo );
			}
		}
	}
	
	[ClientRpc]
	public void CreateTracerEffect( Vector3 hitPosition, float dist )
	{
		// get the muzzle position on our effect entity - either viewmodel or world model
		var pos = EffectEntity.GetAttachment( "muzzle" ) ?? Transform;

		var system = Particles.Create( WeaponDefinition.ShootTrailParticleEffect ?? "particles/gameplay/guns/trail/trail_smoke.vpcf" );
		system?.SetPosition( 0, pos.Position );
		system?.SetPosition( 1, hitPosition );
		system?.SetPosition( 2, dist );
	}

	public bool TakeAmmo( int amount )
	{
		if ( AmmoClip < amount )
			return false;

		AmmoClip -= amount;
		return true;
	}

	public virtual void DryFire()
	{
		Owner?.PlaySound( WeaponDefinition.DryFireSound );
	}

	public bool IsUsable()
	{
		if ( AmmoClip > 0 ) return true;
		if ( AmmoType == AmmoType.None ) return true;
		return AvailableAmmo() > 0;
	}

	public override void OnCarryStart( Entity carrier )
	{
		base.OnCarryStart( carrier );

		if ( PickupTrigger.IsValid() )
		{
			PickupTrigger.EnableTouch = false;
		}
	}

	public override void OnCarryDrop( Entity dropper )
	{
		base.OnCarryDrop( dropper );

		if ( PickupTrigger.IsValid() )
		{
			PickupTrigger.EnableTouch = true;
		}
	}

	protected TimeSince CrosshairLastShoot { get; set; }

	public virtual void RenderHud( in Vector2 screensize )
	{
		var center = screensize * 0.5f;

		RenderCrosshair( center, CrosshairLastShoot.Relative, TimeSinceReload / ReloadTime, Owner?.Velocity.Length ?? 0, IsAiming );
	}

	public virtual void RenderCrosshair( in Vector2 center, float lastAttack, float lastReload, float speed, bool ads = false )
	{
		Crosshair?.RenderCrosshair( in center, lastAttack, lastReload, speed, ads );
	}

	public override string ToString()
	{
		return $"GunfightWeapon[{WeaponDefinition?.WeaponName ?? "base"}]";
	}
}

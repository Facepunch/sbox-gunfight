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
		Recoil = 0;

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

		if ( IsReloading && Input.Pressed( InputButton.PrimaryAttack ) )
		{
			IsReloading = false;
			TimeSinceReload = ReloadTime;
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

	public virtual IEnumerable<TraceResult> TraceBullet( Vector3 start, Vector3 end, float radius, ref float damage )
	{
		float curHits = 0;
		var hits = new List<TraceResult>();

		while ( curHits < MaxAmtOfHits )
		{
			curHits++;

			var tr = Trace.Ray( start, end )
			.UseHitboxes()
			.WithAnyTags( "solid", "player", "glass" )
			.Ignore( this )
			.Size( radius )
			.Run();

			if ( tr.Hit )
				hits.Add( tr );

			var reflectDir = CalculateRicochetDirection( tr, ref curHits );
			var angle = reflectDir.Angle( tr.Direction );

			start = tr.EndPosition;
			end = tr.EndPosition + ( reflectDir * 5000 );

			if ( !ShouldBulletContinue( tr, angle, ref damage ) )
				break;
		}

		return hits;
	}

	/// <summary>
	/// How many ricochet hits until we stop traversing
	/// </summary>
	protected virtual float MaxAmtOfHits => 2f;
	
	/// <summary>
	/// Maximum angle in degrees for ricochet to be possible
	/// </summary>
	protected virtual float MaxRicochetAngle => 45f;

	protected virtual bool ShouldBulletContinue( TraceResult tr, float angle, ref float damage )
	{
		float maxAngle = MaxRicochetAngle;

		if ( angle > maxAngle )
			return false;

		// Half the damage when ricocheting
		// TODO - Data setup
		damage *= 0.5f;

		return true;
	}

	protected virtual Vector3 CalculateRicochetDirection( TraceResult tr, ref float hits )
	{
		if ( tr.Entity is GlassShard )
		{
			// Allow us to do another hit
			hits--;
			return tr.Direction;
		}

		return Vector3.Reflect( tr.Direction, tr.Normal ).Normal;
	}

	public virtual void ShootBullet( float spread, float force, float damage, float bulletSize, int bulletCount = 1, float bulletRange = 5000f )
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

			foreach ( var tr in TraceBullet( Owner.EyePosition, Owner.EyePosition + forward * bulletRange, bulletSize, ref damage ) )
			{
				tr.Surface.DoBulletImpact( tr );

				if ( !IsServer ) continue;
				if ( !tr.Entity.IsValid() ) continue;

				var damageInfo = DamageInfo.FromBullet( tr.EndPosition, forward * 100 * force, damage )
					.UsingTraceResult( tr )
					.WithAttacker( Owner )
					.WithWeapon( this );

				tr.Entity.TakeDamage( damageInfo );

				DoTracer( tr.StartPosition, tr.EndPosition, tr.Distance );
			}
		}
	}

	[ClientRpc]
	public void DoTracer( Vector3 from, Vector3 to, float dist )
	{
		var system = Particles.Create( WeaponDefinition.ShootTrailParticleEffect ?? "particles/gameplay/guns/trail/trail_smoke.vpcf" );
		system?.SetPosition( 0, from );
		system?.SetPosition( 1, to );
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

		RenderCrosshair( center, CrosshairLastShoot.Relative, IsReloading ? TimeSinceReload / ReloadTime : 0, Owner?.Velocity.Length ?? 0, IsAiming );
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

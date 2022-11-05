using Sandbox.UI;

namespace Facepunch.Gunfight;

public enum FireMode
{
	Semi,
	FullAuto,
	Burst
}

public partial class GunfightWeapon : BaseWeapon, IUse
{
	[Net, Predicted] public int AmmoClip { get; set; }
	[Net, Predicted] public TimeSince TimeSinceReload { get; set; }
	[Net, Predicted] public bool IsReloading { get; set; }
	[Net, Predicted] public TimeSince TimeSinceDeployed { get; set; }
	[Net, Predicted] public TimeSince TimeSincePrimaryAttack { get; set; }
	[Net, Predicted] protected int BurstCount { get; set; } = 0;
	[Net, Change( "FireModeChanged" )] public FireMode CurrentFireMode { get; set; } = FireMode.Semi;
	[Net, Predicted] public TimeSince TimeSinceFireModeSwitch { get; set; }
	[Net, Predicted] public TimeSince TimeSinceBurstFinished { get; set; }
	[Net, Predicted] public bool IsBurstFiring { get; set; }

	[Net, Predicted] public Vector2 CameraRecoil { get; set; }
	[Net, Predicted] public Vector2 WeaponSpreadRecoil { get; set; }

	public CrosshairRender Crosshair { get; protected set; }
	public PickupTrigger PickupTrigger { get; protected set; }

	protected GunfightPlayer Player => Owner as GunfightPlayer;
	protected PlayerController PlayerController => Player?.Controller as PlayerController;

	public bool IsDecaying { get; set; } = false;
	public TimeUntil TimeUntilDecay { get; set; } = 0;

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
	public AmmoType AmmoType => WeaponDefinition?.AmmoType ?? AmmoType.None;
	public Vector2 RecoilDecay => WeaponDefinition.Recoil.Decay;
	public float BulletSpread => WeaponDefinition.BulletSpread;
	public float BulletForce => WeaponDefinition.BulletForce;
	public float BulletDamage => WeaponDefinition.BulletDamage;
	public float BulletSize => WeaponDefinition.BulletSize;
	public int BulletCount => WeaponDefinition.BulletCount;
	public float BulletRange => WeaponDefinition.BulletRange;
	public string GunIcon => WeaponDefinition.Icon;
	public float PostSprintAttackDelay => 0.15f;

	// @event
	protected void FireModeChanged( FireMode before, FireMode after )
	{
		Event.Run( "gunfight.firemode.changed", after );
	}

	public void StartDecaying()
	{
		TimeUntilDecay = 30f;
		IsDecaying = true;
	}

	public void StopDecaying()
	{
		IsDecaying = false;
		TimeUntilDecay = 0f;
	}
	
	[Event.Tick.Server]
	protected void TickServer()
	{
		if ( !IsDecaying ) return;

		if ( TimeUntilDecay )
		{
			Delete();
		}
	}

	protected int GetIndex( FireMode fireMode )
	{
		int i = 0;
		foreach( var mode in WeaponDefinition.SupportedFireModes )
		{
			if ( mode == fireMode ) return i;
			i++;
		}

		return 0;
	}

	public void CycleFireMode()
	{
		if ( TimeSinceFireModeSwitch < 0.3f ) return;

		var curIndex = GetIndex( CurrentFireMode );
		var length = WeaponDefinition.SupportedFireModes.Count;
		var newIndex = (curIndex + 1 + length) % length;

		// We didn't change anything
		if ( newIndex == curIndex ) return;

		CurrentFireMode = WeaponDefinition.SupportedFireModes[newIndex];

		// TODO - Sound, animations (?)

		TimeSinceFireModeSwitch = 0;

		if ( Host.IsServer )
		{
			UI.NotificationManager.AddNotification( To.Single( Owner ), UI.NotificationDockType.BottomMiddle, $"Fire Mode: {CurrentFireMode}", 1 );
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
		CameraRecoil = 0;
		WeaponSpreadRecoil = 0;
		TimeSinceBurstFinished = WeaponDefinition.BurstCooldown;
		IsBurstFiring = false;
		BurstCount = 0;
		IsReloading = false;
		StopDecaying();
	}

	public override void Spawn()
	{
		base.Spawn();

		PickupTrigger = new PickupTrigger();
		PickupTrigger.Parent = this;
		PickupTrigger.Position = Position;
	}

	public override void OnCarryStart( Entity carrier )
	{
		base.OnCarryStart( carrier );

		if ( PickupTrigger.IsValid() )
		{
			PickupTrigger.PhysicsEnabled = false;
			PickupTrigger.EnableTouch = false;
		}
	}

	public override void OnCarryDrop( Entity carrier )
	{
		base.OnCarryDrop( carrier );

		if ( PickupTrigger.IsValid() )
		{
			PickupTrigger.PhysicsEnabled = true;
			PickupTrigger.EnableTouch = true;
		}

		StartDecaying();
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
		CameraRecoil -= RecoilDecay * Time.Delta;
		// Clamp down to zero
		CameraRecoil = CameraRecoil.Clamp( 0, float.MaxValue );

		WeaponSpreadRecoil -= WeaponDefinition.Recoil.SpreadDecay * Time.Delta;
		// Clamp down to zero
		WeaponSpreadRecoil = WeaponSpreadRecoil.Clamp( 0, float.MaxValue );
	}

	protected virtual void ApplyRecoil()
	{
		Rand.SetSeed( Time.Tick );

		var randX = Rand.Float( WeaponDefinition.Recoil.BaseRecoilMinimum.x, WeaponDefinition.Recoil.BaseRecoilMaximum.x );
		var randY = Rand.Float( WeaponDefinition.Recoil.BaseRecoilMinimum.y, WeaponDefinition.Recoil.BaseRecoilMaximum.y );
		var randSpreadX = Rand.Float( WeaponDefinition.Recoil.MinimumSpread.x, WeaponDefinition.Recoil.MaximumSpread.x );
		var randSpreadY = Rand.Float( WeaponDefinition.Recoil.MinimumSpread.y, WeaponDefinition.Recoil.MaximumSpread.y );

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
		recoilScale += 1f * speed;

		CameraRecoil += new Vector2( randX, randY ) * recoilScale;
		// Apply spread too
		WeaponSpreadRecoil += new Vector2( randSpreadX, randSpreadY ) * recoilScale;
	}

	public override void BuildInput( InputBuilder input )
	{
		input.ViewAngles.pitch -= CameraRecoil.y * Time.Delta;
		input.ViewAngles.yaw -= CameraRecoil.x * Time.Delta;
	}

	public override void FrameSimulate( Client cl )
	{
		FrameSimulateAttachments( cl );
	}

	public override void Simulate( Client cl )
	{
		SimulateAttachments( cl );

		if ( TimeSinceDeployed < 0.6f )
			return;

		if ( Input.Pressed( InputButton.View ) )
		{
			CycleFireMode();
		}

		if ( IsBurst && BurstCount >= WeaponDefinition.BurstAmount )
		{
			TimeSinceBurstFinished = 0;
			IsBurstFiring = false;
			BurstCount = 0;
		}

		SimulateRecoil( cl );

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


	[ClientRpc]
	protected virtual void RpcHolster()
	{
		//Log.Info( $"{Host.Name} Holster {ViewModelEntity}" );
		ViewModelEntity?.SetAnimParameter( "holster", true );
	}

	public void Holster()
	{
		if ( IsServer )
			RpcHolster( To.Single( Owner ) );
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
		if ( Input.Down( InputButton.PrimaryAttack ) && TimeSinceBurstFinished >= WeaponDefinition.BurstCooldown )
			IsBurstFiring = true;

		if ( !IsBurstFiring ) return false;
		if ( TimeSinceBurstFinished < WeaponDefinition.BurstCooldown ) return false;

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
		ShootBullet( BulletSpread, BulletForce, BulletSize, BulletCount, BulletRange );

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

	protected TraceResult DoTraceBullet( Vector3 start, Vector3 end, float radius )
	{
		return Trace.Ray( start, end )
			.UseHitboxes()
			.WithAnyTags( "solid", "player", "glass" )
			.Ignore( this )
			.Size( radius )
			.Run();
	}

	protected float PenetrationIncrementAmount => 15f;
	protected int PenetrationMaxSteps => 2;

	protected bool ShouldPenetrate()
	{
		if ( !WeaponDefinition.DamageFlags.HasFlag( DamageFlags.Bullet ) )
			return false;

		return true;
	}

	public virtual IEnumerable<TraceResult> TraceBullet( Vector3 start, Vector3 end, float radius, ref float damage )
	{
		float curHits = 0;
		var hits = new List<TraceResult>();

		while ( curHits < MaxAmtOfHits )
		{
			curHits++;

			var tr = DoTraceBullet( start, end, radius );
			if ( tr.Hit )
			{
				if ( curHits > 1 )
					damage *= 0.5f;
				hits.Add( tr );
			}

			var reflectDir = CalculateRicochetDirection( tr, ref curHits );
			var angle = reflectDir.Angle( tr.Direction );

			start = tr.EndPosition;
			end = tr.EndPosition + (reflectDir * BulletRange);
			
			var didPenetrate = false;
			if ( ShouldPenetrate() )
			{
				// Look for penetration
				var forwardStep = 0f;

				while ( forwardStep < PenetrationMaxSteps )
				{
					forwardStep++;

					var penStart = tr.EndPosition + tr.Direction * (forwardStep * PenetrationIncrementAmount);
					var penEnd = tr.EndPosition + tr.Direction * (forwardStep + 1 * PenetrationIncrementAmount);

					var penTrace = DoTraceBullet( penStart, penEnd, radius );
					if ( !penTrace.StartedSolid )
					{
						var newStart = penTrace.EndPosition;
						var newTrace = DoTraceBullet( newStart, newStart + tr.Direction * BulletRange, radius );
						hits.Add( newTrace );
						didPenetrate = true;
						break;
					}
				}
			}

			if ( didPenetrate || !ShouldBulletContinue( tr, angle, ref damage ) )
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

	public virtual void ShootBullet( float spread, float force, float bulletSize, int bulletCount = 1, float bulletRange = 5000f )
	{
		//
		// Seed rand using the tick, so bullet cones match on client and server
		//
		Rand.SetSeed( Time.Tick );

		for ( int i = 0; i < bulletCount; i++ )
		{
			var rot = Owner.EyeRotation;
			var weaponRecoil = WeaponSpreadRecoil;
			rot *= Rotation.From( new Angles( -weaponRecoil.y, -weaponRecoil.x, 0 ) );

			var forward = rot.Forward;
			forward += (Vector3.Random + Vector3.Random + Vector3.Random + Vector3.Random) * spread * 0.25f;
			forward = forward.Normal;

			var damage = BulletDamage;

			Vector3 LastImpact = Vector3.Zero;
			int count = 0;
			foreach ( var tr in TraceBullet( Owner.EyePosition, Owner.EyePosition + forward * bulletRange, bulletSize, ref damage ) )
			{
				tr.Surface.DoBulletImpact( tr );

				if ( !IsServer ) continue;
				if ( !tr.Entity.IsValid() ) continue;

				var damageInfo = DamageInfo.FromBullet( tr.EndPosition, forward * 100 * force, damage )
					.UsingTraceResult( tr )
					.WithAttacker( Owner )
					.WithFlag( WeaponDefinition.DamageFlags )
					.WithWeapon( this );

				tr.Entity.TakeDamage( damageInfo );

				if ( WeaponDefinition.DamageFlags.HasFlag( DamageFlags.Bullet ) )
					DoTracer( tr.StartPosition, tr.EndPosition, tr.Distance, count );

				if ( count == 1 )
				{
					Particles.Create( "particles/gameplay/guns/trail/rico_trail_impact_spark.vpcf", LastImpact );
				}

				LastImpact = tr.EndPosition;
				count++;
			}
		}
	}

	[ClientRpc]
	public void DoTracer( Vector3 from, Vector3 to, float dist, int bullet )
	{
		var path = WeaponDefinition.ShootTrailParticleEffect ?? "particles/gameplay/guns/trail/trail_smoke.vpcf";

		if ( bullet > 0 )
		{
			path = "particles/gameplay/guns/trail/rico_trail_smoke.vpcf";

			// Project backward
			Vector3 dir = (from - to).Normal;
			var tr = Trace.Ray( to, from + (dir * 50f) )
				.Radius( 1f )
				.Ignore( this )
				.Run();

			tr.Surface.DoBulletImpact( tr );
		}

		var system = Particles.Create( path );

		system?.SetPosition( 0, bullet == 0 ? EffectEntity.GetAttachment( "muzzle" )?.Position ?? from : from );
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


	public bool OnUse( Entity user )
	{
		var player = user as GunfightPlayer;
		if ( !player.IsValid() ) return false;

		if ( !player.Inventory.Add( this, true ) )
		{
			Log.Info( "can't add to inventory" );
		}

		return false;
	}

	public bool IsUsable( Entity user ) => IsUsable();

	protected TimeSince CrosshairLastShoot { get; set; }

	public Task DeleteTask { get; set; }

	public virtual void RenderHud( in Vector2 screensize )
	{
		var center = screensize * 0.5f;

		RenderCrosshair( center, CrosshairLastShoot.Relative, IsReloading ? TimeSinceReload / ReloadTime : 0, Owner?.Velocity.Length ?? 0, IsAiming );
	}

	public virtual void RenderCrosshair( Vector2 center, float lastAttack, float lastReload, float speed, bool ads = false )
	{
		Crosshair?.RenderCrosshair( center, lastAttack, lastReload, speed, ads );
	}

	public override string ToString()
	{
		return $"GunfightWeapon[{WeaponDefinition?.WeaponName ?? "base"}]";
	}
}

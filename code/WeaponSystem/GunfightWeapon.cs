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
	public TimeSince TimeSinceDeployed { get; set; }
	[Net, Predicted] public TimeSince TimeSincePrimaryAttack { get; set; }
	[Net, Predicted] protected int BurstCount { get; set; } = 0;
	[Net, Change( "FireModeChanged" )] public FireMode CurrentFireMode { get; set; } = FireMode.Semi;
	[Net, Predicted] public TimeSince TimeSinceFireModeSwitch { get; set; }
	[Net, Predicted] public TimeSince TimeSinceBurstFinished { get; set; }
	[Net, Predicted] public bool IsBurstFiring { get; set; }

	[Net, Predicted] public Vector2 CameraRecoil { get; set; }
	[Net, Predicted] public Vector2 WeaponSpreadRecoil { get; set; }

	public PickupTrigger PickupTrigger { get; protected set; }

	protected GunfightPlayer Player => Owner as GunfightPlayer;
	protected PlayerController PlayerController => Player?.Controller as PlayerController;

	public bool IsDecaying { get; set; } = false;
	public TimeUntil TimeUntilDecay { get; set; } = 0;

	public float LowAmmoFraction => 0.2f;
	public bool IsLowAmmo() => (AmmoClip / (float)ClipSize) <= LowAmmoFraction;
	public bool IsUnlimitedAmmo() => AmmoClip == 0 && ClipSize == 0;

	public float HolsterTime => WeaponDefinition?.HolsterTime ?? 0.5f;
	public new string Name => WeaponDefinition?.WeaponName ?? base.Name;
	public string ShortName => WeaponDefinition.WeaponShortName;
	public bool IsSprinting => PlayerController?.IsSprinting ?? false;
	public bool IsAiming => PlayerController?.IsAiming ?? false;
	public float PrimaryFireRate => WeaponDefinition.BaseFireRate;
	public bool IsBurst => CurrentFireMode == FireMode.Burst;
	public int ClipSize => WeaponDefinition.ClipSize;
	public float ReloadTime => WeaponDefinition.ReloadTime;
	public AmmoType AmmoType => WeaponDefinition?.AmmoType ?? AmmoType.None;
	public float BulletSpread => WeaponDefinition.BulletSpread;
	public float BulletForce => WeaponDefinition.BulletForce;
	public float BulletDamage => WeaponDefinition.BulletDamage;
	public float BulletSize => WeaponDefinition.BulletSize;
	public int BulletCount => WeaponDefinition.BulletCount;
	public float BulletRange => WeaponDefinition.BulletRange;
	public string GunIcon => WeaponDefinition.Icon;
	public float PostSprintAttackDelay => 0.15f;
	public float BaseAimTime => WeaponDefinition.BaseAimTime;
	public float DeployTime => WeaponDefinition.DeployTime;

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
	
	[GameEvent.Tick.Server]
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

		if ( Game.IsServer )
		{
			UI.NotificationManager.AddNotification( To.Single( Owner ), UI.NotificationDockType.BottomMiddle, $"Fire Mode: {CurrentFireMode}", 1 );
		}

		RpcCycleFireModeEffect();
	}

	[ClientRpc]
	void RpcCycleFireModeEffect()
	{
		int firingModeParameter = CurrentFireMode switch
		{
			FireMode.Semi => 3,
			FireMode.FullAuto => 2,
			FireMode.Burst => 1,
			_ => 0
		};

		ViewModelEntity?.SetAnimParameter( "firing_mode", firingModeParameter );
	}

	[ClientRpc]
	protected virtual void RpcHolster()
	{
		//Log.Info( $"{Host.Name} Holster {ViewModelEntity}" );
		ViewModelEntity?.SetAnimParameter( "b_holster", true );
	}

	public void Holster()
	{
		if ( Game.IsServer )
		{
			foreach ( var attachment in Attachments )
			{
				attachment.Enabled = false;
			}

			RpcHolster( To.Single( Owner ) );
		}
	}

	public int AvailableAmmo()
	{
		var owner = Owner as GunfightPlayer;
		if ( owner == null ) return 0;
		return owner.AmmoCount( AmmoType );
	}

	private ParticleSystem EjectBrass;

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

		Sound.FromEntity( "sounds/guns/switch/weapon_switch.sound", ent );
		
		EjectBrass = Cloud.ParticleSystem( "https://asset.party/facepunch/9mm_ejectbrass" );

		if ( Game.IsClient )
		{
			DeployAsync();
		}

		foreach ( var attachment in Attachments )
		{
			if ( attachment.IsSupported( this ) )
			{
				attachment.Enabled = true;
			}
		}
	}

	async void DeployAsync()
	{
		ViewModelEntity?.SetAnimParameter( "b_deploy", true );
		await Task.Delay( 100 );
		ViewModelEntity?.SetAnimParameter( "b_deploy", false );
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

	public override void PostCreateViewModel()
	{
		foreach ( var attachment in Attachments )
		{
			if ( attachment.IsSupported( this ) )
			{
				attachment.SetupViewModel( ViewModelEntity );
			}
		}
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

		(Owner as AnimatedEntity)?.SetAnimParameter( "b_reload", true );
		ViewModelEntity?.SetAnimParameter( "b_reload", true );

		StartReloadEffects();
	}

	public virtual bool WantsReload()
	{
		return Input.Down( "Reload" );
	}

	[Net, Predicted] public float RecoilDispersion { get; set; } = 0;
	public float MaxRecoilDispersion => 0.15f;
	public float RecoilDispersionRate => 3f;
	
	protected virtual void SimulateRecoil( IClient owner )
	{
		if ( TimeSincePrimaryAttack > RecoilDispersion || AmmoClip < 1 )
		{
			CameraRecoil = CameraRecoil.LerpTo( 0, Time.Delta * 10f );
			WeaponSpreadRecoil = WeaponSpreadRecoil.LerpTo( 0, Time.Delta * 10f );
			
			RecoilDispersion -= RecoilDispersionRate * 0.5f;
			RecoilDispersion = RecoilDispersion.Clamp( 0, MaxRecoilDispersion );
		}

		if ( Input.AnalogLook.pitch > 0f )
		{ 
			var pitchDelta = Input.AnalogLook.pitch;
			RecoilDispersion -= pitchDelta * Time.Delta;
		}

		// Clamp down to zero
		WeaponSpreadRecoil = WeaponSpreadRecoil.Clamp( 0, 2.5f );
		CameraRecoil = CameraRecoil.Clamp( 0, 20f );
	}

	protected virtual void ApplyRecoil()
	{
		Game.SetRandomSeed( Time.Tick );

		var randX = Game.Random.Float( WeaponDefinition.Recoil.BaseRecoilMinimum.x, WeaponDefinition.Recoil.BaseRecoilMaximum.x );
		var randY = Game.Random.Float( WeaponDefinition.Recoil.BaseRecoilMinimum.y, WeaponDefinition.Recoil.BaseRecoilMaximum.y );
		var randSpreadX = Game.Random.Float( WeaponDefinition.Recoil.MinimumSpread.x, WeaponDefinition.Recoil.MaximumSpread.x );
		var randSpreadY = Game.Random.Float( WeaponDefinition.Recoil.MinimumSpread.y, WeaponDefinition.Recoil.MaximumSpread.y );

		var recoilScale = 1f;
		bool isInAir = !PlayerController.GroundEntity.IsValid();

		// Recoil gets decreased when aiming down the sights.
		if ( IsAiming )
			recoilScale *= 0.75f;

		// Recoil gets decreased when ducking.
		if ( !isInAir && PlayerController.Duck.IsActive )
			recoilScale *= 0.75f;

		if ( PlayerController.CoverAim.IsActive )
			recoilScale *= 0.5f;

		if ( isInAir )
		{
			recoilScale *= 3f;
		}
		
		var spreadScale = recoilScale;

		// If you're moving at speed, apply more recoil.
		var speed = Player.Velocity.Length.LerpInverse( 0, 400, true );

		spreadScale += speed;
		
		CameraRecoil += new Vector2( randX, randY ) * recoilScale;
		// Apply spread too
		WeaponSpreadRecoil += new Vector2( randSpreadX, randSpreadY ) * spreadScale;

		RecoilDispersion += RecoilDispersionRate * recoilScale * Time.Delta;
		RecoilDispersion = RecoilDispersion.Clamp( 0, MaxRecoilDispersion );
	}

	public override void BuildInput()
	{
		if ( !Player.IsValid() ) return;

		var viewAngles = Player.LookInput;
		viewAngles.pitch -= CameraRecoil.y * Time.Delta;
		viewAngles.yaw -= CameraRecoil.x * Time.Delta;
		Player.LookInput = viewAngles;
	}

	[Net, Predicted] public TimeSince TimeSinceStartFiring { get; set; }

	public override void Simulate( IClient cl )
	{
		base.Simulate( cl );

		ViewModelEntity?.SetAnimParameter( "b_grounded", Player.Controller.GroundEntity.IsValid() );


		IsFiring = false;
		
		if ( TimeSinceDeployed < DeployTime )
			return;

		if ( Input.Pressed( "FireMode" ) )
		{
			CycleFireMode();
			return;
		}

		if ( IsBurst && BurstCount >= WeaponDefinition.BurstAmount )
		{
			TimeSinceBurstFinished = 0;
			IsBurstFiring = false;
			BurstCount = 0;
		}

		if ( IsBurst && AmmoClip == 0 ) IsBurstFiring = false;

		SimulateRecoil( cl );

		if ( WantsReload() )
		{
			Reload();
			return;
		}

		// Reload cancelling
		// if ( IsReloading && Input.Pressed( InputButton.PrimaryAttack ) )
		// {
		// 	IsReloading = false;
		// 	TimeSinceReload = ReloadTime;
		// }

		//
		// Reload could have changed our owner
		//
		if ( !Owner.IsValid() )
			return;
		
		if ( !Input.Down( "Attack1" ) )
		{
			TimeSinceStartFiring = 0;
		}

		if ( CanPrimaryAttack() )
		{
			TimeSincePrimaryAttack = 0;
			AttackPrimary();
		}

		if ( IsReloading && TimeSinceReload > ReloadTime )
		{
			OnReloadFinish();
		}
	}
	
	[Net, Predicted] public bool IsFiring { get; set; }
	public bool IsTriggerHeld => Input.Down( "Attack1" );
  
	protected override void InitializeWeapon( WeaponDefinition def )
	{
		base.InitializeWeapon( def );

		AmmoClip = def.StandardClip;
		CurrentFireMode = def.DefaultFireMode;
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

	protected virtual float GetFireRate()
	{
		return CurrentFireMode == FireMode.Burst ? WeaponDefinition.BurstFireRate : WeaponDefinition.BaseFireRate;
	}

	protected bool CanDefaultPrimaryAttack()
	{
		if ( Player.IsHolstering ) return false;

		if ( GamemodeSystem.Current?.AllowMovement == false ) return false;
		
		if ( TimeSinceDeployed < 0.2f ) return false;
		//if ( !PlayerController?.AimFireDelay ?? false ) return false;
		//if ( PlayerController.Slide.IsActive ) return false;
		if ( IsSprinting ) return false;
		if ( IsReloading ) return false;
		if ( PlayerController?.SinceStoppedSprinting <= PostSprintAttackDelay ) return false;

		return TimeSincePrimaryAttack >= GetFireRate();
	}

	protected bool CanPrimaryAttackSemi()
	{
		return Input.Pressed( "Attack1" );
	}

	protected bool CanPrimaryAttackBurst()
	{
		if ( Input.Down( "Attack1" ) && TimeSinceBurstFinished >= WeaponDefinition.BurstCooldown )
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

		return TimeSincePrimaryAttack >= PrimaryFireRate && IsTriggerHeld;
	}

	public string FireSound => Attachments.Select( x => x.GetSound( "fire" ) ).FirstOrDefault( x => x is not null ) ?? WeaponDefinition.FireSound;

	public virtual void AttackPrimary()
	{
		TimeSincePrimaryAttack = 0;

		if ( !IsUnlimitedAmmo() && !TakeAmmo( 1 ) )
		{
			if ( Input.Pressed( "Attack1" ) )
			{
				DryFire();
			}
			return;
		}

		(Owner as AnimatedEntity)?.SetAnimParameter( "b_attack", true );
		ViewModelEntity?.SetAnimParameter( "b_attack", true );

		//
		// Tell the clients to play the shoot effects
		//
		ShootEffects();
		Owner?.PlaySound( FireSound );

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
		Game.AssertClient();

		ViewModelEntity?.SetAnimParameter( "fire", true );
		CrosshairLastShoot = 0;

		if ( IsLowAmmo() )
			AmmoLowSound( AmmoClip / (float)ClipSize );
		
		if ( EjectBrass != null ) Particles.Create( EjectBrass.ResourcePath, EffectEntity, "eject" );

		Particles.Create( "particles/pistol_muzzleflash.vpcf", EffectEntity, "muzzle" );
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
		if ( !WeaponDefinition.DamageFlags.Contains( "bullet" ) )
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
			var dist = tr.Distance.Remap( 0, WeaponDefinition.MaxEffectiveRange, 1, 0.5f ).Clamp( 0.5f, 1f );
			damage *= dist;

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

	public virtual float GetAimTime()
	{
		var aimTime = BaseAimTime;
		return aimTime.Clamp( 0, 1 );
	}

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
		Game.SetRandomSeed( Time.Tick );

		for ( int i = 0; i < bulletCount; i++ )
		{
			var rot = Rotation.LookAt( Owner.AimRay.Forward );
			var weaponRecoil = WeaponSpreadRecoil;
			rot *= Rotation.From( new Angles( -weaponRecoil.y, -weaponRecoil.x, 0 ) );

			var forward = rot.Forward;
			forward += (Vector3.Random + Vector3.Random + Vector3.Random + Vector3.Random) * spread * 0.25f;
			forward = forward.Normal;

			var damage = BulletDamage;

			Vector3 LastImpact = Vector3.Zero;
			int count = 0;
			foreach ( var tr in TraceBullet( Owner.AimRay.Position, Owner.AimRay.Position + forward * bulletRange, bulletSize, ref damage ) )
			{
				tr.Surface.DoBulletImpact( tr );

				if ( !Game.IsServer ) continue;
				if ( !tr.Entity.IsValid() ) continue;

				var damageInfo = DamageInfo.FromBullet( tr.EndPosition, forward * 100 * force, damage )
					.UsingTraceResult( tr )
					.WithAttacker( Owner )
					.WithWeapon( this );

				tr.Entity.TakeDamage( damageInfo );

				if ( WeaponDefinition.DamageFlags.Contains( "bullet" ) )
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
		(Owner as AnimatedEntity)?.SetAnimParameter( "b_attack_dry", true );
		ViewModelEntity?.SetAnimParameter( "b_attack_dry", true );

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

	public override string ToString()
	{
		return $"GunfightWeapon[{WeaponDefinition?.WeaponName ?? "base"}]";
	}
}

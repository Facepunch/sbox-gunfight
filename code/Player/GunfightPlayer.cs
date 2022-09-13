﻿namespace Facepunch.Gunfight;

public partial class GunfightPlayer : Player, IHudMarker
{
	[Net] public float Armour { get; set; }
	[Net] public float MaxHealth { get; set; }
	[Net] public PlayerInventory PlayerInventory { get; set; }
	[Net, Predicted] public TimeSince TimeSinceDropped { get; set; }

	public string SpawnPointTag { get; set; } = null;

	public bool SupressPickupNotices { get; private set; }
	public bool IsRegen { get; set; }
	public new PlayerInventory Inventory => PlayerInventory;

	public override void Respawn()
	{
		SetModel( "models/citizen/citizen.vmdl" );

		Animator = new PlayerAnimator();
		CameraMode = new FirstPersonCamera();
		Controller = new PlayerController();

		PlayerInventory?.DeleteContents();
		PlayerInventory = new PlayerInventory( this );

		EnableAllCollisions = true;
		EnableDrawing = true;
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;

		ClearAmmo();
		Clothing.DressEntity( this );

		SupressPickupNotices = true;

		Inventory.DeleteContents();

		GiveAll();

		SupressPickupNotices = false;

		MaxHealth = 100;
		Health = 100;
		Armour = 0;

		base.Respawn();
	}

	public void GiveAll()
	{
		GiveAmmo( AmmoType.Pistol, MaxAmmo( AmmoType.Pistol ) );
		GiveAmmo( AmmoType.SMG, MaxAmmo( AmmoType.SMG ) );
		GiveAmmo( AmmoType.Rifle, MaxAmmo( AmmoType.Rifle ) );
		GiveAmmo( AmmoType.DMR, MaxAmmo( AmmoType.DMR ) );
		GiveAmmo( AmmoType.Sniper, MaxAmmo( AmmoType.Sniper ) );
		GiveAmmo( AmmoType.Shotgun, MaxAmmo( AmmoType.Shotgun ) );

		GiveWeapon( "knife" );
		GiveWeapon( "1911" );

		Rand.SetSeed( Time.Tick );
		var rand = Rand.Int( 0, 2 );

		if ( rand == 0 )
			GiveWeapon( "mp5", true );
		else if ( rand == 1 )
			GiveWeapon( "r870", true );
		else
			GiveWeapon( "famas", true );
	}

	public void GiveWeapon( string name, bool makeActive = false )
	{
		Inventory.Add( WeaponDefinition.CreateWeapon( name ), makeActive );
	}

	public override void OnKilled()
	{
		base.OnKilled();

		var primary = Inventory.PrimaryWeapon;
		if ( Inventory.Drop( primary ) )
		{
			primary.StartDecaying();
		}

		Inventory.DeleteContents();

		if ( LastDamage.Flags.HasFlag( DamageFlags.Blast ) )
		{
			using ( Prediction.Off() )
			{
				var particles = Particles.Create( "particles/gib.vpcf" );
				if ( particles != null )
				{
					particles.SetPosition( 0, Position + Vector3.Up * 40 );
				}
			}
		}
		else
		{
			BecomeRagdollOnClient( Velocity, LastDamage.Flags, LastDamage.Position, LastDamage.Force, GetHitboxBone( LastDamage.HitboxIndex ) );
		}

		Controller = null;

		CameraMode = new SpectateRagdollCamera();

		EnableAllCollisions = false;
		EnableDrawing = false;

		foreach ( var child in Children.OfType<ModelEntity>() )
		{
			child.EnableDrawing = false;
		}

		// Inform the active gamemode
		GamemodeSystem.Current?.OnPlayerKilled( this, LastDamage );
	}

	protected void SimulateView()
	{
		if ( Input.Pressed( InputButton.View ) )
		{
			if ( CameraMode is ThirdPersonCamera )
			{
				CameraMode = new FirstPersonCamera();
			}
			else
			{
				CameraMode = new ThirdPersonCamera();
			}
		}
	}

	protected void SimulateWeapons( Client cl )
	{
		//
		// Input requested a weapon switch
		//
		if ( Input.ActiveChild != null )
			ActiveChild = Input.ActiveChild;

		SimulateActiveChild( cl, ActiveChild );

		//
		// If the current weapon is out of ammo and we last fired it over half a second ago
		// lets try to switch to a better wepaon
		//
		if ( ActiveChild is GunfightWeapon weapon && !weapon.IsUsable() && weapon.TimeSincePrimaryAttack > 0.5f )
		{
			SwitchToBestWeapon();
		}
	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		if ( LifeState != LifeState.Alive )
			return;

		TickPlayerUse();
		SimulateView();
		SimulateWeapons( cl );

		if ( TimeSinceDamage > 5f )
		{
			PassiveHeal();
			if ( Health != MaxHealth )
			{
				IsRegen = true;
			}
			else
			{
				IsRegen = false;
			}
		}
		else
		{
			IsRegen = false;
		}
	}

	protected void PassiveHeal()
	{
		Health += 10f * Time.Delta;
		Health = Health.Clamp( 0, MaxHealth );
	}

	public void SwitchToBestWeapon()
	{
		var best = Children.Select( x => x as GunfightWeapon )
			.Where( x => x.IsValid() && x.IsUsable() )
			.FirstOrDefault();

		if ( best == null ) return;

		ActiveChild = best;
	}

	public override void StartTouch( Entity other )
	{
		if ( TimeSinceDropped < 1f ) return;

		base.StartTouch( other );

		if ( other is GunfightWeapon weapon )
		{
			var ammoType = weapon.AmmoType;

			// Must have a weapon with the correct ammo type in inventory
			if ( !Inventory.HasWeaponWithAmmoType( ammoType ) )
				return;

			var taken = GiveAmmo( ammoType, weapon.AmmoClip );

			weapon.AmmoClip -= taken;

			if ( weapon.AmmoClip <= 0 )
				weapon.Delete();
		}
	}

	private bool OverrideViewAngles = false;
	private Angles NewViewAngles;
	[ClientRpc]
	public void SetViewAngles( Angles angles )
	{
		OverrideViewAngles = true;
		NewViewAngles = angles;
	}

	public override void PostCameraSetup( ref CameraSetup setup )
	{
		base.PostCameraSetup( ref setup );

		//if ( setup.Viewer != null )
		//{
		//	AddCameraEffects( ref setup );
		//}
	}

	float walkBob = 0;

	private void AddCameraEffects( ref CameraSetup setup )
	{
		var speed = Velocity.Length.LerpInverse( 0, 270 );

		var left = setup.Rotation.Left;
		var up = setup.Rotation.Up;

		if ( GroundEntity != null )
			walkBob += Time.Delta * 3f * speed;

		setup.Position += up * MathF.Sin( walkBob ) * speed * 2;
		setup.Position += left * MathF.Sin( walkBob ) * speed * -1f;
	}

	DamageInfo LastDamage;

	public override void TakeDamage( DamageInfo info )
	{
		if ( LifeState == LifeState.Dead )
			return;

		if ( !GamemodeSystem.Current?.AllowDamage() ?? true )
			return;

		LastDamage = info;

		// Headshot
		var isHeadshot = GetHitboxGroup( info.HitboxIndex ) == 1;
		if ( isHeadshot )
		{
			info.Damage *= 2.0f;
		}

		this.ProceduralHitReaction( info );

		LastAttacker = info.Attacker;
		LastAttackerWeapon = info.Weapon;

		if ( IsServer && Armour > 0 )
		{
			Armour -= info.Damage;

			if ( Armour < 0 )
			{
				info.Damage = Armour * -1;
				Armour = 0;
			}
			else
			{
				info.Damage = 0;
			}
		}

		if ( info.Flags.HasFlag( DamageFlags.Blast ) )
		{
			Deafen( To.Single( Client ), info.Damage.LerpInverse( 0, 60 ) );
		}

		if ( Health > 0 && info.Damage > 0 )
		{
			Health -= info.Damage;
			if ( Health <= 0 )
			{
				Health = 0;
				OnKilled();
			}
		}

		TimeSinceDamage = 0;

		if ( info.Attacker is GunfightPlayer attacker )
		{
			if ( attacker != this )
			{
				attacker.DidDamage( To.Single( attacker ), info.Position, info.Damage, Health.LerpInverse( 100, 0 ), isHeadshot );
			}

			TookDamage( To.Single( this ), info.Weapon.IsValid() ? info.Weapon.Position : info.Attacker.Position );
		}

		//
		// Add a score to the killer
		//
		if ( LifeState == LifeState.Dead && info.Attacker != null )
		{
			if ( info.Attacker.Client != null && info.Attacker != this )
			{
				info.Attacker.Client.AddInt( "kills" );
			}
		}
	}

	[ClientRpc]
	public void DidDamage( Vector3 pos, float amount, float healthinv, bool isHeadshot )
	{
		if ( isHeadshot )
		{
			Sound.FromScreen( "ui.hit" ).SetPitch( 1.25f );
		}
		else
		{
			Sound.FromScreen( "ui.hit" );
		}
		HitIndicator.Current?.OnHit( pos, amount, isHeadshot );
	}

	[Net, Predicted] public TimeSince TimeSinceDamage { get; set; }

	[ClientRpc]
	public void TookDamage( Vector3 pos, bool headshot = false )
	{
		DamageIndicator.Current?.OnHit( pos, headshot );
	}

	[ClientRpc]
	public void PlaySoundFromScreen( string sound )
	{
		Sound.FromScreen( sound );
	}

	TimeSince timeSinceLastFootstep = 0;

	public override void OnAnimEventFootstep( Vector3 pos, int foot, float volume )
	{
		if ( !IsServer )
			return;

		if ( LifeState != LifeState.Alive )
			return;

		if ( timeSinceLastFootstep < 0.18f )
			return;

		var ctrl = Controller as PlayerController;

		// No footsteps while sliding
		if ( ctrl.Slide.IsActive )
			return;

		volume *= FootstepVolume();

		timeSinceLastFootstep = 0;

		var tr = Trace.Ray( pos, pos + Vector3.Down * 20 )
			.Radius( 1 )
			.Ignore( this )
			.Run();

		if ( !tr.Hit ) return;

		if ( ctrl.IsSprinting )
		{
			var sound = PlaySound( "sounds/player/foley/gear/player.walk.gear.sound" );
			sound.SetVolume( volume * 0.5f );
		}

		tr.Surface.DoFootstep( this, tr, foot, volume * 20 );
	}

	public void RenderHud( Vector2 screenSize )
	{
		if ( LifeState != LifeState.Alive )
			return;

		if ( ActiveChild is GunfightWeapon weapon )
		{
			weapon.RenderHud( screenSize );
		}
	}
}

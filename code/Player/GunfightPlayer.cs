﻿namespace Facepunch.Gunfight;

public partial class GunfightPlayer : Player
{
	TimeSince timeSinceDropped;

	[Net]
	public float Armour { get; set; } = 0;

	[Net]
	public float MaxHealth { get; set; } = 100;

	public bool SupressPickupNotices { get; private set; }

	public int ComboKillCount { get; set; } = 0;
	public TimeSince TimeSinceLastKill { get; set; }

	public GunfightPlayer()
	{
		Inventory = new GunfightInventory( this );
	}

	public override void Respawn()
	{
		SetModel( "models/citizen/citizen.vmdl" );

		Animator = new StandardPlayerAnimator();
		CameraMode = new FirstPersonCamera();
		Controller = new PlayerController();

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
		Health = 100;
		Armour = 0;

		base.Respawn();
	}


	public void GiveAll()
	{
		GiveAmmo( AmmoType.Pistol, 256 );
		GiveAmmo( AmmoType.Buckshot, 64 );
		GiveAmmo( AmmoType.Crossbow, 16 );

		Inventory.Add( new Pistol() );
		Inventory.Add( new Shotgun() );
		Inventory.Add( new SMG() );
		Inventory.Add( new Crossbow() );
	}

	public override void OnKilled()
	{
		base.OnKilled();

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
			BecomeRagdollOnClient( LastDamage.Force, GetHitboxBone( LastDamage.HitboxIndex ) );
		}

		Controller = null;

		CameraMode = new SpectateRagdollCamera();

		EnableAllCollisions = false;
		EnableDrawing = false;

		foreach ( var child in Children.OfType<ModelEntity>() )
		{
			child.EnableDrawing = false;
		}
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
		if ( Input.Pressed( InputButton.Drop ) )
		{
			var dropped = Inventory.DropActive();
			if ( dropped != null )
			{
				if ( dropped.PhysicsGroup != null )
					dropped.PhysicsGroup.Velocity = Velocity + (EyeRotation.Forward + EyeRotation.Up) * 300;

				timeSinceDropped = 0;
				SwitchToBestWeapon();
			}
		}

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
		if ( ActiveChild is ShooterWeapon weapon && !weapon.IsUsable() && weapon.TimeSincePrimaryAttack > 0.5f && weapon.TimeSinceSecondaryAttack > 0.5f )
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
	}

	public void SwitchToBestWeapon()
	{
		var best = Children.Select( x => x as ShooterWeapon )
			.Where( x => x.IsValid() && x.IsUsable() )
			.OrderByDescending( x => x.BucketWeight )
			.FirstOrDefault();

		if ( best == null ) return;

		ActiveChild = best;
	}

	public override void StartTouch( Entity other )
	{
		if ( timeSinceDropped < 1 ) return;

		base.StartTouch( other );
	}

	public override void PostCameraSetup( ref CameraSetup setup )
	{
		base.PostCameraSetup( ref setup );

		if ( setup.Viewer != null )
		{
			AddCameraEffects( ref setup );
		}
	}

	float walkBob = 0;
	float lean = 0;
	float fov = 0;

	private void AddCameraEffects( ref CameraSetup setup )
	{
		var speed = Velocity.Length.LerpInverse( 0, 270 );
		var forwardspeed = Velocity.Normal.Dot( setup.Rotation.Forward );

		var left = setup.Rotation.Left;
		var up = setup.Rotation.Up;

		if ( GroundEntity != null )
			walkBob += Time.Delta * 12f * speed;

		setup.Position += up * MathF.Sin( walkBob ) * speed * 2;
		setup.Position += left * MathF.Sin( walkBob ) * speed * -1f;

		// Camera lean
		lean = lean.LerpTo( Velocity.Dot( setup.Rotation.Right ) * 0.01f, Time.Delta * 10.0f );

		var appliedLean = lean;
		appliedLean += MathF.Cos( walkBob ) * speed * 0.5f;
		setup.Rotation *= Rotation.From( 0, 0, appliedLean );

		speed = (speed - 0.7f).Clamp( 0, 1 ) * 3.0f;

		fov = fov.LerpTo( speed * 5 * MathF.Abs( forwardspeed ), Time.Delta * 10.0f );

		setup.FieldOfView += fov;

	}

	DamageInfo LastDamage;

	public override void TakeDamage( DamageInfo info )
	{
		if ( LifeState == LifeState.Dead )
			return;

		LastDamage = info;

		if ( GetHitboxGroup( info.HitboxIndex ) == 1 )
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

		if ( info.Attacker is GunfightPlayer attacker )
		{
			if ( attacker != this )
			{
				attacker.DidDamage( To.Single( attacker ), info.Position, info.Damage, Health.LerpInverse( 100, 0 ) );
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
	public void DidDamage( Vector3 pos, float amount, float healthinv )
	{
		Sound.FromScreen( "dm.ui_attacker" )
			.SetPitch( 1 + healthinv * 1 );

		HitIndicator.Current?.OnHit( pos, amount );
	}

	public TimeSince TimeSinceDamage = 1.0f;

	[ClientRpc]
	public void TookDamage( Vector3 pos )
	{
		TimeSinceDamage = 0;
		DamageIndicator.Current?.OnHit( pos );
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

		if ( timeSinceLastFootstep < 0.2f )
			return;

		volume *= FootstepVolume();

		timeSinceLastFootstep = 0;

		var tr = Trace.Ray( pos, pos + Vector3.Down * 20 )
			.Radius( 1 )
			.Ignore( this )
			.Run();

		if ( !tr.Hit ) return;

		tr.Surface.DoFootstep( this, tr, foot, volume * 10 );
	}

	public void RenderHud( Vector2 screenSize )
	{
		if ( LifeState != LifeState.Alive )
			return;

		if ( ActiveChild is ShooterWeapon weapon )
		{
			weapon.RenderHud( screenSize );
		}
	}
}
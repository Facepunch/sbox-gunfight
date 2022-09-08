namespace Facepunch.Gunfight;

public partial class GunfightPlayer : Player
{
	TimeSince timeSinceDropped;

	[Net]
	public float Armour { get; set; } = 25;

	[Net]
	public float MaxHealth { get; set; } = 100;

	public bool SupressPickupNotices { get; private set; }

	public int ComboKillCount { get; set; } = 0;
	public TimeSince TimeSinceLastKill { get; set; }

	[Net] public PlayerInventory PlayerInventory { get; set; }
	public new PlayerInventory Inventory => PlayerInventory;

	public override void Respawn()
	{
		SetModel( "models/citizen/citizen.vmdl" );

		Animator = new PlayerAnimator();
		CameraMode = new FirstPersonCamera();
		Controller = new PlayerController();
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
		GiveWeapon( "r870" );
		GiveWeapon( "mp5", true );
	}

	public void GiveWeapon( string name, bool makeActive = false )
	{
		Inventory.Add( WeaponDefinition.CreateWeapon( name ), makeActive );
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
		if ( timeSinceDropped < 1 ) return;

		base.StartTouch( other );
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
		if(isHeadshot)
		{
			Sound.FromScreen( "ui.hit" ).SetPitch( 1.25f );
		}
		else
		{
			Sound.FromScreen( "ui.hit" );
		}
		HitIndicator.Current?.OnHit( pos, amount, isHeadshot );
	}

	public TimeSince TimeSinceDamage = 1.0f;

	[ClientRpc]
	public void TookDamage( Vector3 pos, bool headshot = false )
	{
		TimeSinceDamage = 0;
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

		// No footsteps while sliding
		if ( (Controller as PlayerController).Slide.IsActive )
			return;

		volume *= FootstepVolume();

		timeSinceLastFootstep = 0;

		var tr = Trace.Ray( pos, pos + Vector3.Down * 20 )
			.Radius( 1 )
			.Ignore( this )
			.Run();

		if ( !tr.Hit ) return;

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

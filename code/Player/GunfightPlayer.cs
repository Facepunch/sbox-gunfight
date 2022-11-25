namespace Facepunch.Gunfight;

public partial class GunfightPlayer : Player, IHudMarker
{
	[Net] public float Armour { get; set; }
	[Net] public float MaxHealth { get; set; }
	[Net] public PlayerInventory PlayerInventory { get; set; }
	[Net, Predicted] public TimeSince TimeSinceDropped { get; set; }
	[Net] public CapturePointEntity CapturePoint { get; set; }
	[Net] public string PlayerLocation { get; set; } = "";
	public string SpawnPointTag { get; set; } = null;

	[Net, Predicted] public TimeUntil TimeUntilHolstered { get; set; } = -1;
	[Net, Predicted] public bool IsHolstering { get; set; } = false;

	public bool SupressPickupNotices { get; private set; }
	public bool IsRegen { get; set; }
	public new PlayerInventory Inventory => PlayerInventory;

	public override void Respawn()
	{
		SetModel( "models/citizen/citizen.vmdl" );

		Animator = new PlayerAnimator();
		CameraMode = new GunfightPlayerCamera();
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
		ClearEffects();
		ClearKillStreak();

		SupressPickupNotices = false;

		Transmit = TransmitType.Always;
		MaxHealth = 100;
		Health = 100;
		Armour = 0;

		Tags.Add( "player" );

		base.Respawn();
	}

	public void GiveAll()
	{
		var overrideLoadout = GamemodeSystem.Current?.PlayerLoadout( this ) ?? false;
		// Use a default loadout
		if ( !overrideLoadout )
		{
			GiveAmmo( AmmoType.Pistol, MaxAmmo( AmmoType.Pistol ) );
			GiveAmmo( AmmoType.SMG, MaxAmmo( AmmoType.SMG ) );
			GiveAmmo( AmmoType.Rifle, MaxAmmo( AmmoType.Rifle ) );
			GiveAmmo( AmmoType.DMR, MaxAmmo( AmmoType.DMR ) );
			GiveAmmo( AmmoType.Sniper, MaxAmmo( AmmoType.Sniper ) );
			GiveAmmo( AmmoType.Shotgun, MaxAmmo( AmmoType.Shotgun ) );

			var primary = WeaponDefinition.Random( WeaponDefinition.FindFromSlot( WeaponSlot.Primary ) );
			GiveWeapon( primary, true );

			var secondary = WeaponDefinition.Random( WeaponDefinition.FindFromSlot( WeaponSlot.Secondary ) );
			GiveWeapon( secondary );
		}
	}

	public GunfightWeapon GiveWeapon( string name, bool makeActive = false, params string[] attachments )
	{
		var wpn = WeaponDefinition.CreateWeapon( name );
		Inventory.Add( wpn, makeActive );

		foreach ( var str in attachments )
			wpn.CreateAttachment( str );

		return wpn;
	}

	public GunfightWeapon GiveWeapon( WeaponDefinition def, bool makeActive = false, params string[] attachments )
	{
		var wpn = WeaponDefinition.CreateWeapon( def );
		Inventory.Add( wpn, makeActive );

		foreach( var str in attachments )
			wpn.CreateAttachment( str );

		return wpn;
	}

	[ClientRpc]
	public void ClearEffects()
	{
		StopSlidingEffects();
	}

	public Particles SlidingParticles { get; set; }
	public Sound SlidingSound { get; set; }
	bool SlidingEffectsActive { get; set; } = false;

	public void StartSlidingEffects()
	{
		if ( SlidingEffectsActive ) return;

		SlidingEffectsActive = true;
		SlidingParticles?.Destroy( true );
		SlidingParticles = Particles.Create( "particles/gameplay/player/slide/slide.vpcf", this, true );

		SlidingSound.Stop();
		SlidingSound = Sound.FromEntity( "sounds/player/foley/slide/ski.loop.sound", this );
	}

	public void StopSlidingEffects( bool stopSound = true )
	{
		SlidingEffectsActive = false;
		SlidingParticles?.Destroy( true );
		SlidingSound.Stop();

		if ( stopSound )
			Sound.FromEntity( "sounds/player/foley/slide/ski.stop.sound", this );
	}

	public override void OnKilled()
	{
		Game.Current?.OnKilled( this );

		// Default life state is Respawning, this means the player will handle respawning after a few seconds
		LifeState newLifeState = LifeState.Respawning;

		// Inform the active gamemode
		GamemodeSystem.Current?.OnPlayerKilled( this, LastDamage, out newLifeState );

		TimeSinceKilled = 0;
		LifeState = newLifeState;
		StopUsing();
		Client?.AddInt( "deaths", 1 );

		if ( LastDamage.Attacker.IsValid() )
		{
			Progression.GiveAward( LastDamage.Attacker.Client, "Kill" );
			( LastDamage.Attacker as GunfightPlayer )?.AddKill();
		}

		if ( CapturePoint.IsValid() )
		{
			CapturePoint.RemovePlayer( this );
		}

		var primary = Inventory.PrimaryWeapon;
		if ( primary.IsValid() && Inventory.Drop( primary ) )
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
			BecomeRagdollOnClient( Velocity, LastDamage.Flags, LastDamage.Position, LastDamage.Force, LastDamage.BoneIndex );
		}

		ClearEffects();

		Controller = null;

		CameraMode = new GunfightDeathCamera( LastDamage.Attacker.IsValid() ? LastDamage.Attacker : this );

		EnableAllCollisions = false;
		EnableDrawing = false;

		foreach ( var child in Children.OfType<ModelEntity>() )
		{
			child.EnableDrawing = false;
		}

		// Inform the active gamemode
		GamemodeSystem.Current?.PostPlayerKilled( this, LastDamage );
	}

	TimeSince TimeSinceKilled;
	public override void Simulate( Client cl )
	{
		if ( LifeState == LifeState.Respawning )
		{
			if ( TimeSinceKilled > 3 && IsServer )
			{
				Respawn();
			}

			return;
		}

		var controller = GetActiveController();
		controller?.Simulate( cl, this, GetActiveAnimator() );

		if ( Input.Pressed( InputButton.View ) )
		{
			if ( GamemodeSystem.Current?.AllowThirdPerson ?? false )
			{
				GunfightCamera.IsThirdPerson ^= true;
			}
		}

		if ( LifeState != LifeState.Alive )
			return;

		TickPlayerUse();
		SimulateWeapons( cl );
		SimulatePing( cl );

		if ( TimeSinceDamage > 5f && ( GamemodeSystem.Current?.CanPlayerRegenerate( this ) ?? true ) )
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

		if ( setup.Viewer != null )
		{
			AddCameraEffects( ref setup );
		}
	}

	float WalkBob = 0;
	private void AddCameraEffects( ref CameraSetup setup )
	{
		var speed = Velocity.Length.LerpInverse( 0, 350 );

		var left = setup.Rotation.Left;
		var up = setup.Rotation.Up;

		if ( GroundEntity != null )
			WalkBob += Time.Delta * 10f * speed;

		setup.Position += up * MathF.Sin( WalkBob ) * speed * 2;
		setup.Position += left * MathF.Sin( WalkBob ) * speed * -1f;

		GunfightGame.AddedCameraFOV = 0f;
		var ctrl = Controller as PlayerController;
		if ( ctrl != null )
		{
			if ( ctrl.IsBurstSprinting )
			{
				GunfightGame.AddedCameraFOV = 10f;
			}
		}
	}

	DamageInfo LastDamage;

	public override void TakeDamage( DamageInfo info )
	{
		if ( LifeState != LifeState.Alive )
			return;

		if ( GamemodeSystem.Current.IsValid() && !GamemodeSystem.Current.AllowDamage )
			return;

		var attacker = info.Attacker as GunfightPlayer;
		if ( attacker.IsValid() && !GamemodeSystem.Current.AllowFriendlyFire && Gamemode.FriendlyFireOverride == false )
		{
			if ( attacker.Team == Team )
				return;
		}

		LastDamage = info;

		// Headshot
		var isHeadshot = info.Hitbox.HasTag( "head" );
		if ( isHeadshot )
		{
			info.Damage *= 2.5f;
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

		if ( attacker.IsValid() )
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
		if ( LifeState != LifeState.Alive && info.Attacker != null )
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

		var camera = Client.Components.Get<DevCamera>( false );
		if ( camera is not null )
			return;

		if ( ActiveChild is GunfightWeapon weapon )
		{
			weapon.RenderHud( screenSize );
		}
	}

	[ConCmd.Admin( "gunfight_debug_sethp" )]
	public static void Debug_SetHP( int hp )
	{
		var pawn = ConsoleSystem.Caller?.Pawn;
		if ( pawn.IsValid() )
		{
			pawn.Health = hp;
		}
	}
	[ConCmd.Admin( "gunfight_debug_damage" )]
	public static void Debug_Damage( int amt )
	{
		var pawn = ConsoleSystem.Caller?.Pawn;
		if ( pawn.IsValid() )
		{
			pawn.TakeDamage( DamageInfo.FromBullet( pawn.Position, 100f, amt ) );
		}
	}
}

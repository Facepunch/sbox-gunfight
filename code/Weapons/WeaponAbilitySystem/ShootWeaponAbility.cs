using Sandbox;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;

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
	/// Gets a surface from a trace. Trying to find a SurfaceComponent before using the one from the model.
	/// </summary>
	/// <param name="tr"></param>
	/// <returns></returns>
	private Surface GetSurfaceFromTrace( SceneTraceResult tr )
	{
		if ( tr.GameObject?.GetSurface() is Surface surface )
		{
			return surface;
		}

		return tr.Surface;
	}

	private LegacyParticleSystem CreateParticleSystem( string particle, Vector3 pos, Rotation rot, List<ParticleControlPoint> cps = null, float decay = 5f )
	{
		var gameObject = Scene.CreateObject();
		gameObject.Transform.Position = pos;
		gameObject.Transform.Rotation = rot;

		var p = gameObject.Components.Create<LegacyParticleSystem>();
		p.Particles = ParticleSystem.Load( particle );
		p.ControlPoints = cps ?? new()
		{
			new() { Value = ParticleControlPoint.ControlPointValueInput.Vector3, VectorValue = pos }
		};

		// Clear off in a suitable amount of time.
		gameObject.DestroyAsync( decay );

		return p;
	}

	private void CreateImpactEffects( GameObject hitObject, Surface surface, Vector3 pos, Vector3 normal )
	{
		var decalPath = Game.Random.FromArray( surface.ImpactEffects.BulletDecal, "decals/bullethole.decal" );
		if ( ResourceLibrary.TryGet<DecalDefinition>( decalPath, out var decalResource ) )
		{
			CreateParticleSystem( Game.Random.FromArray( surface.ImpactEffects.Bullet ), pos, Rotation.LookAt( normal ) );
			
			var decal = Game.Random.FromList( decalResource.Decals );

			var gameObject = Scene.CreateObject();
			gameObject.Transform.Position = pos;
			gameObject.Transform.Rotation = Rotation.LookAt( -normal );

			// what the fuck? did I fuck something big time here
			gameObject.Transform.Rotation *= Rotation.FromAxis( Vector3.Left, -90f );

			// Random rotation
			gameObject.Transform.Rotation *= Rotation.FromAxis( Vector3.Up, decal.Rotation.GetValue() );

			var decalRenderer = gameObject.Components.Create<DecalRenderer>();
			decalRenderer.Material = decal.Material;
			decalRenderer.Size = new( decal.Width.GetValue(), decal.Height.GetValue(), decal.Depth.GetValue() );

			// Creates a destruction component to destroy the gameobject after a while
			gameObject.DestroyAsync( 30f );
		}

		if ( !string.IsNullOrEmpty( surface.Sounds.Bullet ) )
		{
			hitObject.PlaySound( surface.Sounds.Bullet );
		}
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

		int count = 0;

		foreach ( var tr in GetShootTrace() )
		{
			if ( !tr.Hit )
			{
				DoShootEffects();
				return;
			}

			DoShootEffects();
			CreateImpactEffects( tr.GameObject, GetSurfaceFromTrace( tr ), tr.EndPosition, tr.Normal );
			DoTracer( tr.StartPosition, tr.EndPosition, tr.Distance, count );

			// Inflict damage on whatever we find.
			var damageInfo = DamageInfo.Bullet( BaseDamage, Weapon.PlayerController.GameObject, Weapon.GameObject );
			tr.GameObject.TakeDamage( ref damageInfo );

			Log.Trace( $"ShootWeaponAbility: We hit {tr.GameObject}!" );
			count++;
		}
	}

	protected void DoTracer( Vector3 startPosition, Vector3 endPosition, float distance, int count )
	{
		var effectPath = "assets/particles/gameplay/guns/trail/trail_smoke.vpcf";

		// For when we have bullet penetration implemented.
		if ( count > 0 )
		{
			effectPath = "assets/particles/gameplay/guns/trail/rico_trail_smoke.vpcf";

			// Project backward
			Vector3 dir = (startPosition - endPosition).Normal;
			var tr = Scene.Trace.Ray( endPosition, startPosition + (dir * 50f) )
				.Radius( 1f )
				.WithoutTags( "weapon" )
				.Run();

			if ( tr.Hit )
			{
				CreateImpactEffects( tr.GameObject, GetSurfaceFromTrace( tr ), tr.StartPosition, dir );
			}
		}

		var origin = count == 0 ? EffectsRenderer.GetAttachment( "muzzle" )?.Position ?? startPosition : startPosition;

		// What in tarnation is this 
		CreateParticleSystem( effectPath, startPosition, Rotation.Identity, new()
		{
			new() { StringCP = "0", Value = ParticleControlPoint.ControlPointValueInput.Vector3, VectorValue = origin },
			new() { StringCP = "1", Value = ParticleControlPoint.ControlPointValueInput.Vector3, VectorValue = endPosition },
			new() { StringCP = "2", Value = ParticleControlPoint.ControlPointValueInput.Float, FloatValue = distance }
		}, 50f );
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
	protected virtual IEnumerable<SceneTraceResult> GetShootTrace()
	{
		var tr = Scene.Trace.Ray( WeaponRay, MaxRange )
			.Size( BulletSize )
			.WithAnyTags( "solid" )
			.UseHitboxes()
			.Run();

		yield return tr;
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

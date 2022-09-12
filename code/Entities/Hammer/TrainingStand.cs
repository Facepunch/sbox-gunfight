using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Facepunch.Gunfight.Entities.Hammer
{
	[EditorModel( "models/training/dummyeditor.vmdl" )]
	[Library( "GF_TrainingStand" )]
	[HammerEntity]
	[VisGroup( VisGroup.Dynamic )]
//	[Model( Model = "models/training/dummy.vmdl" )]
	[Title( "Training Stand" ), Category( "Gameplay" ), Icon( "brush" )]
	public partial class TrainingStand : AnimatedEntity
	{
		/// <summary>
		/// Name of Dummy.
		/// </summary>
		[Net, Property]
		public string DummyName { get; set; } = "Training Dummy";

		[Net] public TimeSince TimeSinceKilled { get; set; } 
		public override void Spawn()
		{
			base.Spawn();
			SetModel( "models/training/dummy.vmdl" );
			CreateHull();
			SetupPhysicsFromModel( PhysicsMotionType.Keyframed );
			Tags.Add( "player" );
		}
		public virtual void CreateHull()
		{
			UsePhysicsCollision = true;
			EnableAllCollisions = true;
			SetupPhysicsFromModel( PhysicsMotionType.Dynamic );
			EnableHitboxes = true;
			EnableAllCollisions = true;
			SurroundingBoundsMode = SurroundingBoundsType.Physics;
		}

		[Net, Predicted] public TimeSince TimeSinceDamage { get; set; }
		public long FakeId { get; private set; }
		
		[Net]
		public bool IsDead { get; private set; }

		public override void TakeDamage( DamageInfo info )
		{
			if ( LifeState == LifeState.Dead )
				return;

			LastDamage = info;
			Log.Info( GetHitboxBone( LastDamage.HitboxIndex ) );
			// Headshot
			var isHeadshot = GetHitboxBone( info.HitboxIndex ) == 1;
			if ( isHeadshot )
			{
				info.Damage *= 2.0f;
			}

			LastAttacker = info.Attacker;
			LastAttackerWeapon = info.Weapon;
			var attackerGun = info.Weapon as GunfightWeapon;

			if ( Health > 0 && info.Damage > 0 )
			{
				Health -= info.Damage;
				if ( Health <= 0 )
				{
					Health = 0;
					OnDeath();
					GunfightGame.Current?.OnKilledMessage( LastAttacker.Client.PlayerId, LastAttacker.Client.Name, FakeId, DummyName, attackerGun.Name );
					(this as AnimatedEntity)?.SetAnimParameter( "Hit", true );
				}
			}TimeSinceDamage = 0;

			
			if ( info.Attacker is GunfightPlayer attacker )
			{
				if ( attacker != Local.Pawn )
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
		public void TookDamage( Vector3 pos, bool headshot = false )
		{
			DamageIndicator.Current?.OnHit( pos, headshot );
		}

		DamageInfo LastDamage;
		public void OnDeath()
		{
		//	base.OnKilled();

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

			EnableAllCollisions = false;
			//EnableDrawing = false;

			//Respawn();

			foreach ( var child in Children.OfType<ModelEntity>() )
			{
				//child.EnableDrawing = false;
			}

	
		}

		[Event.Tick.Server]
		public void Respawn()
		{		
			if (Health > 0)
			{
				TimeSinceKilled = 0;
			}
			
			if ( TimeSinceKilled > 5 )
			{
				LifeState = LifeState.Alive;
				Health = 100;
				EnableAllCollisions = true;
				EnableDrawing = true;
				(this as AnimatedEntity)?.SetAnimParameter( "Hit", false );
				foreach ( var child in Children.OfType<ModelEntity>() )
				{
					child.EnableDrawing = true;
				}
			}
		}
	}
}

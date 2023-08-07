using Sandbox.Utility;

namespace Facepunch.Gunfight;

public partial class VaultMoveMechanic : BaseMoveMechanic
{
	public float MaxVaultHeight => 50f;
	public float MinimumHeight => 20f;

	public override bool TakesOverControl => true;

	private TimeSince timeSinceVault;
	private Vector3 vaultEnd;

	public VaultMoveMechanic() { }
	public VaultMoveMechanic( PlayerController ctrl ) : base( ctrl ) { }

	public override bool IsDebugging => true;

	private float LastWallHeight = 0f;
	
	public bool CanActivate( bool assignValues = false )
	{
		if ( Controller.Climb.CanActivate( false ) ) return false;

		var wall = GetWallInfo( Controller.Rotation.Forward, MaxVaultHeight, 30f, 32 );

		if ( wall == null ) return false;
		if ( wall.Height == 0 ) return false;
		if ( wall.Height < MinimumHeight ) return false;
		if ( wall.Distance > Controller.BodyGirth * 1 ) return false;
		
		if ( Vector3.Dot( Controller.WishVelocity.Normal, wall.Normal ) > 0.0f ) return false;

		var posFwd = Controller.Position - wall.Normal * Controller.BodyGirth * 2f;
		var floorTraceStart = posFwd.WithZ( wall.AbsoluteHeight );
		var floorTrace = Controller.TraceBBox( floorTraceStart, posFwd );
		
		//DebugOverlay.TraceResult( floorTrace );
		
		//DebugOverlay.Sphere( floorTrace.EndPosition, 16f, Color.Green );

		if ( floorTrace.StartedSolid ) return false;
		
		vaultEnd = floorTrace.EndPosition;

		if ( assignValues )
		{
			timeSinceVault = 0;
			LastWallHeight = wall.Height;
			VaultDone = VaultTime;
		}

		if ( IsStuck( vaultEnd ) )
		{
			return false;
		}

		return true;
	}

	protected override bool TryActivate()
	{
		if ( !Input.Down( "Jump" ) ) return false;

		if ( !CanActivate( true ) )
			return false;

		Controller.Pawn.PlaySound( "sounds/footsteps/footstep-concrete-jump.sound" );

		return true;
	}

	protected bool CloseEnough()
	{
		return VaultDone.Fraction >= 1f;
	}

	public float Frac => VaultDone.Fraction;
	
	public TimeUntil VaultDone { get; set; }

	float GetZOffset()
	{
		var x = Frac;
		var val = 0.5f * MathF.Sin( MathF.PI * x ) + 0.5f;

		return val - 0.5f;
	}

	private float VaultTime => 0.5f;
	private float ZHeightOffset => LastWallHeight * 0.5f;

	private void DestroyGlassNearby()
	{
		using ( Entity.LagCompensation() )
		{
			var pos = Player.AimRay.Position + Vector3.Down * 10f;
			var tr = Trace.Ray( pos, pos + Player.AimRay.Forward * 100f ).WithoutTags( "player" ).Run();

			if ( tr.Hit && tr.Entity is GlassShard )
			{
//				DebugOverlay.Sphere( tr.HitPosition, 32, Game.IsServer ? Color.Blue : Color.Yellow,  1f );
				tr.Entity.TakeDamage( DamageInfo.FromBullet( tr.HitPosition, Player.AimRay.Forward * 100f, 20f ) );
			}
		}
	}
	
	public Vector3 GetNextStepPos()
	{
		var offset = Easing.ExpoOut( GetZOffset() );

		return Controller.Position.LerpTo( vaultEnd.WithZ( vaultEnd.z + ( offset * ZHeightOffset ) ), Frac );
	}

	protected bool IsStuck( Vector3 testpos )
	{
		var result = Controller.TraceBBox( testpos, testpos, 1f );
		
		return result.StartedSolid;
	}

	protected void Stop()
	{
		IsActive = false;
	}

	public override void Simulate()
	{
		base.Simulate();

		if ( !CloseEnough() )
		{
			var nextPos = GetNextStepPos();
			Controller.Position = nextPos;
			DestroyGlassNearby();
			Controller.SetTag( "ducked" );
			return;
		}
		
		Stop();
	}
}

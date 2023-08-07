namespace Facepunch.Gunfight;

public partial class ClimbMechanic : BaseMoveMechanic
{
	public float MinVaultHeight => 30f;
	public float MaxVaultHeight => 80f;

	public override bool TakesOverControl => true;

	private TimeSince timeSinceVault;
	private Vector3 vaultEnd;

	public ClimbMechanic() { }
	public ClimbMechanic( PlayerController ctrl ) : base( ctrl ) { }

	public bool CanActivate( bool assignValues = false )
	{
		var wall = GetWallInfo( Controller.Rotation.Forward );

		if ( wall == null ) return false;
		if ( wall.Height == 0 ) return false;
		if ( wall.Distance > Controller.BodyGirth * 1 ) return false;
		
		if ( Vector3.Dot( Controller.WishVelocity.Normal, wall.Normal ) > 0.0f ) return false;

		var posFwd = Controller.Position - wall.Normal * (Controller.BodyGirth + wall.Distance);
		var floorTraceStart = posFwd.WithZ( wall.AbsoluteHeight );
		var floorTraceEnd = posFwd.WithZ( Controller.Position.z );

		var floorTrace = Controller.TraceBBox( floorTraceStart, floorTraceEnd );
		if ( !floorTrace.Hit ) return false;
		if ( floorTrace.StartedSolid ) return false;

		var vaultHeight = floorTrace.EndPosition.z - Controller.Position.z;
		if ( vaultHeight < MinVaultHeight ) return false;
		if ( vaultHeight > MaxVaultHeight ) return false;

		if ( assignValues )
		{
			timeSinceVault = 0;
			vaultEnd = floorTrace.EndPosition.WithZ( floorTrace.EndPosition.z + 6.8f );
			Controller.Velocity = Controller.Velocity.WithZ( 0 );
		}

		Vector3 vaultTop = Controller.Position.WithZ( vaultEnd.z );
		if ( IsStuck( vaultEnd ) || IsStuck( vaultTop ) ) return false;

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
		if ( Controller.Position.Distance( vaultEnd ) < 10f )
		{
			return true;
		}
		return false;
	}

	protected bool ReachedZ()
	{
		return vaultEnd.z.AlmostEqual( Controller.Position.z, 10f );
	}

	protected bool IsStuck( Vector3 testpos )
	{
		var result = Controller.TraceBBox( testpos, testpos );
		return result.StartedSolid;
	}

	protected void Stop()
	{
		IsActive = false;
		_ = new ScreenShake.Pitch( 0.2f, 1f );
	}

	public Vector3 GetNextStepPos()
	{
		Controller.Velocity = Controller.Velocity.WithZ( 0 ); // Null gravity

		if ( !ReachedZ() )
			return Controller.Position.LerpTo( Controller.Position.WithZ( vaultEnd.z ), Time.Delta * 7f );

		return Controller.Position.LerpTo( vaultEnd, Time.Delta * 10f );
	}

	public override void Simulate()
	{
		base.Simulate();

		if ( timeSinceVault > 1f )
			Stop();

		if ( !CloseEnough() )
		{
			var nextPos = GetNextStepPos();

			Controller.Position = nextPos;
			Controller.Velocity = Vector3.Zero;
			Controller.SetTag( "ducked" );

			return;
		}

		Stop();
	}

}

namespace Facepunch.Gunfight;

public partial class VaultMoveMechanic : BaseMoveMechanic
{
	public float MinVaultHeight => 30f;
	public float MaxVaultHeight => 80f;

	public override bool TakesOverControl => true;

	private TimeSince timeSinceVault;
	private Vector3 vaultEnd;

	public VaultMoveMechanic() { }
	public VaultMoveMechanic( PlayerController ctrl ) : base( ctrl ) { }

	public bool CanActivate( bool assignValues = false )
	{
		var wall = GetWallInfo( Controller.Rotation.Forward );

		if ( wall == null ) return false;
		if ( wall.Height == 0 ) return false;
		if ( wall.Distance > Controller.BodyGirth * 1 ) return false;
		if ( Vector3.Dot( Controller.EyeRotation.Forward, wall.Normal ) > -.5f ) return false;

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
			vaultEnd = floorTrace.EndPosition.WithZ( floorTrace.EndPosition.z + 10f );
			Controller.Velocity = Controller.Velocity.WithZ( 0 );
		}

		return true;
	}

	protected override bool TryActivate()
	{
		if ( !Input.Pressed( InputButton.Jump ) ) return false;

		return CanActivate( true );
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
		if ( !ReachedZ() )
			return Controller.Position.LerpTo( Controller.Position.WithZ( vaultEnd.z ), Time.Delta * 10f );

		return Controller.Position.LerpTo( vaultEnd, Time.Delta * 7f );
	}

	public override void Simulate()
	{
		base.Simulate();

		if ( timeSinceVault > 1f )
			Stop();

		if ( !CloseEnough() )
		{
			var nextPos = GetNextStepPos();
			if ( IsStuck( nextPos ) )
			{
				Stop();
				return;
			}

			Controller.Position = nextPos;
			Controller.Velocity = Vector3.Zero;
			Controller.SetTag( "ducked" );

			return;
		}

		Stop();
	}

}

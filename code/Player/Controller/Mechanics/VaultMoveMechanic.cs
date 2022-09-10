namespace Facepunch.Gunfight;

public partial class VaultMoveMechanic : BaseMoveMechanic
{
	public float MinVaultHeight => 30f;
	public float MaxVaultHeight => 80f;
	public float MinVaultTime => .1f;
	public float MaxVaultTime => .25f;
	public float ClimbVaultMultiplier => 1.5f;

	public override bool TakesOverControl => true;

	private bool vaultingFromGround;
	private float vaultHeight;
	private TimeSince timeSinceVault;
	private Vector3 vaultStart, vaultEnd;
	private WallInfo wall;

	public VaultMoveMechanic() { }
	public VaultMoveMechanic( PlayerController ctrl ) : base( ctrl ) { }

	public bool CanActivate( bool assignValues = false )
	{
		var wall = GetWallInfo( Controller.Rotation.Forward );

		if ( wall == null ) return false;
		if ( wall.Value.Height == 0 ) return false;
		if ( wall.Value.Distance > Controller.BodyGirth * 2 ) return false;
		if ( Vector3.Dot( Controller.EyeRotation.Forward, wall.Value.Normal ) > -.5f ) return false;

		var posFwd = Controller.Position - wall.Value.Normal * (Controller.BodyGirth + wall.Value.Distance);
		var floorTraceStart = posFwd.WithZ( wall.Value.Height );
		var floorTraceEnd = posFwd.WithZ( Controller.Position.z );

		var floorTrace = Controller.TraceBBox( floorTraceStart, floorTraceEnd );
		if ( !floorTrace.Hit ) return false;
		if ( floorTrace.StartedSolid ) return false;

		var vaultHeight = floorTrace.EndPosition.z - Controller.Position.z;
		if ( vaultHeight < MinVaultHeight ) return false;
		if ( vaultHeight > MaxVaultHeight ) return false;

		if ( assignValues )
		{
			this.wall = wall.Value;
			this.vaultHeight = vaultHeight;
			vaultingFromGround = Controller.GroundEntity != null;
			timeSinceVault = 0;
			vaultStart = Controller.Position;
			vaultEnd = Controller.Position.WithZ( floorTrace.EndPosition.z + 10 ) + Controller.Rotation.Forward * Controller.BodyGirth;
			Controller.Velocity = Controller.Velocity.WithZ( 0 );
		}

		return true;
	}

	protected override bool TryActivate()
	{
		if ( !Input.Down( InputButton.Jump ) ) return false;

		return CanActivate( true );
	}


	bool reachedVertically = false;
	public override void Simulate()
	{
		base.Simulate();

		var vaultTime = MinVaultTime.LerpTo( MaxVaultTime, vaultHeight / MaxVaultHeight );

		if ( !vaultingFromGround )
		{
			vaultTime *= ClimbVaultMultiplier;
		}

		if ( timeSinceVault <= vaultTime + Time.Delta )
		{
			var a = timeSinceVault / vaultTime;
			reachedVertically = Controller.Position.z.AlmostEqual( vaultEnd.z, 1 );

			if ( reachedVertically )
				Controller.Position = Vector3.Lerp( vaultStart, vaultEnd, a, false );
			else
				Controller.Position = Vector3.Lerp( vaultStart, vaultStart.WithZ(vaultEnd.z), a, false );


			Controller.Velocity = Controller.Velocity.WithZ( 0 );
			return;
		}

		IsActive = false;
	}

}

namespace Facepunch.Gunfight;

public partial class CoverAimMechanic : BaseMoveMechanic
{
	public CoverAimMechanic() { }
	public CoverAimMechanic( PlayerController ctrl ) : base( ctrl ) { }

	public override bool TakesOverControl => true;
	public float MaxWallMountDistance => 10f;

	protected float WallHeight { get; set; }
	protected bool Wish { get; set; }

	private WallInfo CachedWallInfo;

	public Vector3 MountWorldPosition { get; set; }
	public bool CanMountWall()
	{
		var wall = GetWallInfo( Controller.Rotation.Forward );

		CachedWallInfo = wall;

		if ( wall == null ) return false;
		if ( wall.Distance > MaxWallMountDistance ) return false;

		MountWorldPosition = wall.TracePos;
		return wall.Height <= 65f && wall.Height >= 30f;
	}

	protected void DoVisualEffects( bool inverted = false )
	{
		if ( CachedWallInfo == null ) return;

		var size = (CachedWallInfo.Height > 60f ? 1 : -1);
		if ( inverted ) size *= -1;

		_ = new ScreenShake.Pitch( .2f, size );
	}

	protected override bool TryActivate()
	{
		Wish = Input.Pressed( "Mount" );

		if ( !Wish ) return false;
		if ( !CanMountWall() ) return false;
		if ( Controller.Slide.IsActive ) return false;
		if ( Controller.IsSprinting ) return false;

		TimeSinceActivate = 0;

		DoVisualEffects();

		return true;
	}

	public override void PreSimulate()
	{
		bool shouldStop = false;
		if ( !Player.MoveInput.x.AlmostEqual( 0f ) || !Player.MoveInput.y.AlmostEqual( 0f ) || Input.Pressed( "Jump" ) )
			shouldStop = true;

		if ( Vector3.Dot( Player.AimRay.Forward.Normal, CachedWallInfo.Normal ) > - 0.5f )
			shouldStop = true;

		if ( shouldStop ) 
			StopTry();
	}

	public override void StopTry()
	{
		base.StopTry();
		DoVisualEffects( true );
	}

	public override void Simulate()
	{
		if ( !CanMountWall() ) return;

		var wall = CachedWallInfo;
		var wallHeight = wall.Height + 4f;

		WallHeight = wallHeight;

		// Duck the player
		if ( WallHeight < 50f )
		{
			Controller.SetTag( "ducked" );
		}
	}

	public override float? GetEyeHeight()
	{
		if ( WallHeight == 0f ) return null;

		return WallHeight + 16f;
	}
}

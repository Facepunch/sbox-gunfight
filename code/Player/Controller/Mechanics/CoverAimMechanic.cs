namespace Facepunch.Gunfight;

public partial class CoverAimMechanic : BaseMoveMechanic
{
	public CoverAimMechanic() { }
	public CoverAimMechanic( PlayerController ctrl ) : base( ctrl ) { }

	public override bool TakesOverControl => true;
	public float MaxWallMountDistance => 5f;

	protected float WallHeight { get; set; }
	protected bool Wish { get; set; }

	private WallInfo CachedWallInfo;

	public bool CanMountWall()
	{
		var wall = GetWallInfo( Controller.Rotation.Forward );

		CachedWallInfo = wall;

		if ( wall == null ) return false;
		if ( wall.Distance > MaxWallMountDistance ) return false;

		return wall.Height <= 80f && wall.Height >= 40f;
	}

	protected void DoVisualEffects( bool inverted = false )
	{

		var size = (CachedWallInfo.Height > 60f ? 1 : -1);
		if ( inverted ) size *= -1;

		_ = new ScreenShake.Pitch( .2f, size );
	}

	protected override bool TryActivate()
	{
		Wish = Input.Down( InputButton.SecondaryAttack );

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
		if ( !Input.Down( InputButton.SecondaryAttack ) ) StopTry();
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
		var wallHeight = wall.Height;

		WallHeight = wallHeight - 5f;
	}

	public override float? GetEyeHeight()
	{
		if ( WallHeight == 0f ) return null;

		return WallHeight;
	}
}

namespace Facepunch.Gunfight;

public partial class SlideMechanic : BaseMoveMechanic
{
	protected bool Wish { get; set; }

	public float BoostTime => 1f;
	// You can only slide once every X
	public float Cooldown => 1f;
	public float MinimumSpeed => 120f;
	public float WishDirectionFactor => 1200f;
	public float SlideIntensity => 1 + (TimeSinceActivate / BoostTime);
	public float SlideSpeed => 750.0f;

	public SlideMechanic() { }
	public SlideMechanic( PlayerController ctrl ) : base( ctrl ) { }

	protected bool ShouldSlide()
	{
		if ( Controller.GroundEntity == null ) return false;
		if ( Controller.Velocity.Length <= MinimumSpeed ) return false;

		return true;
	}

	protected override bool TryActivate()
	{
		Wish = Input.Down( InputButton.Duck );

		if ( Controller.Velocity.Length < Controller.DefaultSpeed ) return false;
		if ( !Wish ) return false;
		if ( !ShouldSlide() ) return false;
		if ( TimeSinceActivate < Cooldown ) return false;

		TimeSinceActivate = 0;

		// Give it an initial boost
		var slopeForward = new Vector3( Controller.Velocity.x, Controller.Velocity.y, 0 ).Normal;
		if( Controller.Velocity.Length < 300.0f )
			Controller.Velocity += slopeForward * 50.0f;

		return true;
	}

	public override void PreSimulate()
	{
		if ( !ShouldSlide() ) StopTry();
	}

	public override void Simulate()
	{
		var hitNormal = Controller.GroundNormal;

		var slopeDir = Vector3.Cross( Vector3.Up, Vector3.Cross( Vector3.Up, Controller.GroundNormal ) );
		var dot = Vector3.Dot( Controller.Velocity.Normal, slopeDir );
		var slopeForward = new Vector3( hitNormal.x, hitNormal.y, 0 );

		if( Controller.Velocity.Length < SlideSpeed )
			Controller.Velocity += slopeForward * Time.Delta * SlideSpeed;

		Controller.SetTag( "sliding" );
	}

	public override float? GetEyeHeight()
	{
		return 30f;
	}

	public override float? GetGroundFriction()
	{
		return 0.7f;
	}
}

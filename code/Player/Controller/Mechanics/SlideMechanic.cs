namespace Facepunch.Gunfight;

public partial class SlideMechanic : BaseMoveMechanic
{
	protected bool Wish { get; set; }

	public float MinimumSpeed => 55f;
	public float SlideSpeed => 850.0f;
	public SlideMechanic() { }
	public SlideMechanic( PlayerController ctrl ) : base( ctrl ) 
	{
	}

	public TimeSince TimeSinceStopped;

	protected bool ShouldSlide()
	{
		if ( Controller.GroundEntity == null ) return false;
		if ( TimeSinceStopped < 1f ) return false;

		return true;
	}

	protected override bool TryActivate()
	{
		Wish = Input.Down( "Slide" ) || Input.Down( "Duck" );

		if ( Controller.Velocity.Length < Controller.DefaultSpeed ) return false;
		if ( !Wish ) return false;
		if ( !ShouldSlide() ) return false;

		// Give it an initial boost
		var slopeForward = new Vector3( Controller.Velocity.x, Controller.Velocity.y, 0 ).Normal;
		
		Controller.Velocity += slopeForward * 300.0f;

		Controller.Pawn.PlaySound( "sounds/player/foley/slide/ski.stop.sound" );

		return true;
	}

	public override void StopTry()
	{
		base.StopTry();

		TimeSinceStopped = 0;
	}

	public override void PreSimulate()
	{
		if ( !ShouldSlide() ) StopTry();
		if ( TimeSinceActivate > 0.5f && ( Input.Pressed( "Slide" ) || Input.Pressed( "Duck" ) ) ) StopTry();
	}

	public override void Simulate()
	{
		var hitNormal = Controller.GroundNormal;

		var slopeDir = Vector3.Cross( Vector3.Up, Vector3.Cross( Vector3.Up, Controller.GroundNormal ) );
		var slopeForward = new Vector3( hitNormal.x, hitNormal.y, 0 );

		if ( Controller.Velocity.Length < SlideSpeed )
			Controller.Velocity += slopeForward * Time.Delta * SlideSpeed;

		Controller.SetTag( "sliding" );
		
		if ( TimeSinceActivate > 1f && Controller.Velocity.Length < 250f )
		{
			StopTry();
		}
	}

	public override float? GetEyeHeight()
	{
		return 20f;
	}

	public override float? GetGroundFriction()
	{
		// Slide further if we are aiming down sights
		return Controller.IsAiming ? 0.5f : 0.7f;
	}
}

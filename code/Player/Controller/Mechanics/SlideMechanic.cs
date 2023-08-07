namespace Facepunch.Gunfight;

public partial class SlideMechanic : BaseMoveMechanic
{
	protected bool Wish { get; set; }

	public float MinimumSpeed => 155f;
	public float SlideSpeed => 750.0f;
	private Sound SlideSound;

	public SlideMechanic() { }
	public SlideMechanic( PlayerController ctrl ) : base( ctrl ) 
	{
	}

	protected bool ShouldSlide()
	{
		if ( Controller.GroundEntity == null ) return false;
		if ( Controller.Velocity.Length <= MinimumSpeed ) return false;

		return true;
	}

	protected override bool TryActivate()
	{
		Wish = Input.Down( "Slide" );

		if ( Controller.Velocity.Length < Controller.DefaultSpeed ) return false;
		if ( !Wish ) return false;
		if ( !ShouldSlide() ) return false;

		// Give it an initial boost
		var slopeForward = new Vector3( Controller.Velocity.x, Controller.Velocity.y, 0 ).Normal;
		if( Controller.Velocity.Length < 300.0f )
			Controller.Velocity += slopeForward * 200.0f;

		Controller.Pawn.PlaySound( "sounds/player/foley/slide/ski.stop.sound" );
		SlideSound = Controller.Pawn.PlaySound( "sounds/player/foley/slide/ski.loop.sound");
		SlideSound.SetVolume( 2.0f );

		return true;
	}

	public override void StopTry()
	{
		SlideSound.Stop();
		base.StopTry();
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

		// Doesn't work?
		SlideSound.SetPitch( Controller.Velocity.Length / Controller.SprintSpeed );

		Controller.SetTag( "sliding" );
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

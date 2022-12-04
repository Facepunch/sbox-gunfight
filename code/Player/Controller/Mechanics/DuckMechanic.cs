namespace Facepunch.Gunfight;

public partial class DuckMechanic : BaseMoveMechanic
{
	public bool Wish { get; set; }

	public override bool TakesOverControl => true;

	public DuckMechanic() { }
	public DuckMechanic( PlayerController ctrl ) : base( ctrl ) { }

	protected override bool TryActivate()
	{
		Wish = Input.Down( InputButton.Duck );

		if ( !Wish )
		{
			var test = Controller.TraceBBox( Controller.Position, Controller.Position + Vector3.Up * 10f );
			if ( test.Hit )
			{
				return true;
			}
			return false;
		}

		if ( Controller.Slide.IsActive ) return false;

		TimeSinceActivate = 0;

		return true;
	}

	public override void PreSimulate()
	{
		if ( !Input.Down( InputButton.Duck ) ) StopTry();
	}

	public override float GetWishSpeed()
	{
		return 100f;
	}

	public override float? GetEyeHeight()
	{
		return 32f;
	}

	public override void Simulate()
	{
		Controller.SetTag( "ducked" );
	}
}

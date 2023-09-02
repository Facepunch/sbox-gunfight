namespace Facepunch.Gunfight;

public partial class NoclipMechanic : BaseMoveMechanic
{
	public bool Wish { get; set; }

	public override bool TakesOverControl => true;

	public NoclipMechanic() { }
	public NoclipMechanic( PlayerController ctrl ) : base( ctrl ) { }

	protected override bool TryActivate()
	{
		Wish = Input.Down( "noclip" );
		if ( !Wish ) return false;

		TimeSinceActivate = 0;

		return true;
	}

	public override void PreSimulate()
	{
		if ( !Input.Down( "noclip" ) ) StopTry();
	}

	public override void Simulate()
	{
		Controller.SetTag( "noclip" );

		var pl = Player;

		var fwd = pl.MoveInput.x.Clamp( -1f, 1f );
		var left = pl.MoveInput.y.Clamp( -1f, 1f );
		var rotation = pl.EyeRotation;

		var vel = (rotation.Forward * fwd) + (rotation.Left * left);

		if ( Input.Down( "jump" ) )
		{
			vel += Vector3.Up * 1;
		}

		vel = vel.Normal * 2000;

		if ( Input.Down( "run" ) )
			vel *= 5.0f;

		if ( Input.Down( "duck" ) )
			vel *= 0.2f;

		Controller.Velocity += vel * Time.Delta;

		if ( Controller.Velocity.LengthSquared > 0.01f )
		{
			Controller.Position += Controller.Velocity * Time.Delta;
		}

		Controller.Velocity = Controller.Velocity.Approach( 0, Controller.Velocity.Length * Time.Delta * 5.0f );

		Player.EyeRotation = rotation;
		Controller.WishVelocity = Controller.Velocity;
		Controller.GroundEntity = null;
		Controller.BaseVelocity = Vector3.Zero;
	}
}

namespace Facepunch.Gunfight;

public partial class SlideMechanic : BaseMoveMechanic
{
	protected bool Wish { get; set; }

	public float BoostTime => 1f;
	// You can only slide once every X
	public float Cooldown => 1f;
	public float MinimumSpeed => 64f;
	public float WishDirectionFactor => 1200f;
	public float SlideIntensity => 1 - (TimeSinceActivate / BoostTime);

	public SlideMechanic() { }
	public SlideMechanic( PlayerController ctrl ) : base( ctrl ) { }

	protected override void OnActiveChanged( bool before, bool after )
	{
		if ( Host.IsServer ) return;
		if ( Controller == null ) return;

		if ( after )
			StartSliding();
		else
			StopSliding();
	}

	public Particles SlidingParticles { get; set; }
	public Sound SlidingSound { get; set; }

	protected void StartSliding()
	{
		Host.AssertClient();

		var player = Controller.Pawn as GunfightPlayer;
		player.StartSlidingEffects();
	}

	protected void StopSliding()
	{
		Host.AssertClient();

		var player = Controller.Pawn as GunfightPlayer;
		player.StopSlidingEffects();
	}

	protected bool ShouldSlide()
	{
		if ( Controller.GroundEntity == null ) return false;
		if ( Controller.Velocity.Length <= MinimumSpeed ) return false;

		return true;
	}

	protected override bool TryActivate()
	{
		Wish = Input.Down( InputButton.Duck );

		if ( !Wish ) return false;
		if ( !ShouldSlide() ) return false;
		if ( TimeSinceActivate < Cooldown ) return false;

		TimeSinceActivate = 0;

		return true;
	}

	public override void PreSimulate()
	{
		if ( !Input.Down( InputButton.Duck ) || !ShouldSlide() ) StopTry();
	}

	public override void Simulate()
	{
		var hitNormal = Controller.GroundNormal;
		var speedMult = Vector3.Dot( Controller.Velocity.Normal, Vector3.Cross( Controller.Rotation.Up, hitNormal ) );

		if ( BoostTime > TimeSinceActivate )
			speedMult -= 1 - (TimeSinceActivate / BoostTime);

		var slopeDir = Vector3.Cross( Vector3.Up, Vector3.Cross( Vector3.Up, Controller.GroundNormal ) );
		var dot = Vector3.Dot( Controller.Velocity.Normal, slopeDir );
		var slopeForward = Vector3.Cross( Controller.GroundNormal, Controller.Pawn.Rotation.Right );
		var spdGain = 4000f;

		if ( dot > 0.15f )
			spdGain *= 0.8f * SlideIntensity;
		else if ( dot < -0.15f )
		{
			spdGain *= 1.2f;
			TimeSinceActivate = 0;
		}
		else
			spdGain *= SlideIntensity;

		Controller.Velocity += spdGain * slopeForward * Time.Delta;

		var map = spdGain.Remap( 0, 3000f, 0, 1 );
		
		_ = new ScreenShake.Perlin( 0.3f, 0.1f, 0.2f * map );

		Controller.SetTag( "sliding" );
	}

	public override float? GetEyeHeight()
	{
		return 50f;
	}
}

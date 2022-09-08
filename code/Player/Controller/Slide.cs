namespace Facepunch.Gunfight;

public partial class Slide : BaseNetworkable
{
	[Net, Change( "OnActiveChanged" )] public bool IsActive { get; set; }
	[Net, Predicted] public bool Wish { get; set; }
	[Net, Predicted] public float BoostTime { get; set; } = 1f;
	[Net, Predicted] public bool IsDown { get; set; }
	[Net, Predicted] TimeSince Activated { get; set; } = 0;

	public float TimeUntilStop => 0.8f;
	// You can only slide once every X
	public float Cooldown => 1f;
	public float MinimumSpeed => 64f;
	public float WishDirectionFactor => 1200f;
	public float SlideIntensity => 1 - (Activated / BoostTime);
	
	PlayerController Controller;

	public Slide()
	{
	}

	protected void OnActiveChanged( bool _, bool isSliding )
	{
		if ( isSliding )
			StartSliding( Controller );
		else
			StopSliding( Controller );
	}

	public void PreTick( PlayerController controller )
	{
		IsDown = Input.Down( InputButton.Duck );

		Controller = controller;

		var oldWish = Wish;
		Wish = IsDown;

		if ( controller.Velocity.Length <= MinimumSpeed )
		{
			StopTry( controller );
			return;
		}

		// No sliding while you're already in the sky
		if ( controller.GroundEntity == null )
			StopTry( controller );

		if ( oldWish == Wish )
			return;

		if ( IsDown != IsActive )
		{
			if ( IsDown ) Try( controller );
			else StopTry( controller );
		}

		if ( IsActive )
			controller.SetTag( "sliding" );
	}

	public Vector3 WishDirOnStart { get; set; }

	void Try( PlayerController controller )
	{
		if ( Activated < Cooldown )
			return;

		if ( controller.GroundEntity == null )
			return;

		if ( controller.Velocity.Length <= MinimumSpeed )
			return;

		var change = IsActive != true;

		IsActive = true;

		if ( change )
		{
			Activated = 0;
			WishDirOnStart = controller.WishVelocity.Normal;
		}
	}

	public Particles SlidingParticles { get; set; }
	public Sound SlidingSound { get; set; }

	protected void StartSliding( PlayerController ctrl )
	{
		Host.AssertClient();

		SlidingParticles?.Destroy( true );
		SlidingParticles = Particles.Create( "particles/gameplay/player/slide/slide.vpcf", ctrl.Pawn, true );

		SlidingSound = Sound.FromEntity( "sounds/player/foley/slide/ski.loop.sound", ctrl.Pawn );
	}

	protected void StopSliding( PlayerController ctrl )
	{
		Host.AssertClient();

		SlidingParticles?.Destroy( true );
		SlidingSound.Stop();
		Sound.FromEntity( "sounds/player/foley/slide/ski.stop.sound", ctrl.Pawn );
	}

	void StopTry( PlayerController controller )
	{
		if ( !IsActive )
			return;

		Activated = 0;
		IsActive = false;
	}

	public float GetWishSpeed()
	{
		if ( !IsActive ) return -1;
		return 64;
	}

	internal void Accelerate( BasePlayerController controller, ref Vector3 wishdir, ref float wishspeed, ref float speedLimit, ref float acceleration )
	{
		var ctrl = controller;

		var hitNormal = controller.GroundNormal;
		var speedMult = Vector3.Dot( controller.Velocity.Normal, Vector3.Cross( controller.Rotation.Up, hitNormal ) );

		wishdir = WishDirOnStart * (WishDirectionFactor * Time.Delta);

		if ( BoostTime > Activated )
			speedMult -= 1 - (Activated / BoostTime);

		var slopeDir = Vector3.Cross( Vector3.Up, Vector3.Cross( Vector3.Up, ctrl.GroundNormal ) );
		var dot = Vector3.Dot( ctrl.Velocity.Normal, slopeDir );
		var slopeForward = Vector3.Cross( ctrl.GroundNormal, ctrl.Pawn.Rotation.Right );
		var spdGain = 4000f;

		if ( dot > 0.15f )
			spdGain *= 0.8f * SlideIntensity;
		else if ( dot < -0.15f )
		{
			spdGain *= 1.2f;
			Activated = 0;
		}
		else
			spdGain *= SlideIntensity;

		ctrl.Velocity += spdGain * slopeForward * Time.Delta;

		var map = spdGain.Remap( 0, 3000f, 0, 1 );
		_ = new ScreenShake.Perlin( 0.3f, 0.1f, 0.2f * map );

		controller.SetTag( "sliding" );
	}

	public void UpdateBBox( ref Vector3 mins, ref Vector3 maxs, float scale )
	{
		if ( IsActive )
		{
			maxs = maxs.WithZ( 36 * scale );
		}
	}
}

namespace Gunfight;

/// <summary>
/// A basic sliding mechanic.
/// </summary>
public partial class SlideMechanic : BasePlayerControllerMechanic
{
	[Property] public float NextSlideCooldown { get; set; } = 0.5f;
	[Property] public float MinimumSlideLength { get; set; } = 1.0f;
	[Property] public float SlideFriction { get; set; } = 0.01f;
	[Property] public float EyeHeight { get; set; } = -20.0f;
	[Property] public float WishDirectionScale { get; set; } = 0.5f;
	[Property] public float SlideSpeed { get; set; } = 300.0f;
	[Property] public float SteepnessScale { get; set; } = 15.0f;

	public override bool ShouldBecomeActive()
	{
		if ( TimeSinceActiveChanged < NextSlideCooldown ) return false;
		return Input.Down( "Slide" );
	}

	public override bool ShouldBecomeInactive()
	{
		if ( IsActive ) 
		{
			return TimeSinceActiveChanged > MinimumSlideLength;
		}

		return base.ShouldBecomeInactive();
	}

	public override IEnumerable<string> GetTags()
	{
		yield return "slide";
		yield return "no_aiming";
	}

	public float GetSurfaceSteepness()
	{
		var tr = Scene.Trace.Ray( Transform.Position, Transform.Position + Vector3.Down * 128f ).Run();

		if ( tr.Hit )
			return 1 - tr.Normal.y;

		return 1;
	}

	public override void BuildWishInput( ref Vector3 wish ) => wish.y *= WishDirectionScale;
	public override float? GetSpeed() => SlideSpeed;
	public override float? GetEyeHeight() => EyeHeight;
	public override float? GetGroundFriction()
	{
		var steepness = GetSurfaceSteepness();
		var remappedSteepness = steepness.Remap( 0, 1, 0, SteepnessScale );
		var newFriction = SteepnessScale - remappedSteepness;
		return newFriction;
	}
	public override float? GetAcceleration() => 2;
}

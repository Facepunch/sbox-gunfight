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
	}

	public override float? GetEyeHeight() => EyeHeight;
	public override float? GetGroundFriction() => SlideFriction;
}

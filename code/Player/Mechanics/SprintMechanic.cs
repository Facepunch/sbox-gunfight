namespace Gunfight;

/// <summary>
/// A sprinting mechanic.
/// </summary>
public partial class SprintMechanic : BasePlayerControllerMechanic
{
	public override bool ShouldUpdateMechanic()
	{
		return Input.Down( "Run" ) && PlayerController.IsGrounded;
	}

	public override IEnumerable<string> GetTags()
	{
		yield return "sprint";
	}

	public override float? GetEyeHeight()
	{
		return -2.0f;
	}

	public override float? GetSpeed()
	{
		return 300.0f;
	}
}

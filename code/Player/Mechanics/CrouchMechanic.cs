namespace Gunfight;

public partial class CrouchMechanic : BasePlayerControllerMechanic
{
	public override bool ShouldUpdateMechanic()
	{
		return Input.Down( "Duck" ) && !PlayerController.IsRunning;
	}

	public override float? GetEyeHeight()
	{
		return -32.0f;
	}

	public override float? GetSpeed()
	{
		return 65.0f;
	}
}

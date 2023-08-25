
namespace Facepunch.Gunfight;

[Library( "usp_rmr" )]
public partial class USPRMRAttachment : BodygroupAttachment
{
	public override string ForWeapon => "usp";

	public override Dictionary<int, int> Bodygroups => new()
	{
		// USP Match
		{ 4, 2 }
	};

	/// <summary>
	/// Inherit bodygroups to the ViewModel
	/// </summary>
	public override bool InheritBodygroups => true;
}

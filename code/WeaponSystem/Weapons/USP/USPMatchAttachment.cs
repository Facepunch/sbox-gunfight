
namespace Facepunch.Gunfight;

[Library( "usp_match" )]
public partial class USPMatchAttachment : BodygroupAttachment
{
	public override string ForWeapon => "usp";

	public override Dictionary<int, int> Bodygroups => new()
	{
		// USP Match
		{ 2, 1 }
	};

	/// <summary>
	/// Inherit bodygroups to the ViewModel
	/// </summary>
	public override bool InheritBodygroups => true;
}

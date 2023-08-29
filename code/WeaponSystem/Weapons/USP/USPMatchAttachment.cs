
namespace Facepunch.Gunfight;

public partial class USPMatchAttachment : BodygroupAttachment
{
	public override string ForWeapon => "usp";
	public override string Identifier => "usp_match";
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

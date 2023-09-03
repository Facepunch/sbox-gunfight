
namespace Facepunch.Gunfight;

public partial class USPRMRAttachment : BodygroupAttachment
{
	public override string ForWeapon => "usp";
	public override string Identifier => "usp_rmr";
	public override string Category => "Optic";
	public override string Name => "RMR";

	public override int Priority => 5;

	public override Dictionary<int, int> Bodygroups => new()
	{
		// USP Match
		{ 4, 2 }
	};

	public override Dictionary<string, int> SceneModelBodygroups => new()
	{
		{ "sights", 2 }
	};

	/// <summary>
	/// Inherit bodygroups to the ViewModel
	/// </summary>
	public override bool InheritBodygroups => true;
}

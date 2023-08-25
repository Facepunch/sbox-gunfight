using System.ComponentModel.DataAnnotations;

namespace Facepunch.Gunfight;

[Library( "mp5_rail" )]
public partial class MP5RailAttachment : BodygroupAttachment
{
	public override string ForWeapon => "mp5";

	public override Dictionary<int, int> Bodygroups => new()
	{
		// Rail
		{ 2, 1 }
	};

	/// <summary>
	/// Inherit bodygroups to the ViewModel
	/// </summary>
	public override bool InheritBodygroups => true;
}

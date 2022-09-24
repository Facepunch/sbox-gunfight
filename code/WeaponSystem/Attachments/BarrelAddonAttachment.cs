namespace Facepunch.Gunfight;

/// <summary>
/// A barrel addon attachment, normally for stuff like Flashlights, and Laser Sights
/// </summary>
public partial class BarrelAddonAttachment : WeaponAttachment
{
	public override AttachmentType AttachmentType => AttachmentType.Barrel;
}

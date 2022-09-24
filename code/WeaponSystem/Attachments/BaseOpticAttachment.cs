namespace Facepunch.Gunfight;

/// <summary>
/// A basic optic attachment for worldspace sights that don't require screenspace effects.
/// </summary>
public partial class BasicOpticAttachment : WeaponAttachment
{
	public override AttachmentType AttachmentType => AttachmentType.Optic;
}

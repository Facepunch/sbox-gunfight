namespace Facepunch.Gunfight;

/// <summary>
/// An enum to define a weapon attachment's type.
/// Used to control where the attachment will be placed on a weapon.
/// </summary>
public enum AttachmentType
{
	Default,
	Barrel,
	Optic
}

public partial class AttachmentSystem
{
	/// <summary>
	/// Fetches the attachment name (not to be confused with a weapon attachment) on a model, based on a weapon attachment's type.
	/// </summary>
	/// <param name="type"></param>
	/// <returns></returns>
	public static string GetModelAttachment( AttachmentType type )
	{
		return type switch
		{
			AttachmentType.Barrel => "barrel",
			AttachmentType.Optic => "optic",
			_ => "root"
		};
	}
}

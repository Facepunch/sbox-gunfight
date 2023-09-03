namespace Facepunch.Gunfight.CreateAClass;

public class Weapon
{
	public string Name { get; set; } = "invalid_weapon";
	public List<string> Attachments { get; set; } = new();

	/// <summary>
	/// Set an attachment on/off
	/// </summary>
	/// <param name="attachment"></param>
	/// <param name="active"></param>
	public void SetAttachment( WeaponAttachment attachment, bool active )
	{
		if ( !active )
		{
			Attachments.Remove( attachment.Identifier );
		}
		else
		{
			Attachments.RemoveAll( x => WeaponAttachment.Get( x ).Category == attachment.Category );
			Attachments.Add( attachment.Identifier );
		}
	}

	/// <summary>
	/// Do we have an attachment?
	/// </summary>
	/// <param name="attachment"></param>
	/// <returns></returns>
	public bool HasAttachment( WeaponAttachment attachment )
	{
		return Attachments.Any( x => x == attachment.Identifier );
	}
}

public partial class CustomClass
{
	public Weapon PrimaryWeapon { get; set; } = new();
	public Weapon SecondaryWeapon { get; set; } = new();

	public CustomClass() { }

	public CustomClass( string primary, string secondary )
	{
		PrimaryWeapon = new Weapon { Name = primary };
		SecondaryWeapon = new Weapon { Name = secondary };
	}
}

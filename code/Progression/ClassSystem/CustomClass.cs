namespace Facepunch.Gunfight.CreateAClass;

public class Weapon
{
	public string Name { get; set; } = "invalid_weapon";
	public List<string> Attachments { get; set; } = new();
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

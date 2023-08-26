namespace Facepunch.Gunfight.CreateAClass;

public class Weapon
{
	public string Name { get; set; }
	public string[] Attachments { get; set; }
}

public partial class CustomClass
{
	public Weapon PrimaryWeapon { get; set; }
	public Weapon SecondaryWeapon { get; set; }
}

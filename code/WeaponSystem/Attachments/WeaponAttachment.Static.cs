namespace Facepunch.Gunfight;

public partial class WeaponAttachment
{
	public static HashSet<WeaponAttachment> All { get; set; } = new();

	public static WeaponAttachment Get( string identifier )
	{
		return All.FirstOrDefault( x => x.Identifier == identifier );
	}

	public static void Init()
	{
		foreach ( var type in TypeLibrary.GetTypes<WeaponAttachment>() )
		{
			var attachment = type.Create<WeaponAttachment>();
			All.Add( attachment );
		}
	}
}

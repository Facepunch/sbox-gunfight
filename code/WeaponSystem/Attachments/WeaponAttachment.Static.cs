namespace Facepunch.Gunfight;

public partial class WeaponAttachment
{
	public static HashSet<WeaponAttachment> All { get; set; }

	public static IEnumerable<WeaponAttachment> For( string weapon ) => All.Where( x => x.IsSupported( weapon ) && !string.IsNullOrEmpty( x.Identifier ) );

	public static WeaponAttachment Get( string identifier )
	{
		return All.FirstOrDefault( x => x.Identifier == identifier );
	}

	public static void Init()
	{
		All = new();

		foreach ( var type in TypeLibrary.GetTypes<WeaponAttachment>() )
		{
			var attachment = type.Create<WeaponAttachment>();
			All.Add( attachment );
		}
	}
}

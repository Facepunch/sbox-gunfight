namespace Facepunch.Gunfight.CreateAClass;


public partial class CustomClass
{
	const string PERSISTENCE_BUCKET = "progression.createaclass";

	/// <summary>
	/// Get a list of custom classes
	/// </summary>
	/// <returns></returns>
	public static Dictionary<string, CustomClass> Fetch()
	{
		return PersistenceSystem.Instance.GetAll<CustomClass>( PERSISTENCE_BUCKET );
	}

	/// <summary>
	/// A reference to the custom class list.
	/// </summary>
	public static Dictionary<string, CustomClass> All { get; set; } = Fetch() ?? new();

	/// <summary>
	/// Save a custom class to persistence and in memory.
	/// </summary>
	/// <param name="className"></param>
	/// <param name="newClass"></param>
	public static void SaveOne( string className, CustomClass newClass )
	{
		All[className] = newClass;
		PersistenceSystem.Instance.Set( PERSISTENCE_BUCKET, className, newClass );
	}
}

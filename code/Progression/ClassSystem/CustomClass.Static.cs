namespace Facepunch.Gunfight.CreateAClass;


public partial class CustomClass
{
	const string PERSISTENCE_BUCKET = "progression.createaclass";

	/// <summary>
	/// Get a list of custom classes
	/// </summary>
	/// <returns></returns>
	public static Dictionary<string, CustomClass> FetchCustomClasses()
	{
		return PersistenceSystem.Instance.GetAll<CustomClass>( PERSISTENCE_BUCKET );
	}

	/// <summary>
	/// A reference to the custom class list.
	/// </summary>
	public static Dictionary<string, CustomClass> CustomClasses { get; set; } = FetchCustomClasses() ?? new();

	/// <summary>
	/// Save a custom class to persistence and in memory.
	/// </summary>
	/// <param name="className"></param>
	/// <param name="newClass"></param>
	public static void SaveCustomClass( string className, CustomClass newClass )
	{
		CustomClasses[className] = newClass;
		PersistenceSystem.Instance.Set( PERSISTENCE_BUCKET, className, newClass );
	}
}

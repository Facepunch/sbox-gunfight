namespace Facepunch.Gunfight.CreateAClass;


public partial class CustomClass
{
	const string PERSISTENCE_BUCKET = "progression.createaclass";
	const string PERSISTENCE_BUCKET_PREFS = "progression.createaclass.preferences";

	/// <summary>
	/// Get a list of custom classes
	/// </summary>
	/// <returns></returns>
	public static Dictionary<string, CustomClass> Fetch()
	{
		return PersistenceSystem.Instance.GetAll<CustomClass>( PERSISTENCE_BUCKET );
	}

	/// <summary>
	/// Get the selected custom class name
	/// </summary>
	/// <returns></returns>
	public static string GetSelectedName()
	{
		var selected = PersistenceSystem.Instance.Get<string>( PERSISTENCE_BUCKET_PREFS, "_Selected", null );
		return selected;
	}

	/// <summary>
	/// Get the selected custom class that you'lls spawn with
	/// </summary>
	/// <returns></returns>
	public static CustomClass GetSelected()
	{
		var selected = GetSelectedName();
		if ( string.IsNullOrEmpty( selected ) ) return null;

		if ( All.TryGetValue( selected, out var customClass ) )
		{
			return customClass;
		}
		
		return null;
	}

	/// <summary>
	/// Set the selected custom class that you'll spawn with
	/// </summary>
	/// <param name="name"></param>
	public static void SetSelected( string name )
	{
		PersistenceSystem.Instance.Set( PERSISTENCE_BUCKET_PREFS, "_Selected", name );
	}

	/// <summary>
	/// A reference to the custom class list.
	/// </summary>
	[SkipHotload]
	public static Dictionary<string, CustomClass> All => Fetch() ?? new();

	/// <summary>
	/// Save a custom class to persistence and in memory.
	/// </summary>
	/// <param name="className"></param>
	/// <param name="newClass"></param>
	public static void SaveOne( string className, CustomClass newClass )
	{
		PersistenceSystem.Instance.Set( PERSISTENCE_BUCKET, className, newClass );
	}

	/// <summary>
	/// Delete a custom class
	/// </summary>
	/// <param name="className"></param>
	public static void Delete( string className )
	{
		PersistenceSystem.Instance.Remove( PERSISTENCE_BUCKET, className );
	}
}

namespace Facepunch.Gunfight;

public class PersistenceSystem
{
	private static PersistenceSystem _instance;
	public static PersistenceSystem Instance
	{
		get
		{
			if ( _instance is null )
				_instance = new();

			return _instance;
		}
	}

	protected IPersistenceSystem System = new JsonPersistenceSystem();

	public T Get<T>( string bucket, string name, T defValue = default ) => System.Get( bucket, name, defValue );
	public void Set( string bucket, string name, object value )
	{
		if ( value is null )
		{
			Remove( bucket, name );
			return;
		}

		System.Set( bucket, name, value );
	}
	public void Remove( string bucket, string name ) => System.Remove( bucket, name );
}

using System.Text.Json;

namespace Facepunch.Gunfight;

public class JsonPersistenceSystem : IPersistenceSystem
{
	public readonly Dictionary<string, Dictionary<string, object>> Data = new();
	protected string GetFilePath( string bucket ) => $"{bucket}.json";

	public Dictionary<string, T> GetAll<T>( string bucket )
	{
		var path = GetFilePath( bucket );

		if ( !FileSystem.Data.FileExists( path ) )
			return new();

		return FileSystem.Data.ReadJson<Dictionary<string, T>>( GetFilePath( bucket ), new() );
	}

	public virtual T Get<T>( string bucket, string name, T defValue = default( T ) )
	{
		var data = GetAll<T>( bucket );
		if ( !data.TryGetValue( name, out T value ) )
			return defValue;

		return (T)value;
	}

	public bool Set( string bucket, string name, object value )
	{
		var data = GetAll<object>( bucket );
		data[name] = value;

		FileSystem.Data.WriteJson( GetFilePath( bucket ), data );

		return true;
	}

	public bool Remove( string bucket, string name )
	{
		var data = GetAll<object>( bucket );
		data.Remove( name );

		FileSystem.Data.WriteJson( GetFilePath( bucket ), data );

		return true;
	}
}

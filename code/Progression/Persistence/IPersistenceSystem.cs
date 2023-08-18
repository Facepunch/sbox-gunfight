namespace Facepunch.Gunfight;

public interface IPersistenceSystem
{
	public Dictionary<string, T> GetAll<T>( string bucket );
	public T Get<T>( string bucket, string name, T defValue = default( T ) );
	public bool Set( string bucket, string name, object value );
	public bool Remove( string bucket, string name );
}

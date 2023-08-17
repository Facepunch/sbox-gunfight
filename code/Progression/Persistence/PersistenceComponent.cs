namespace Facepunch.Gunfight;

public partial class PersistenceComponent : EntityComponent
{
	public virtual string PersistenceBucket => "player";

	public T GetPersistent<T>( string name, T defValue = default( T ) ) => PersistenceSystem.Instance.Get( PersistenceBucket, name, defValue );
	public void SetPersistent( string name, object value ) => PersistenceSystem.Instance.Set( PersistenceBucket, name, value );
	public void RemovePersistent( string name ) => PersistenceSystem.Instance.Remove( PersistenceBucket, name );
}

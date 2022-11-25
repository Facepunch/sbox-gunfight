namespace Facepunch.Gunfight;

public interface ISpawnPoint
{
	public int GetSpawnPriority();
	public bool IsValidSpawn( GunfightPlayer player );
	public Transform? GetSpawnTransform();
}

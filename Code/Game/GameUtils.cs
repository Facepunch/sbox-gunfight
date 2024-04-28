namespace Gunfight;

public partial class GameUtils
{
	public static IEnumerable<SpawnPoint> GetSpawnPoints()
	{
		return Game.ActiveScene.GetAllComponents<SpawnPoint>();
	}
}

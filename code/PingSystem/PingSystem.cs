namespace Facepunch.Gunfight;

public enum PingType
{
	Generic,
	Enemy,
	Resource,
	Flag,
	Item
}

public class PingSystem
{
	public static float GetLifetime( PingType type )
	{
		return type switch
		{
			PingType.Generic => 5f,
			PingType.Enemy => 4f,
			PingType.Resource => 30f,
			_ => 5f
		};
	}
}

namespace Facepunch.Gunfight.Models;

public class Player : BaseModel
{
	public long SteamId { get; set; }

	public ulong Experience { get; set; }

	public Player( long steamId )
	{
		SteamId = steamId;
	}

	public struct UpdateRequest
	{
		public ulong Experience { get; set; }
	}
}

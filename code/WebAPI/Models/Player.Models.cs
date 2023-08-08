namespace Facepunch.Gunfight.Models;

public class Player : BaseModel
{
	public string SteamId { get; set; }

	public ulong Experience { get; set; }

	public Player()
	{
	}

	public Player( string steamId )
	{
		SteamId = steamId;
	}

	public class UpdateRequest
	{
		public ulong Experience { get; set; }
	}
	
	public class GiveExperienceRequest
	{
		public ulong Experience { get; set; }
	}
}

using System.Net.Http;
using System.Text;
using Facepunch.Gunfight.Models;

namespace Facepunch.Gunfight;

public partial class WebAPI
{
	public static class Player
	{
		public static async Task<ulong> GiveExperience( ulong amount )
		{
			// This may look confusing. But it's intentional.
			Game.AssertClient();
			
			var player = await HttpPut<Models.Player>( "Player/xp", new StringContent( $"{amount}", null, "application/json" ) );
			
			// Return new XP
			return player.Experience;
		}

		public static async Task<Models.Player> GetPlayer( ulong steamId )
		{
			var player = await HttpGet<Models.Player>( $"Player/{steamId}" );
			return player;
		}
		
		public static async Task<Models.Player> GetLocalPlayer()
		{
			Game.AssertClientOrMenu();
			return await GetPlayer( Convert.ToUInt64( Game.SteamId ) );
		}
	}
}

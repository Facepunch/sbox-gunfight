using System.Net.Http;
using System.Text;
using System.Text.Json;
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

			var json = JsonSerializer.Serialize( new Models.Player.GiveExperienceRequest { Experience = amount } );
			await HttpPost( "Player/xp", new StringContent( json, null, "application/json" ) );
			
			// Return new XP
			return amount;
		}

		public static async Task<Models.Player> GetPlayer( long steamId )
		{
			var player = await HttpGet<Models.Player>( $"Player/{steamId}" );
			return player;
		}
		
		public static async Task<Models.Player> GetLocalPlayer()
		{
			Game.AssertClientOrMenu();
			return await GetPlayer( Game.SteamId );
		}
	}
}

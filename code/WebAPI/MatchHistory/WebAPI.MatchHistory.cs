using System.Net.Http;

namespace Facepunch.Gunfight;

public partial class WebAPI
{
	public static class MatchHistory
	{
		public static async Task<IEnumerable<Models.Match.WithPlayers>> GetAllAsync()
		{
			try
			{
				return await HttpGet<IEnumerable<Models.Match.WithPlayers>>( "MatchHistory" );
			}
			catch ( Exception e )
			{
				// TODO - Handle exceptions nicely 
				Log.Warning( e );
			}

			return null;
		}

		public static async Task SubmitAsync( Models.MatchSubmitRequest request )
		{
			try
			{
				await HttpPut( "MatchHistory", new StringContent( Json.Serialize( request ) ) );
			}
			catch ( Exception e )
			{
				// TODO - Handle exceptions nicely 
				Log.Warning( e );
			}
		}
	}
}


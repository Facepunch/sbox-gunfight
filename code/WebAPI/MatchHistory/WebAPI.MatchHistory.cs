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
	}
}


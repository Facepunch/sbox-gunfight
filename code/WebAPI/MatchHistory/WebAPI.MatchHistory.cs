using System.Net.Http;
using System.Text;
using Facepunch.Gunfight.Models;

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

		private static TimeSince TimeSinceSubmit = 600;
		
		public static async Task SubmitAsync( Models.MatchSubmitRequest request )
		{
			Game.AssertServer();

			// arbitrary rate limiting for this :S
			if ( TimeSinceSubmit < 120 ) return;

			try
			{
				await HttpPut( "MatchHistory", new StringContent( Json.Serialize( request ), null, "application/json" ) );
				TimeSinceSubmit = 0;
			}
			catch ( Exception e )
			{
				// TODO - Handle exceptions nicely 
				Log.Warning( e );
			}
		}

		public static Models.MatchSubmitRequest BuildRequest( DateTimeOffset startTime, DateTimeOffset endTime, string gamemode )
		{
			Game.AssertServer();
			
			var request = new Models.MatchSubmitRequest();
			request.GamemodeIdent = gamemode;
			request.GameLength = ( endTime - startTime );
			request.ServerSteamId = Game.ServerSteamId;
			request.MapIdent = Game.Server.MapIdent;

			request.Players = new();
			foreach ( var cl in Game.Clients )
			{
				var pl = new MatchPlayerSubmitRequest { PlayerSteamId = cl.SteamId, KeyValues = new()
					{
						{ "kills", cl.GetInt( "frags" ).ToString() },
						{ "deaths", cl.GetInt( "deaths" ).ToString() },
						{ "assists", cl.GetInt( "assists" ).ToString() }
					}
				};

				request.Players.Add( pl );
			}
			
			Log.Info( $"Build MatchSubmitRequest. Game Length: {request.GameLength}, player count: {request.Players.Count}" );
			
			return request;
		}
	}
}


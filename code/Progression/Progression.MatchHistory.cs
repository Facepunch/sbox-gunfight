using System;

namespace Facepunch.Gunfight;

public partial class Progression
{
	public static partial class MatchHistory
	{
		const string PERSISTENCE_BUCKET = "progression.matchhistory";
		const int TAKE_HISTORY = 10; // Take the last X games when displaying Match History

		public static int Count()
		{
			return PersistenceSystem.Instance.GetAll<Match>( PERSISTENCE_BUCKET )
				.Values
				.Count;
		}

		public static List<Match> Get()
		{
			return PersistenceSystem.Instance.GetAll<Match>( PERSISTENCE_BUCKET )
				.Values
				.OrderByDescending( x => x.StartTime )
				.Take( TAKE_HISTORY )
				.ToList();
		}

		public struct MatchPlayer
		{
			public long SteamId { get; set; }
			public Dictionary<string, object> Data { get; set; }
		}

		public struct Match
		{
			public string ServerSteamId { get; set; }
			public string ServerName { get; set; }
			public string MapIdent { get; set; }
			public string Gamemode { get; set; }

			public DateTimeOffset StartTime { get; set; }
			public DateTimeOffset EndTime { get; set; }
			public TimeSpan Duration => ( EndTime - StartTime );

			//
			public Dictionary<string, object> Data { get; set; }
			
			//
			public List<MatchPlayer> Players { get; set; }
		}

		[ConCmd.Client( "gunfight_progression_matchhistory" )]
		public static void Record()
		{
			if ( Game.IsServer )
			{
				RpcRecord( To.Everyone );
			}
			else if ( Game.IsClient )
			{
				var match = BuildFromCurrentGame();
				PersistenceSystem.Instance.Set( PERSISTENCE_BUCKET, match.StartTime.ToString(), match );
			}
		}

		[ClientRpc]
		internal static void RpcRecord()
		{
			Record();
		}

		private static List<MatchPlayer> BuildPlayers()
		{
			var list = new List<MatchPlayer>();

			foreach ( var cl in Game.Clients )
			{
				list.Add( new()
				{
					SteamId = cl.SteamId,
					Data = new()
					{
						{ "kills", cl.GetInt( "frags" ) },
						{ "deaths", cl.GetInt( "deaths" ) },
						{ "assists", cl.GetInt( "assists" ) }
					}
				} );
			}

			return list;
		}

		private static Dictionary<string, object> BuildData()
		{
			var dict = new Dictionary<string, object>();

			// TODO - Populate

			return dict;
		}

		public static Match BuildFromCurrentGame()
		{
			var match = new Match
			{
				ServerSteamId = Game.Server.SteamId,
				ServerName = Game.Server.ServerTitle,
				MapIdent = Game.Server.MapIdent,
				Gamemode = GamemodeSystem.SelectedGamemode,
				
				StartTime = GamemodeSystem.Current.MatchStartTime,
				EndTime = DateTimeOffset.UtcNow,

				Data = BuildData(),
				Players = BuildPlayers()
			};

			return match;
		}
	}
}

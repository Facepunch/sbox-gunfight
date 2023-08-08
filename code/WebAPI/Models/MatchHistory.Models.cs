namespace Facepunch.Gunfight.Models;

public class Match
{
	public Guid Id { get; set; }
	public string ServerSteamId { get; set; }
	public string MapIdent { get; set; }
	public string GamemodeIdent { get; set; }

	public DateTimeOffset StartTime { get; set; }
	public DateTimeOffset EndTime { get; set; }

	public class WithPlayers
	{
		public Match Match { get; set; }
		public IEnumerable<MatchPlayer> Players { get; set; }
	}
}

public class MatchSubmitRequest
{
	public string ServerSteamId { get; set; }
	public string MapIdent { get; set; }
	public string GamemodeIdent { get; set; }

	public TimeSpan GameLength { get; set; }

	public List<MatchPlayerSubmitRequest> Players { get; set; }
}

public class MatchPlayer : BaseModel
{
	public Guid MatchId { get; set; }
	public Match Match { get; set; }
	public string PlayerSteamId { get; set; }
	public Dictionary<string, string> KeyValues { get; set; }
}

public class MatchPlayerSubmitRequest
{
	public string PlayerSteamId { get; set; }
	public Dictionary<string, string> KeyValues { get; set; }
}

namespace Facepunch.Gunfight;

public partial class TeamScores : BaseNetworkable
{
	public TeamScores()
	{
		Reset();

		foreach ( var v in Enum.GetValues<Team>() )
		{
			if ( v == Team.Unassigned ) continue;

			Scores[v] = MinimumScore;
		}
	}

	[Net] protected IDictionary<Team, int> Scores { get; set; }

	public virtual int MinimumScore => 0;
	public int MaximumScore => GamemodeSystem.Current?.MaximumScore ?? 4; 


	public Team GetHighestTeam()
	{
		var list = Scores
			.OrderBy( x => x.Value )
			.Select( x => ( Team: x.Key, Score: x.Value ) )
			.ToList();

		var highest = list.Last();
		if ( highest.Score == list[^2].Score ) return Team.Unassigned;

		return highest.Team;
	}

	public void SetScore( Team team, int score )
	{
		var newScore = Math.Clamp( score, MinimumScore, MaximumScore );
		Scores[team] = newScore;

		GamemodeSystem.Current?.OnScoreChanged( team, newScore, newScore == MaximumScore );
	}

	public int GetScore( Team team )
	{
		return Scores[team];
	}

	public int AddScore( Team team, int score )
	{
		var newScore = GetScore( team ) + score;
		SetScore( team, newScore );
		return newScore;
	}

	public int RemoveScore( Team team, int score )
	{
		var newScore = GetScore( team ) - score;
		SetScore( team, newScore );
		return newScore;
	}

	public void Reset()
	{
		// Set initializing scores.
		SetScore( Team.BLUFOR, MinimumScore );
		SetScore( Team.OPFOR, MinimumScore );
	}
}

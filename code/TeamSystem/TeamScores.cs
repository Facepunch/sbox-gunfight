namespace Facepunch.Gunfight;

public partial class TeamScores : BaseNetworkable, INetworkSerializer
{
	public TeamScores()
	{
		Scores = new int[ArraySize];
		OldScores = new int[ArraySize];

		Reset();
	}

	public virtual int MinimumScore => 0;

	[ConVar.Replicated( "gunfight_score_max" )]
	public static int MaximumScore { get; set; } = 5;

	protected static int ArraySize => Enum.GetNames( typeof( Team ) ).Length;
	protected int[] Scores { get; set; }

	protected int[] OldScores { get; set; }

	public Team GetHighestTeam()
	{
		Team highest = Team.Unassigned;
		float lastHighestValue = 0;

		for ( int i = 0; i < Scores.Length; i++ )
		{
			var score = Scores[i];
			Team team = (Team)i;

			if ( score > lastHighestValue )
			{
				highest = team;
				lastHighestValue = score;
			}
			else if ( score == lastHighestValue )
			{
				// We have a draw!
				highest = Team.Unassigned;
			}
		}

		return highest;
	}

	public static Team GetOpposingTeam( Team team )
	{
		if ( team == Team.BLUFOR )
			return Team.OPFOR;

		if ( team == Team.OPFOR )
			return Team.BLUFOR;

		return Team.Unassigned;
	}

	public void SetScore( Team team, int score )
	{
		var newScore = Math.Clamp( score, MinimumScore, MaximumScore );
		Scores[(int)team] = newScore;

		GamemodeSystem.Current?.OnScoreChanged( team, score );

		WriteNetworkData();
	}

	public int? GetOldScore( Team team ) => OldScores?[(int)team];

	public int GetScore( Team team )
	{
		return Scores[(int)team];
	}

	public void AddScore( Team team, int score )
	{
		SetScore( team, GetScore( team ) + score );
	}

	public void RemoveScore( Team team, int score )
	{
		SetScore( team, GetScore( team ) - score );
	}

	public void Read( ref NetRead read )
	{
		OldScores = Scores?.ToArray();

		Scores = new int[ArraySize];

		int count = read.Read<int>();
		for ( int i = 0; i < count; i++ )
			Scores[i] = read.Read<int>();
	}

	public void Write( NetWrite write )
	{
		write.Write( Scores.Length );

		foreach ( var score in Scores )
			write.Write( score );
	}

	public void Reset()
	{
		// Set initializing scores.
		SetScore( Team.BLUFOR, MaximumScore );
		SetScore( Team.OPFOR, MaximumScore );
	}
}

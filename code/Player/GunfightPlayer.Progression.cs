namespace Facepunch.Gunfight;

public partial class GunfightPlayer
{
    public static float KillStreakTime => 10f;

    [Net]
	public TimeSince TimeSinceLastKill { get; set; } = 10f;
    [Net]
	public int CurrentKillStreak { get; set; } = 0;
    [Net]
	public int CurrentMultikill { get; set; } = 0;

	/// <summary>
	/// The person who killed this player last
	/// </summary>
	[Net] 
	public IEntity LastKiller { get; set; }

    public void AddKill()
    {
        if ( TimeSinceLastKill > KillStreakTime )
            CurrentMultikill = 0;

        TimeSinceLastKill = 0;

        CurrentKillStreak++;
        CurrentMultikill++;

        if ( CurrentMultikill == 2 )
            Progression.GiveAward( Client, "DoubleKill" );
        if ( CurrentMultikill == 3 )
            Progression.GiveAward( Client, "TripleKill" );
        if ( CurrentMultikill == 4 )
            Progression.GiveAward( Client, "UltraKill" );
    }

    public void ClearKillStreak()
    {
        CurrentKillStreak = 0;
        CurrentMultikill = 0;
    }
}

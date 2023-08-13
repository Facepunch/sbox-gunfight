namespace Facepunch.Gunfight;

public partial class ProgressionComponent : EntityComponent
{
	private int _level { get; set; }
	private int _experience { get; set; }

	protected override void OnActivate()
	{
		if ( Game.IsClient )
		{
			_ = Sync();
		}
	}

	public int Level
	{
		get => _level;
	}

	public int Experience
	{
		get => _experience;
	}
	
	int LevelToExp( int level )
	{
		return (int)MathF.Round( ( level ^ ( 50 / 27 ) ) * 300 );
	}

	int ExpToLevel( ulong exp )
	{
		return (int)MathF.Round( ( exp / 300 ) ^ ( 27 / 50 ) ) + 1;
	}

	int ExpRemaining( ulong exp )
	{
		return NextLevelExp() - (int)exp;
	}

	int NextLevelExp()
	{
		return LevelToExp( Level + 1 );
	}

	public void Set( ulong exp )
	{
		var level = ExpToLevel( exp );

		_level = level;
		
		var xp = ExpRemaining( exp );

		_experience = xp;
		
		Log.Trace( $"Set level to {Level}, with XP {Experience}"  );
	}

	public async Task Sync()
	{
		Log.Trace( $"Syncing Progression for {Entity.Client.SteamId}" );
		
		// TODO - Hold data locally
	}
}

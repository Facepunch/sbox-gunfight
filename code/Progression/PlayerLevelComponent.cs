namespace Facepunch.Gunfight;

public partial class PlayerLevelComponent : PersistenceComponent
{
	public override string PersistenceBucket => "level";

	private int _level { get; set; } = 0;
	private int _experience { get; set; } = 0;

	protected override void OnActivate()
	{
		if ( Game.IsServer ) RpcUpdate( To.Single( Entity ) );
	}

	[ClientRpc]
	public void RpcUpdate()
	{
		if ( Game.LocalClient == Entity )
		{
			Sync( noNotify: true );
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

	public const int MAX_LEVEL = 50;
	public const int MAX_EXPERIENCE = 1000000;

	int LevelToExp( int level )
	{
		return (int)MathF.Round( ( level ^ ( 50 / 27 ) ) * 600 );
	}

	int ExpToLevel( int exp )
	{
		return (int)MathF.Round( ( exp / 600 ) ^ ( 27 / 50 ) ) + 1;
	}

	int ExpRemaining( int exp )
	{
		return NextLevelExp() - exp;
	}

	int NextLevelExp()
	{
		return LevelToExp( Level + 1 );
	}

	void SetExperience( int exp, bool save = true, bool noNotify = false )
	{
		exp = exp.Clamp( 0, MAX_EXPERIENCE );

		var level = ExpToLevel( exp )
			.Clamp( 0, MAX_LEVEL );

		if ( !noNotify && level > _level )
		{
			NotifyLevelUp( level );
		}

		_level = level;
		_experience = exp;

		if ( save )
		{
			SetPersistent( "experience", exp );
		}
	}

	public void Sync( bool noNotify = false )
	{
		var xp = GetPersistent( "experience", 0 );

		SetExperience( xp, save: false, noNotify: noNotify );
	}

	public void AddExperience( int exp )
	{
		SetExperience( _experience + exp );
	}

	void NotifyLevelUp( int level )
	{
		Log.Info( $"Level up! Now level {level}" );
	}
}

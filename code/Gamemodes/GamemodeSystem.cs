namespace Facepunch.Gunfight;

public partial class GamemodeSystem
{
	[ConVar.Server( "gunfight_gamemode" )]
	public static string SelectedGamemode { get; set; } = "";

	private static GamemodeEntity current;
	public static GamemodeEntity Current
	{
		get
		{
			if ( Host.IsServer ) return current;

			if ( !current.IsValid() )
				current = Entity.All.FirstOrDefault( x => x is GamemodeEntity ) as GamemodeEntity;

			return current;
		}
		set
		{
			current = value;
		}
	}

	protected static GamemodeEntity FetchGamemodeEntity()
	{
		// First, see if the map has a gamemode we want to use already
		var gamemode = Entity.All.FirstOrDefault( x => x is GamemodeEntity ) as GamemodeEntity;

		// If not, use game preferences to create one.
		if ( !gamemode.IsValid() && !string.IsNullOrEmpty( SelectedGamemode ) )
		{
			var gamemodeEntity = TypeLibrary.Create<GamemodeEntity>( SelectedGamemode );
			if ( gamemodeEntity.IsValid() )
			{
				Log.Info( $"Found gamemode from TypeLibrary - {SelectedGamemode}" );
				gamemode = gamemodeEntity;
			}
			else
			{
				Log.Warning( "No gamemode found while fetching." );
			}
		}

		return gamemode;
	}

	public static void SetupGamemode()
	{
		Current = FetchGamemodeEntity();

		if ( Current.IsValid() )
		{
			Current.Initialize();
		}
	}
}

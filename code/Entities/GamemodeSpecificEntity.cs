namespace Facepunch.Gunfight;

public partial class GamemodeSpecificEntity : AnimatedEntity
{
	[Property] public GamemodeType SupportedGamemodes { get; set; } = GamemodeType.Any;

	[GameEvent.Entity.PostSpawn]
	protected void EventPostSpawn()
	{
		if ( SupportedGamemodes.GetArray().Any( x => x == GamemodeSystem.SelectedGamemode || x is null ) )
			return;

		Delete();
		Log.Warning( $"Deleting {this} because gamemode doesn't match" );
	}
}

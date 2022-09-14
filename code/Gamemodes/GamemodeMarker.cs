using System.ComponentModel.DataAnnotations;

namespace Facepunch.Gunfight;

[Library( "gunfight_gamemode_marker" )]
[HammerEntity]
[Display( Name = "Gamemode Marker" ), Icon( "navigation" )]
public partial class GamemodeMarker : Entity
{
	public static IEnumerable<GamemodeMarker> WithTag( string tag )
	{
		return Entity.All.OfType<GamemodeMarker>()
			.Where( x => x.Tags.Has( tag ) );
	}
}

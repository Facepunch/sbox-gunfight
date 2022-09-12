namespace Facepunch.Gunfight;

public class ToExtensions
{
	public static To BLUFOR => Team( Gunfight.Team.BLUFOR );
	public static To OPFOR => Team( Gunfight.Team.BLUFOR );
	public static To Team( Team team ) => To.Multiple( Client.All.Where( x => TeamSystem.GetTeam( x ) == team ) );
}

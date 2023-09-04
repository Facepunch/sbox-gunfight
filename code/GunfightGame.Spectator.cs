namespace Facepunch.Gunfight;

/// <summary>
/// This is the heart of the gamemode. It's responsible for creating the player and stuff.
/// </summary>
public partial class GunfightGame
{
	[ConCmd.Server( "gunfight_togglespectator", Help = "Toggles spectator mode" )]
	public static void ToggleSpectator()
	{
		var cl = ConsoleSystem.Caller;

		var spectator = cl.Pawn is GunfightSpectatorPlayer; 
		if ( spectator )
		{
		    var player = Current.CreatePawn( cl );
			cl.Pawn = player;

            GamemodeSystem.Current?.AssignTeam( cl );

			player.Respawn();
		}
		else
		{
			var pawn = cl.Pawn as Entity;
			pawn?.TakeDamage( DamageInfo.Generic( 5000f ) );
			pawn.Delete();
			cl.Pawn = null;

            // Set team to unassigned
            cl.Components.GetOrCreate<TeamComponent>().Team = Team.Unassigned;

			var newPawn = new GunfightSpectatorPlayer();
			cl.Pawn = newPawn;

			UI.GunfightChatbox.AddChatEntry( To.Everyone, cl.Name, $"became a spectator", cl.SteamId, false );
		}
	}
}

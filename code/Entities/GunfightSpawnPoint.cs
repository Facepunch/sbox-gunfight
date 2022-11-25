namespace Facepunch.Gunfight;

[Library( "gunfight_player_start" )]
[HammerEntity, EditorModel( "models/editor/playerstart.vmdl", FixedBounds = true )]
[Title( "Gunfight: Spawn Point" ), Category( "Setup" ), Icon( "place" )]
public partial class GunfightSpawnPoint : GamemodeSpecificEntity, ISpawnPoint
{
	[Property] public Team Team { get; set; } = Team.Unassigned;

	int ISpawnPoint.GetSpawnPriority() => 0;
	Transform? ISpawnPoint.GetSpawnTransform() => Transform;

	bool ISpawnPoint.IsValidSpawn( GunfightPlayer player )
	{
		return TeamSystem.IsFriendly( player.Team, Team );
	}
}

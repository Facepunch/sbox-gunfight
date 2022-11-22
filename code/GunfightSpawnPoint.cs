namespace Facepunch.Gunfight;

[Library( "gunfight_player_start" )]
[HammerEntity, EditorModel( "models/editor/playerstart.vmdl", FixedBounds = true )]
[Title( "Gunfight: Spawn Point" ), Category( "Setup" ), Icon( "place" )]
public partial class GunfightSpawnPoint : SpawnPoint
{
	[Property] public Team Team { get; set; } = Team.Unassigned;
	[Property] public string GamemodeIdent { get; set; } = "";
}

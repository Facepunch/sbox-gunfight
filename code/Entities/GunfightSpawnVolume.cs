namespace Facepunch.Gunfight;

[Library( "gunfight_spawn_volume" )]
[HammerEntity]
[Title( "Gunfight: Spawn Volume" ), Category( "Setup" ), Icon( "zoom_in_map" )]
public partial class GunfightSpawnVolume : BaseTrigger, ISpawnPoint
{
	[Property] public Team Team { get; set; } = Team.Unassigned;
	[Property] public string PointName { get; set; } = "H";
	[Property] public string NiceName { get; set; } = "Base";

	string ISpawnPoint.GetIdentity() => NiceName;
	int ISpawnPoint.GetSpawnPriority() => 0;
	
	public Vector3 GetRandomPoint()
	{
		return CollisionBounds.RandomPointInside.WithZ( Position.z );
	}

	Transform? ISpawnPoint.GetSpawnTransform() => SpawnPointSystem.GetSuitableSpawn( Transform.WithPosition( GetRandomPoint() ) );

	bool ISpawnPoint.IsValidSpawn( GunfightPlayer player )
	{
		return TeamSystem.IsFriendly( player.Team, Team );
	}

	public override void Spawn()
	{
		Tags.Add( "trigger" );
		SetupPhysicsFromModel( PhysicsMotionType.Keyframed, true );
	}
}

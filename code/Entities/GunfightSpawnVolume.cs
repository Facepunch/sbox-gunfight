namespace Facepunch.Gunfight;

[Library( "gunfight_spawn_volume" )]
[HammerEntity]
[Title( "Gunfight: Spawn Volume" ), Category( "Setup" ), Icon( "zoom_in_map" )]
public partial class GunfightSpawnVolume : BaseTrigger, ISpawnPoint
{
	[Property, Net] public Team Team { get; set; } = Team.Unassigned;
	[Property] public string PointName { get; set; } = "H";
	[Property] public string NiceName { get; set; } = "Base";
	[Property] public bool IsBase { get; set; } = true;

	string ISpawnPoint.GetIdentity() => IsBase ? $"{(IsValidSpawn( GunfightCamera.Target ) ? "Friendly" : "Enemy")} Base" : NiceName;
	int ISpawnPoint.GetSpawnPriority() => 1;
	
	public Vector3 GetRandomPoint()
	{
		var randOffset = CollisionBounds.RandomPointInside;

		return Transform.Position + randOffset.WithZ( 0f );
	}

	Transform? ISpawnPoint.GetSpawnTransform() => SpawnPointSystem.GetSuitableSpawn( Transform.WithPosition( GetRandomPoint() ) );

	public bool IsValidSpawn( GunfightPlayer player )
	{
		return TeamSystem.IsFriendly( player.Team, Team );
	}

	public override void Spawn()
	{
		Tags.Add( "trigger" );
		SetupPhysicsFromModel( PhysicsMotionType.Keyframed, true );
	}
}

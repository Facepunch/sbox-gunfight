namespace Facepunch.Gunfight;

public partial class SpawnPointSystem
{
	[ConVar.Server( "gunfight_debug_spawnsystem" )]
	public static bool Debug { get; set; } = false;

	/// <summary>
	/// The max suitable distance a player can spawn away from the original spawn point location.
	/// </summary>
	public static float MaxSuitableDistance => 1024;
	/// <summary>
	/// How many times we'll try to spawn the player, before falling back to the original spot.
	/// </summary>
	public static int MaxIterations => 60;

	/// <summary>
	/// The range (lowest radius, highest radius) that we use to generate a random point around the spawn.
	/// </summary>
	public static Vector2 Range => new( 128, 256 );

	protected static Vector3 GeneratePoint( Vector3 origin )
	{
		var angle = Game.Random.Int( 0, 360 );
		var radius = Game.Random.Float( Range.x, Range.y );
		var x = radius * MathF.Cos( angle );
		var y = radius * MathF.Sin( angle );

		return origin + new Vector3( x, y, 0 );
	}

	protected static TraceResult TraceBBox( Vector3 start, Vector3 end, Vector3 mins, Vector3 maxs, float liftFeet = 0.0f )
	{
		if ( liftFeet > 0 )
		{
			start += Vector3.Up * liftFeet;
			maxs = maxs.WithZ( maxs.z - liftFeet );
		}

		var tr = Trace.Ray( start, end )
			.Size( mins, maxs )
			.WorldOnly()
			.Run();

		return tr;
	}

	protected static bool IsValidSpot( Vector3 spot, Vector3 destination, Vector3 mins, Vector3 maxs )
	{
		if ( destination.IsNearlyZero() || destination.Distance( spot ) > MaxSuitableDistance )
			return false;

		var trace = TraceBBox( destination, destination + Vector3.Up * 70, mins, maxs, 0 );
		if ( trace.Hit ) return false;

		return true;
	}

	// TODO: Right now this'll grab one spot. Ideally, we'd collate all the valid spots that a player can spawn at.
	// Then weigh them. See if that spot is in the line of sight of an enemy, etc.
	public static Transform GetSuitableSpawn( Transform transform )
	{
		if ( Debug )
		{
			DebugOverlay.Circle( transform.Position, Rotation.From( new( 90, 0, 0 ) ), Range.y, Color.Green.WithAlpha( 0.1f ), 15, false );
			DebugOverlay.Circle( transform.Position, Rotation.From( new( 90, 0, 0 ) ), Range.x, Color.Red.WithAlpha( 0.2f ), 15, false );
		}

		int i = 0;
		while ( i < MaxIterations )
		{
			i++;

			var spot = GeneratePoint( transform.Position ) + Vector3.Up * 20f;
			Vector3 navSpot = Vector3.Zero;
			_ = NavArea.GetClosestNav( spot, NavAgentHull.Default, GetNavAreaFlags.NoFlags, ref navSpot );

			// TODO - move this shite
			var girth = 32 * 0.5f;
			var mins = new Vector3( -girth, -girth, 0 );
			var maxs = new Vector3( +girth, +girth, 72.0f );
			bool valid = IsValidSpot( spot, navSpot, mins, maxs );

			if ( Debug )
			{
				var color = valid ? Color.Green : Color.Red;
				const int time = 15;
				DebugOverlay.Line( navSpot, navSpot + Vector3.Up * 70, color.WithAlpha( 0.5f ), time, false );
				DebugOverlay.Sphere( navSpot, 5, color.WithAlpha( 0.5f ), time, false );
			}

			if ( valid ) 
				return transform.WithPosition( navSpot );

			continue;
		}

		return transform;
	}
}

public interface ISpawnPoint
{
	public string GetIdentity();
	public int GetSpawnPriority();
	public bool IsValidSpawn( GunfightPlayer player );
	public Transform? GetSpawnTransform();
}

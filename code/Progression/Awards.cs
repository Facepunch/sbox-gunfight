namespace Facepunch.Gunfight;

[AttributeUsage( AttributeTargets.Method )]
public class AwardAttribute : Attribute
{
	public string Title { get; set; }
	public string Description { get; set; }

	public int PointsGiven { get; set; }
	public string IconTexture { get; set; }
}

public static class Awards
{
	[Award( Title = "Kill", PointsGiven = 100, Description = "Enemy killed" )]
	public static void Kill( GunfightPlayer player )
	{
		player.Client.AddInt( "frags", 1 );
	}

	[Award( Title = "Team Kill", PointsGiven = -50, Description = "Team killed" )]
	public static void TeamKill( GunfightPlayer player )
	{
		player.Client.AddInt( "frags", -1 );
	}

	[Award( Title = "Point Capture", PointsGiven = 30, Description = "Point captured" )]
	public static void PointCapture( GunfightPlayer player )
	{
		player.Client.AddInt( "captures", 1 );
	}

    public static MethodDescription? Get( string title )
	{
		return TypeLibrary.FindStaticMethods<AwardAttribute>( title ).FirstOrDefault();
	}
}
namespace Facepunch.Gunfight;

[AttributeUsage( AttributeTargets.Method )]
public class AwardAttribute : Attribute
{
	public string Title { get; set; }
	public string Description { get; set; }

	public int PointsGiven { get; set; }
	public string IconTexture { get; set; }

	public bool ShareXP { get; set; } = true;
}

public partial class Awards
{
	[Award( Title = "Kill", PointsGiven = 100, Description = "Enemy killed" )]
	public static void Kill( GunfightPlayer player )
	{
		player.Client.AddInt( "frags", 1 );
	}

	[Award( Title = "Kill Confirmed", PointsGiven = 75, Description = "Kill confirmed" )]
	public static void KillConfirmed( GunfightPlayer player )
	{
	}

	[Award( Title = "First Blood", PointsGiven = 100, Description = "First blood" )]
	public static void FirstBlood( GunfightPlayer player )
	{
		Sound.FromScreen( To.Single( player.Client ), "sounds/announcer/announcer_firstblood.sound" );
	}

	[Award( Title = "Kill Denied", PointsGiven = 25, Description = "Kill Denied" )]
	public static void KillDenied( GunfightPlayer player )
	{
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

	[Award( Title = "Payback", PointsGiven = 0, Description = "Killed the player who killed you last" )]
	public static void Payback( GunfightPlayer player )
	{
		Sound.FromScreen( To.Single( player.Client ), "sounds/announcer/announcer_payback.sound" );
	}
	
	// Multi Kill

	[ClientRpc]
	public static void KillFeedMessage( long steamid, string left, string method )
	{
		KillFeed.Current.AddInformation( steamid, left, method );
	}

	[Award( Title = "Double Kill", PointsGiven = 100, Description = "Double Kill" )]
	public static void DoubleKill( GunfightPlayer player )
	{
		KillFeedMessage( To.Everyone, player.Client.SteamId, player.Client.Name, "got a Double Kill!" );
		player.Client.AddInt( "stat_doublekills", 1 );
		Sound.FromScreen( To.Single( player.Client ), "sounds/announcer/announcer_doublekill.sound" );
	}

	[Award( Title = "Triple Kill", PointsGiven = 200, Description = "Triple Kill" )]
	public static void TripleKill( GunfightPlayer player )
	{
		KillFeedMessage( To.Everyone, player.Client.SteamId, player.Client.Name, "got a Triple Kill!" );
		player.Client.AddInt( "stat_triplekills", 1 );
		Sound.FromScreen( To.Single( player.Client ), "sounds/announcer/announcer_triplekill.sound" );
	}

	[Award( Title = "Ultra Kill", PointsGiven = 500, Description = "Ultra Kill" )]
	public static void QuadKill( GunfightPlayer player )
	{
		KillFeedMessage( To.Everyone, player.Client.SteamId, player.Client.Name, "got a Ultra Kill!" );
		player.Client.AddInt( "stat_ultrakills", 1 );
		Sound.FromScreen( To.Single( player.Client ), "sounds/announcer/announcer_ultrakill.sound" );
	}

	public static MethodDescription? Get( string title )
	{
		return TypeLibrary.FindStaticMethods<AwardAttribute>( title ).FirstOrDefault();
	}
}

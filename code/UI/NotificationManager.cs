namespace Facepunch.Gunfight.UI;

public enum NotificationDockType 
{
    BottomMiddle,
    Middle,
    TopMiddle
}

public partial class NotificationManager
{
    [ClientRpc]
    public static void AddNotification( NotificationDockType dock, string text, int seconds = 5 )
    {
        Current?.Add( dock, text, seconds );
    }

    [ConCmd.Server( "gunfight_notif_test" )]
    public static void NotificationTest()
    {
        AddNotification( NotificationDockType.Middle, "Middle dock notification" );
        AddNotification( NotificationDockType.BottomMiddle, "Bottom Middle dock notification" );
        AddNotification( NotificationDockType.TopMiddle, "Top Middle dock notification" );
    }

	TimeSince LastHint;

	public List<string> Hints = new()
	{
		"Press F+R to switch fire modes if your weapon supports it.",
		"Everything in Gunfight is subject to change."
	};

	[Event.Tick.Client]
	protected void TickHints()
	{
		if ( LastHint > 60f )
		{
			var randHint = Game.Random.FromList( Hints );
			Current?.Add( NotificationDockType.BottomMiddle, randHint, 5 );
			LastHint = 0;
		}
	}
}

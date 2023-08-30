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

	public List<string> Hints => new()
	{
		"Press <InputHint=FireMode> to switch fire modes if your weapon supports it.",
		"Press <InputHint=Slide> to traverse the map quickly by sliding.",
		"Press <InputHint=Interact> interact with things.",
	};

	[GameEvent.Tick.Client]
	protected void TickHints()
	{
		if ( LastHint > 30f )
		{
			var randHint = Game.Random.FromList( Hints );
			Current?.Add( NotificationDockType.BottomMiddle, randHint, 5 );
			LastHint = 0;
		}
	}
}

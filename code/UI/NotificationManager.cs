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
}
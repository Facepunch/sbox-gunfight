using Sandbox.UI;

namespace Facepunch.Gunfight.UI;

public static partial class NotificationSystem
{
	public static List<INotification> Notifications = new();

	public static async void Notify( INotification notification )
	{
		if ( Game.IsServer ) return;

		NotificationOverlay.Instance?.Add( notification );

		await Task.Delay( ( notification.Lifetime * 1000).FloorToInt() );

		Kill( notification );
	}

	public static void Notify( string text, string title = "Notification" )
	{
		if ( Game.IsServer )
		{
			RpcNotify( text, title );
		}
		else
		{
			var notif = new BasicNotification();
			notif.Title = title;
			notif.Text = text;

			Notify( notif );
		}
	}

	[ClientRpc]
	public static void RpcNotify( string text, string title = "Notification" )
	{
		Notify( text, title );
	}

	public static void Kill( INotification notification )
	{
		notification.Destroy();
	}

	[ConCmd.Client( "gunfight_notify_test" )]
	public static void Test()
	{
		Notify( "Testing a notification" );
	}
}

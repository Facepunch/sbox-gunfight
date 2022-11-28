namespace Facepunch.Gunfight.War;

public partial class CapturePoints
{
	[ClientRpc]
	public static void MarkCapture( CapturePointEntity point )
	{
		_ = This.Captured( point );
	}
}

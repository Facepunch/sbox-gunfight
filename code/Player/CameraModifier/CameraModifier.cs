namespace Facepunch.Gunfight;

public abstract class CameraModifier
{
	internal static List<CameraModifier> List = new();

	internal static void Apply( ref CameraSetup setup )
	{
		for ( int i = List.Count; i > 0; i-- )
		{
			var entry = List[i - 1];
			var keep = entry.Update( ref setup );

			if ( !keep )
			{
				entry.OnRemove( ref setup );
				List.RemoveAt( i - 1 );
			}
		}
	}

	protected virtual void OnRemove( ref CameraSetup setup )
	{
	}

	public static void ClearAll()
	{
		List.Clear();
	}

	public CameraModifier()
	{
		if ( Prediction.FirstTime )
		{
			List.Add( this );
		}
	}

	public abstract bool Update( ref CameraSetup setup );
}

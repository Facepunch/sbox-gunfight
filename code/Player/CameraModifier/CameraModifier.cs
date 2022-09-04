namespace Facepunch.Gunfight;

public abstract class CameraModifier
{
	internal static List<CameraModifier> List = new();

	internal static void Apply( ref CameraSetup setup )
	{
		for ( int i = List.Count; i > 0; i-- )
		{
			var keep = List[i - 1].Update( ref setup );

			if ( !keep )
				List.RemoveAt( i - 1 );
		}
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

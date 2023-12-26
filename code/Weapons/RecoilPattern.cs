namespace Gunfight;

public struct RecoilPattern
{
	public List<Vector2> Points { get; set; }

	public Vector2? GetRawPoint( int index )
	{
		var pointCount = Points.Count;
		if ( index + 1 > pointCount ) return null;

		var point = Points[index];
		return point;
	}

	/// <summary>
	/// Tries to get a point, and will wrap around if the index falls over.
	/// </summary>
	/// <param name="index"></param>
	/// <returns></returns>
	public Vector2 GetPoint( int index )
	{
		// TODO: Define this somehow in the recoil pattern itself and pass it to the editor
		var range = new Vector2( -5, 5 );
		var pointCount = Points.Count;

		// Wrap around.
		if ( index + 1 > pointCount )
		{
			index = index % pointCount;
			Log.Info( $"We exceeded {pointCount}, so wrapping around to {index}" );
		}

		var rawPoint = GetRawPoint( index ) ?? default;
		rawPoint.y = range.y - rawPoint.y;

		// If we have nothing to compare against, use the raw point.
		if ( index == 0 )
		{
			return rawPoint;
		}
		else
		{
			var lastPoint = index - 1;
			Vector2 lastPointValue = lastPoint < 0 ? ( GetRawPoint( 0 ) ?? default ) : ( GetRawPoint( lastPoint ) ?? default );
			lastPointValue.y = range.y - lastPointValue.y;

			return new Vector2( rawPoint.x - lastPointValue.x, rawPoint.y - lastPointValue.y );
		}
	}

	public int Count => Points.Count;
}

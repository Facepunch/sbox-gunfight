namespace Facepunch.Gunfight;

public partial class Duck : BaseNetworkable
{
	[Net, Predicted]
	public bool IsActive { get; set; }

	public Duck()
	{
	}

	public void PreTick( PlayerController controller )
	{
		bool wants = Input.Down( InputButton.Duck );

		if ( wants != IsActive )
		{
			if ( wants ) TryDuck();
			else TryUnDuck( controller );
		}

		if ( IsActive )
		{
			controller.SetTag( "ducked" );
		}
	}

	protected void TryDuck()
	{
		IsActive = true;
	}

	protected void TryUnDuck( BasePlayerController controller )
	{
		var pm = controller.TraceBBox( controller.Position, controller.Position, originalMins, originalMaxs );
		if ( pm.StartedSolid ) return;

		IsActive = false;
	}

	// Uck, saving off the bbox kind of sucks
	// and we should probably be changing the bbox size in PreTick
	Vector3 originalMins;
	Vector3 originalMaxs;

	public void UpdateBBox( ref Vector3 mins, ref Vector3 maxs, float scale )
	{
		originalMins = mins;
		originalMaxs = maxs;
	}

	//
	// Could we do this in a generic callback too?
	//
	public float GetWishSpeed()
	{
		if ( !IsActive ) return -1;
		return 64.0f;
	}
}

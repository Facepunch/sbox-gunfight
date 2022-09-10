using Sandbox;

namespace Facepunch.Gunfight;

public partial class BaseMoveMechanic : BaseNetworkable
{
	[Net, Change( "Net_OnActiveChanged" )] public bool IsActive { get; protected set; }
	public virtual bool AlwaysSimulate => false;
	public virtual bool TakesOverControl => false;
	[Net, Predicted] public TimeSince TimeSinceActivate { get; protected set; }

	private PlayerController ctrl;
	protected PlayerController Controller
	{
		get
		{
			if ( Host.IsClient )
			{
				var player = Local.Pawn as GunfightPlayer;
				return player.Controller as PlayerController;
			}
			else
			{
				return ctrl;
			}
		}
		set
		{
			ctrl = value;
		}
	}

	public BaseMoveMechanic() { }
	public BaseMoveMechanic( PlayerController controller ) : this()
	{
		Controller = controller;
	}

	protected void Net_OnActiveChanged( bool before, bool after )
	{
		OnActiveChanged( before, after );
	}

	protected virtual void OnActiveChanged( bool before, bool after )
	{
		//
	}

	public bool Try()
	{
		var newActive = TryActivate();
		if ( newActive != IsActive )
			IsActive = newActive;

		if ( IsActive )
		{
			TimeSinceActivate = 0;

			if ( BasePlayerController.Debug )
			{
				Log.Info( "ACTIVATED: " + GetType().Name );
			}
		}

		return IsActive;
	}

	public void StopTry()
	{
		if ( !IsActive ) return;

		TimeSinceActivate = 0;
		IsActive = false;
	}

	public virtual void PreSimulate() { }
	public virtual void PostSimulate() { }
	public virtual void Simulate() { }
	public virtual void UpdateBBox( ref Vector3 mins, ref Vector3 maxs, float scale = 1f ) { }
	public virtual float GetWishSpeed() { return -1f; }
	protected virtual bool TryActivate() { return false; }

	protected WallInfo? GetWallInfo( Vector3 direction )
	{
		var trace = Controller.TraceBBox( Controller.Position, Controller.Position + direction * 100 );
		if ( !trace.Hit ) return null;

		var height = ApproximateWallHeight( Controller.Position, trace.Normal, 500f, 100f, 32 );

		return new WallInfo()
		{
			Height = height,
			Distance = trace.Distance,
			Normal = trace.Normal,
			Trace = trace
		};
	}

	private static float ApproximateWallHeight( Vector3 startPos, Vector3 wallNormal, float maxHeight, float maxDist, int precision = 16 )
	{
		var step = maxHeight / precision;
		var wallFoudn = false;
		for ( int i = 0; i < precision; i++ )
		{
			startPos.z += step;
			var trace = Trace.Ray( startPos, startPos - wallNormal * maxDist )
				.WorldOnly()
				.Run();

			if ( !trace.Hit && !wallFoudn ) continue;
			if ( trace.Hit )
			{
				wallFoudn = true;
				continue;
			}

			return startPos.z;
		}
		return 0f;
	}

	public virtual float? GetEyeHeight()
	{
		return null;
	}

	public virtual float? GetGroundFriction()
	{
		return null;
	}
}

public struct WallInfo
{
	public float Distance;
	public Vector3 Normal;
	public float Height;
	public TraceResult Trace;
}

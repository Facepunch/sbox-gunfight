using Sandbox;

namespace Facepunch.Gunfight;

public partial class BaseMoveMechanic : BaseNetworkable
{
	private bool isActive;
	public bool IsActive
	{
		get => isActive;
		set
		{
			var before = isActive;
			isActive = value;
			
			if ( isActive != before )
				OnActiveChanged( before, isActive );
		}
	}
	public virtual bool AlwaysSimulate => false;
	public virtual bool TakesOverControl => false;
	public TimeSince TimeSinceActivate { get; protected set; }

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

	public virtual void StopTry()
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

	protected WallInfo GetWallInfo( Vector3 direction )
	{
		var trace = Controller.TraceBBox( Controller.Position, Controller.Position + direction * 100 );
		if ( !trace.Hit ) return null;

		Vector3 tracePos;
		var height = ApproximateWallHeight( Controller.Position, trace.Normal, 500f, 100f, 100, out tracePos );

		return new WallInfo()
		{
			Height = height,
			Distance = trace.Distance,
			Normal = trace.Normal,
			Trace = trace,
			TracePos = tracePos,
		};
	}

	private static int MaxWallTraceIterations => 20;
	private static float ApproximateWallHeight( Vector3 startPos, Vector3 wallNormal, float maxHeight, float maxDist, int precision, out Vector3 tracePos )
	{
		tracePos = Vector3.Zero;

		var step = maxHeight / precision;

		float currentHeight = 0f;
		var foundWall = false;
		for ( int i = 0; i < Math.Min( precision, MaxWallTraceIterations ); i++ )
		{
			startPos.z += step;
			currentHeight += step;
			var trace = Trace.Ray( startPos, startPos - wallNormal * maxDist )
				.WorldOnly()
				.Run();

			DebugOverlay.TraceResult( trace );

			if ( !trace.Hit && !foundWall ) continue;
			if ( trace.Hit )
			{
				tracePos = trace.HitPosition;

				foundWall = true;
				continue;
			}
			return currentHeight;
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

public class WallInfo
{
	public float Distance;
	public Vector3 Normal;
	public float Height;
	public TraceResult Trace;
	public Vector3 TracePos;
}

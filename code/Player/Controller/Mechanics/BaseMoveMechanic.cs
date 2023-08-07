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
	public GunfightPlayer Player => ctrl.Player;

	private PlayerController ctrl;
	protected PlayerController Controller
	{
		get
		{
			if ( Game.IsClient )
			{
				var player = Game.LocalPawn as GunfightPlayer;
				return player.Controller;
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

	protected WallInfo GetWallInfo( Vector3 direction, float maxHeight = 500f, float maxDistance = 100f, int precision = 128 )
	{
		var trace = Controller.TraceBBox( Controller.Position, Controller.Position + direction * 100 );
		if ( !trace.Hit ) return null;

		Vector3 tracePos;
		var height = ApproximateWallHeight( this, Controller.Position + Vector3.Up * 10f, trace.Normal, maxHeight, maxDistance, precision, out tracePos, out float absoluteHeight );

		return new WallInfo()
		{
			Height = height,
			AbsoluteHeight = absoluteHeight,
			Distance = trace.Distance,
			Normal = trace.Normal,
			Trace = trace,
			TracePos = tracePos,
		};
	}

	public virtual bool IsDebugging => false;

	private static int MaxWallTraceIterations => 40;
	private static float ApproximateWallHeight( BaseMoveMechanic mechanic, Vector3 startPos, Vector3 wallNormal, float maxHeight, float maxDist, int precision, out Vector3 tracePos, out float absoluteHeight )
	{
		tracePos = Vector3.Zero;
		absoluteHeight = startPos.z;

		var step = maxHeight / precision;

		Vector3 firstHit = Vector3.Zero;
		
		float currentHeight = 0f;
		var foundWall = false;
		for ( int i = 0; i < Math.Min( precision, MaxWallTraceIterations ); i++ )
		{
			startPos.z += step;
			currentHeight += step;
			var trace = Trace.Ray( startPos, startPos - wallNormal * maxDist )
				.StaticOnly()
				.Run();

			if ( firstHit.IsNearZeroLength && trace.Hit )
			{
				firstHit = trace.HitPosition;
			}
			else
			{
				if ( trace.HitPosition.WithZ( firstHit.z ).Distance( firstHit ) > 10f )
				{
					return currentHeight;
				}
			}

			
			if ( mechanic.IsDebugging ) DebugOverlay.TraceResult( trace );

			if ( !trace.Hit && !foundWall ) continue;
			if ( trace.Hit )
			{
				tracePos = trace.HitPosition;

				foundWall = true;
				continue;
			}

			absoluteHeight = startPos.z;
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
	public float AbsoluteHeight;
	public TraceResult Trace;
	public Vector3 TracePos;
}

namespace Facepunch.Gunfight;

public partial class UnstuckMechanic : BaseMoveMechanic
{
	private int _stuckTries = 0;

	public UnstuckMechanic() { }
	public UnstuckMechanic( PlayerController ctrl ) : base( ctrl ) { }

	protected override bool TryActivate()
	{
		if ( Controller.Vault?.IsActive ?? false ) return false;
		
		var result = Controller.TraceBBox( Controller.Position, Controller.Position );

		// Not stuck, we cool
		if ( !result.StartedSolid )
		{

			_stuckTries = 0;
			return false;
		}

		if ( BasePlayerController.Debug )
		{
			DebugOverlay.Text( $"[stuck in {result.Entity}]", Controller.Position, Color.Red );
			Box( result.Entity, Color.Red );
		}

		if ( Game.IsClient )
			return false;

		int AttemptsPerTick = 20;

		for ( int i = 0; i < AttemptsPerTick; i++ )
		{
			var pos = Controller.Position + Vector3.Random.Normal * (((float)_stuckTries) / 2.0f);

			// First try the up direction for moving platforms
			if ( i == 0 )
			{
				pos = Controller.Position + Vector3.Up * 5;
			}

			result = Controller.TraceBBox( pos, pos );

			if ( !result.StartedSolid )
			{
				if ( BasePlayerController.Debug )
				{
					DebugOverlay.Text( $"unstuck after {_stuckTries} tries ({_stuckTries * AttemptsPerTick} tests)", Controller.Position, Color.Green, 5.0f );
					DebugOverlay.Line( pos, Controller.Position, Color.Green, 5.0f, false );
				}

				Controller.Position = pos;
				return false;
			}
			else
			{
				if ( BasePlayerController.Debug )
				{
					DebugOverlay.Line( pos, Controller.Position, Color.Yellow, 0.5f, false );
				}
			}
		}

		_stuckTries++;
		
		return false;
	}

	public void Box( Entity ent, Color color, float duration = 0.0f )
	{
		if ( ent is ModelEntity modelEnt )
		{
			var bbox = modelEnt.CollisionBounds;
			DebugOverlay.Box( modelEnt.Position, modelEnt.Rotation, bbox.Mins, bbox.Maxs, color, duration );
		}
		else
		{
			DebugOverlay.Box( ent.Position, ent.Rotation, -1, 1, color, duration );
		}
	}
}

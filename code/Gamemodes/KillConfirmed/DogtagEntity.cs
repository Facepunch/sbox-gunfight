namespace Facepunch.Gunfight;

public partial class DogtagEntity : BaseTrigger, IHudMarker
{
    public Team ScoringTeam { get; set; } = Team.Unassigned;
    public TimeSince TimeSinceCreated = 0;
    public float Lifetime => 30f;

    public override void Spawn()
    {
        base.Spawn();

    	SetupPhysicsFromSphere( PhysicsMotionType.Keyframed, Vector3.Zero, 32f );
		Transmit = TransmitType.Always;
        Tags.Add( "trigger" );
    }
	
    string IHudMarker.GetClass() => "dogtag";
	bool IHudMarker.UpdateMarker( ref HudMarkerBuilder info )
	{
		if ( !this.IsValid() )
			return false;

		info.Text = "";
		info.Position = Position + Rotation.Up * 150f;
		info.StayOnScreen = true;

		return true;
	}

    public override void StartTouch( Entity other )
	{
		base.StartTouch( other );

		if ( Host.IsServer && other is GunfightPlayer player )
		{
			Take( player );
		}
	}

    public void Take( GunfightPlayer player )
    {
        var team = player.Team;
        if ( team == ScoringTeam )
        {
            // Deny
			UI.NotificationManager.AddNotification( UI.NotificationDockType.BottomMiddle, $"Denied kill!", 3 );
        }
        else
        {
			UI.NotificationManager.AddNotification( UI.NotificationDockType.BottomMiddle, $"Secured kill!", 3 );
            var scores = GunfightGame.Current.Scores;
            scores.AddScore( team, 1 );
        }

        Poof();
    }

    public void Poof()
    {
        // TODO - VFX
        Delete();
    }

    [Event.Tick.Server]
    public void TickServer()
    {
        if ( TimeSinceCreated > Lifetime )
        {
            Poof();
        }
    }
}

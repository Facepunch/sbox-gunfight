namespace Facepunch.Gunfight;

public partial class DogtagEntity : BaseTrigger, IHudMarker
{
    [Net] public Team ScoringTeam { get; set; } = Team.Unassigned;
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
		info.Position = Position + Rotation.Up * 30f;
		info.StayOnScreen = true;

        info.Classes["friendly"] = TeamSystem.IsFriendly( TeamSystem.MyTeam, ScoringTeam );

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
			Progression.GiveAward( player.Client, "KillDenied" );
        }
        else
        {
			Progression.GiveAward( player.Client, "KillConfirmed" );

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

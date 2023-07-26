namespace Facepunch.Gunfight;

public partial class DogtagEntity : BaseTrigger
{
    [Net, Change( "UpdateDogtag" )] public Team ScoringTeam { get; set; } = Team.Unassigned;
    public TimeSince TimeSinceCreated = 0;
    public float Lifetime => 30f;

    public Particles Particle;

    public override void Spawn()
    {
        base.Spawn();

    	SetupPhysicsFromSphere( PhysicsMotionType.Keyframed, Vector3.Zero, 32f );
		Transmit = TransmitType.Always;
        Tags.Add( "trigger" );
    }

    public override void StartTouch( Entity other )
	{
		base.StartTouch( other );

		if ( Game.IsServer && other is GunfightPlayer player )
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
        Particles.Create( "particles/break/break.cardboard.vpcf", Position + Vector3.Up * 20f );

        Delete();
    }

    [GameEvent.Tick.Server]
    public void TickServer()
    {
        if ( TimeSinceCreated > Lifetime )
        {
            Poof();
        }
    }

    public void UpdateDogtag()
    {
        Particle?.Destroy( true );

        var isFriendly = TeamSystem.IsFriendly( TeamSystem.MyTeam, ScoringTeam );

        Particle = Particles.Create( "particles/gameplay/dogtags/team_1_dog_tags.vpcf", this, true );
        Particle.SetPosition( 1, new( isFriendly ? 0 : 1, 0, 0 ) );
    }
}

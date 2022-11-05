namespace Facepunch.Gunfight;

public partial class DogtagEntity : BaseTrigger, IHudMarker
{
    public Team Team { get; set; } = Team.Unassigned;
    public TimeSince TimeSinceCreated = 0;
    public float Lifetime => 30f;

    public override void Spawn()
    {
        base.Spawn();

        // TODO - Set model based on team
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
        Poof();

        // TODO - Give points, stats
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

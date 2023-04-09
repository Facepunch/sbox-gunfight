namespace Facepunch.Gunfight;

public partial class GunfightPlayer
{
	public string GetClass()
	{
		return "player";
	}
	bool CalculateVisibility()
	{
		var tr = Trace.Ray( Camera.Position, AimRay.Position )
			.WorldAndEntities()
			.Ignore( Game.LocalPawn )
			.Run();

		if ( tr.Hit && tr.Entity == this )
			return true;
		else
			return false;
	}

	public bool UpdateMarker( ref HudMarkerBuilder builder )
	{
		if ( !this.IsValid() )
			return false;

		if ( this == Game.LocalPawn ) 
			return false;

		if ( LifeState != LifeState.Alive )
			return false;

		var friendState = TeamSystem.GetFriendState( Team, TeamSystem.MyTeam );
		var isEnemy = friendState == TeamSystem.FriendlyStatus.Hostile;
		if ( isEnemy )
		{
			if ( !CalculateVisibility() )
				return false;
		}

		builder.Text = $"{Client.Name}";
		builder.MaxDistance = isEnemy ? 1000000f : 10000000f;
		builder.DistanceScale = 0.5f;
		builder.Position = AimRay.Position + Vector3.Up * 80f;

		// Classes
		builder.Classes["friendly"] = friendState == TeamSystem.FriendlyStatus.Friendly;
		builder.Classes["enemy"] = friendState == TeamSystem.FriendlyStatus.Hostile;

		return true;
	}
}

namespace Facepunch.Gunfight;

public partial class GunfightPlayer
{
	public string GetClass()
	{
		return "player";
	}
	bool CalculateVisibility()
	{
		var tr = Trace.Ray( CurrentView.Position, EyePosition )
			.WorldAndEntities()
			.Ignore( Local.Pawn )
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

		if ( this == Local.Pawn ) 
			return false;

		if ( LifeState != LifeState.Alive )
			return false;

		//if ( !Client.IsFriend( Local.Client ) )
		//	return false;

		if ( !CalculateVisibility() )
			return false;

		builder.Text = $"{Client.Name}";
		builder.MaxDistance = 1000000f;
		builder.DistanceScale = 0.5f;
		builder.Position = EyePosition + Vector3.Up * 15f;

		return true;
	}
}

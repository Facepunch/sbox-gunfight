namespace Facepunch.Gunfight;

public partial class GunfightPlayer
{
	public Team Team
	{
		get
		{
			var cl = Client;
			if ( cl is null ) return Team.Unassigned;

			return cl.Components.GetOrCreate<TeamComponent>().Team;
		}
		set
		{
			var cl = Client;
			if ( cl is null ) return;

			cl.Components.GetOrCreate<TeamComponent>().Team = value;
		}
	}
}

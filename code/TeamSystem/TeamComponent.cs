namespace Facepunch.Gunfight;

public partial class TeamComponent : EntityComponent
{
	[Net] private Team team { get; set; }
	
	public Team Team
	{
		get => team;
		set
		{
			var previous = team;
			team = value;

			var cl = Entity as IClient;
			var pawn = cl?.Pawn as Entity;
			SetupTags( pawn ?? Entity, previous, team );
		}
	}

	public void SetupTags( Entity ent, Team before, Team after )
	{
		ent.Tags.Remove( before.GetTag() );
		ent.Tags.Add( after.GetTag() );
	}
}

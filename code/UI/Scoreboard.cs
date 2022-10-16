using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Facepunch.Gunfight;

public class Scoreboard : Panel
{
	public struct TeamSection
	{
		public Label TeamName;
		public Panel TeamIcon;
		public Panel TeamContainer;
		public Panel Header;
		public Panel TeamHeader;
		public Label TeamTickets;
		public Panel Canvas;
	}

	public Dictionary<Client, ScoreboardEntry> Rows = new();
	public Dictionary<Team, TeamSection> TeamSections = new();

	bool IsLoaded = false;

	public Scoreboard()
	{
		StyleSheet.Load( "/UI/Scoreboard.scss" );

		AddClass( "scoreboard" );
	}

	public override void Tick()
	{
		base.Tick();

		SetClass( "open", Input.Down( InputButton.Score ) );

		if ( !IsVisible )
			return;

		if ( !IsLoaded )
		{
			AddTeamHeader( Team.BLUFOR );
			AddTeamHeader( Team.OPFOR );
			IsLoaded = true;
		}

		foreach ( var client in Client.All.Except( Rows.Keys ) )
		{
			var entry = AddClient( client );
			Rows[client] = entry;
		}

		foreach ( var client in Rows.Keys.Except( Client.All ) )
		{
			if ( Rows.TryGetValue( client, out var row ) )
			{
				row?.Delete();
				Rows.Remove( client );
			}
		}

		foreach ( var kv in Rows )
		{
			CheckTeamIndex( kv.Value );
		}

		foreach ( var kv in TeamSections )
		{
			kv.Value.TeamTickets.Text = $"{GunfightGame.Current.Scores.GetScore( kv.Key )}";
		}
	}

	protected void AddTeamHeader( Team team )
	{
		var section = new TeamSection
		{

		};

		section.TeamContainer = Add.Panel( "team" );
		section.TeamHeader = section.TeamContainer.Add.Panel( "team-header" );
		section.Header = section.TeamContainer.Add.Panel( "table-header" );
		section.Canvas = section.TeamContainer.Add.Panel( "canvas" );

		section.TeamIcon = section.TeamHeader.Add.Panel( "teamIcon" );
		section.TeamName = section.TeamHeader.Add.Label( TeamSystem.GetTeamName( team ), "teamName" );
		section.TeamTickets = section.TeamHeader.Add.Label( $"{GunfightGame.Current.Scores.GetScore( team )}", "teamTickets" );

		var hudClass = TeamSystem.GetTeamName( team );

		section.TeamIcon.AddClass( hudClass );

		section.Header.Add.Label( "NAME", "name" );
		section.Header.Add.Label( "CAPTURES", "captures" );
		section.Header.Add.Label( "KILLS", "kills" );
		section.Header.Add.Label( "DEATHS", "deaths" );
		section.Header.Add.Label( "SCORE", "score" );

		section.Canvas.AddClass( hudClass );
		section.Header.AddClass( hudClass );
		section.TeamHeader.AddClass( hudClass );

		TeamSections[team] = section;
	}

	protected virtual ScoreboardEntry AddClient( Client entry )
	{
		var teamIndex = entry.Components.Get<TeamComponent>()?.Team ?? Team.Unassigned;

		if ( !TeamSections.TryGetValue( teamIndex, out var section ) )
		{
			section = TeamSections[0];
		}

		var p = section.Canvas.AddChild<ScoreboardEntry>();
		p.Client = entry;
		return p;
	}

	private void CheckTeamIndex( ScoreboardEntry entry )
	{
		Team currentTeamIndex = Team.Unassigned;
		var teamIndex = entry.Client.Components.Get<TeamComponent>()?.Team ?? Team.Unassigned;

		foreach ( var kv in TeamSections )
		{
			if ( kv.Value.Canvas == entry.Parent )
			{
				currentTeamIndex = kv.Key;
			}
		}

		if ( currentTeamIndex != teamIndex )
		{
			entry.Parent = TeamSections[teamIndex].Canvas;
		}
	}
}

public class ScoreboardEntry : Panel
{
	public Client Client { get; set; }
	public Label PlayerName { get; set; }
	public Label Captures { get; set; }
	public Label Kills { get; set; }
	public Label Deaths { get; set; }
	public Label Score { get; set; }

	private RealTimeSince TimeSinceUpdate { get; set; }

	public ScoreboardEntry()
	{
		AddClass( "entry" );

		PlayerName = Add.Label( "PlayerName", "name" );
		Captures = Add.Label( "", "captures" );
		Kills = Add.Label( "", "kills" );
		Deaths = Add.Label( "", "deaths" );
		Score = Add.Label( "", "score" );
	}

	public override void Tick()
	{
		base.Tick();

		if ( !IsVisible )
			return;

		if ( !Client.IsValid() )
			return;

		if ( TimeSinceUpdate < 1f )
			return;

		TimeSinceUpdate = 0;
		UpdateData();
	}

	public virtual void UpdateData()
	{
		PlayerName.Text = Client.Name;
		Captures.Text = Client.GetInt( "captures" ).ToString();
		Kills.Text = Client.GetInt( "frags" ).ToString();
		Deaths.Text = Client.GetInt( "deaths" ).ToString();
		Score.Text = Client.GetInt( "score" ).ToString();
		SetClass( "me", Client == Local.Client );
	}

	public virtual void UpdateFrom( Client client )
	{
		Client = client;
		UpdateData();
	}
}

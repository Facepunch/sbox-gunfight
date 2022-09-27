using Sandbox.UI;

namespace Facepunch.Gunfight;

[UseTemplate]
public partial class GunfightScorePanel : Panel
{
	// @ref
	public Panel FriendlyBoxes { get; set; }
	// @ref
	public Panel EnemyBoxes { get; set; }
	
	// @text
	public string FriendlyScore { get; set; } = "0";
	// @text
	public string EnemyScore { get; set; } = "0";
	// @text
	public string TimeLabel => (GamemodeSystem.Current as GunfightGamemode)?.GetTimeLeftLabel() ?? "00:00";

	protected List<Panel> Boxes { get; set; } = new();
	protected int BoxAmount => GunfightGame.Current.Scores.MaximumScore;

	[Event( "gunfight.scores.changed" )]
	public void Update()
	{	
		Boxes.Clear();

		UpdateScores( TeamSystem.MyTeam );
		UpdateScores( TeamSystem.MyTeam.GetOpponent() );

		FriendlyScore = $"{GunfightGame.Current.Scores.GetScore( TeamSystem.MyTeam )}";
		EnemyScore = $"{GunfightGame.Current.Scores.GetScore( TeamSystem.MyTeam.GetOpponent() )}";
	}

	protected void UpdateScores( Team team )
	{
		var pnl = team == TeamSystem.MyTeam ? FriendlyBoxes : EnemyBoxes;
		pnl.DeleteChildren( true );

		var score = GunfightGame.Current.Scores.GetScore( team );

		for ( int i = 0; i < BoxAmount; i++ )
		{
			var box = pnl.AddChild<Panel>( "box" );

			if ( score > i )
				box.SetClass( "won", true );
			
			Boxes.Add( box );
		}
	}

	protected override void PostTemplateApplied()
	{
		base.PostTemplateApplied();
		Update();
	}
}

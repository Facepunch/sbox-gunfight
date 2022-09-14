using Sandbox.UI;

namespace Facepunch.Gunfight;

[UseTemplate]
public partial class GunfightScorePanel : Panel
{
	// @ref
	public Panel BoxLayout { get; set; }
	// @text
	public string GameState => (GamemodeSystem.Current as GunfightGamemode)?.GetGameStateLabel() ?? "";
	// @text
	public string TimeLabel => (GamemodeSystem.Current as GunfightGamemode)?.GetTimeLeftLabel() ?? "00:00";
	// @text
	public string FriendlyCount => $"{TeamSystem.MyTeam.AlivePlayers().Count()}";
	public string EnemyCount => $"{TeamSystem.MyTeam.GetOpponent().AlivePlayers().Count()}";

	protected List<Panel> Boxes { get; set; } = new();

	public int BoxAmount => ( GunfightGame.Current.Scores.MaximumScore * 2 ) - 1;

	[Event( "gunfight.scores.changed" )]
	public void Update()
	{
		BoxLayout.DeleteChildren( true );
		Boxes.Clear();

		var myScore = GunfightGame.Current.Scores.GetScore( TeamSystem.MyTeam );
		var enemyScore = GunfightGame.Current.Scores.GetScore( TeamSystem.GetEnemyTeam( TeamSystem.MyTeam ) );

		for ( int i = 0; i < BoxAmount; i++ )
		{
			var pnl = BoxLayout.AddChild<Panel>( "box" );
			if ( i == BoxAmount / 2 )
				pnl.AddClass( "winning" );

			Boxes.Add( pnl );
		}

		for ( int i = 0; i < myScore; i++ )
		{
			var box = Boxes[i];
			box.SetClass( "friendly", true );
		}

		for ( int i = BoxAmount - 1; i > BoxAmount - 1 - enemyScore; i-- )
		{
			var box = Boxes[i];
			box.SetClass( "enemy", true );
		}
	}

	protected override void PostTemplateApplied()
	{
		base.PostTemplateApplied();
		Update();
	}
}

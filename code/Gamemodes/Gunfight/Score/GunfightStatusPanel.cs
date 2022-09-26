using Sandbox.UI;

namespace Facepunch.Gunfight;

[UseTemplate]
public partial class GunfightStatusPanel : Panel
{
	public static GunfightStatusPanel Current { get; set; }
	public GunfightStatusPanel() => Current = this;

	// @ref
	public Panel Friendlies { get; set; }
	// @ref
	public Panel Enemies { get; set; }

	// @ref
	public Panel FriendlyBar { get; set; }
	// @ref
	public Panel EnemyBar { get; set; }

	// @text
	public string GameState => (GamemodeSystem.Current as GunfightGamemode)?.GetGameStateLabel() ?? "";
	// @text
	public string TimeLabel => (GamemodeSystem.Current as GunfightGamemode)?.GetTimeLeftLabel() ?? "00:00";
	// @text
	public string FriendlyHealth => $"{GetHealth( AliveFriendlyPlayers )}";
	public string EnemyHealth => $"{GetHealth( AliveEnemyPlayers )}";

	public int GetHealth( IEnumerable<GunfightPlayer> players )
	{
		var hp = 0f;
		players.ToList().ForEach( x => hp += x.Health );

		return hp.CeilToInt();
	}

	public float GetMaxHealth( IEnumerable<GunfightPlayer> players )
	{
		var hp = 0f;
		players.ToList().ForEach( x => hp += x.MaxHealth );

		return hp.CeilToInt();
	}

	public IEnumerable<GunfightPlayer> FriendlyPlayers => TeamSystem.MyTeam.AllPlayers();
	public IEnumerable<GunfightPlayer> AliveFriendlyPlayers => TeamSystem.MyTeam.AlivePlayers();
	public IEnumerable<GunfightPlayer> EnemyPlayers => TeamSystem.MyTeam.GetOpponent().AllPlayers();
	public IEnumerable<GunfightPlayer> AliveEnemyPlayers => TeamSystem.MyTeam.GetOpponent().AlivePlayers();

	protected List<Panel> Boxes { get; set; } = new();

	public int BoxAmount => ( GunfightGame.Current.Scores.MaximumScore * 2 ) - 1;

	[ClientRpc]
	public static void RpcUpdate()
	{
		Current?.Update();
	}

	string GetClass( GunfightPlayer player )
	{
		var @class = "person";
		if ( player.LifeState != LifeState.Alive ) @class += " dead";
		return @class;
	}

	string GetLabel( GunfightPlayer player )
	{
		var label = "person";
		if ( player.LifeState != LifeState.Alive ) label = "person_off";
		return label;
	}

	public override void Tick()
	{
		base.Tick();

		var friendlyMax = GetMaxHealth( FriendlyPlayers );
		var friendlyCurrent = GetHealth( AliveFriendlyPlayers );
		FriendlyBar.Style.Width = Length.Fraction( friendlyCurrent / friendlyMax );

		var enemyMax = GetMaxHealth( EnemyPlayers );
		var enemyCurrent = GetHealth( AliveEnemyPlayers );
		EnemyBar.Style.Width = Length.Fraction( enemyCurrent / enemyMax );
	}

	[Event( "gunfight.scores.changed" )]
	public void Update()
	{
		Friendlies.DeleteChildren( true );
		Enemies.DeleteChildren( true );

		foreach ( var player in FriendlyPlayers )
		{
			var label = Friendlies.AddChild<Label>( GetClass( player ) );
			label.Text = GetLabel( player );
		}

		foreach ( var player in EnemyPlayers )
		{
			var label = Enemies.AddChild<Label>( GetClass( player ) );
			label.Text = GetLabel( player );
		}
	}

	protected override void PostTemplateApplied()
	{
		base.PostTemplateApplied();

		Update();
	}
}

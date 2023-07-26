using Facepunch.Gunfight.UI;
using Sandbox.UI;

namespace Facepunch.Gunfight;

public enum HudVisibilityState
{
	Visible,
	AlmostVisible,
	Invisible
}

public partial class GunfightHud : HudEntity<HudRootPanel>
{
	public static Panel CurrentHudPanel { get; protected set; }
	
	public static HudVisibilityState HudState = HudVisibilityState.Visible;

	[GameEvent.Tick.Client]
	protected void DoTick()
	{
		if ( !GamemodeSystem.Current.IsValid() ) return;
		if ( CurrentHudPanel is not null ) return;

		var gamemode = GamemodeSystem.Current;
		CurrentHudPanel = gamemode.HudPanel;

		if ( CurrentHudPanel is not null )
			CurrentHudPanel.Parent = this.RootPanel;
	}

	[ClientRpc]
	public static void ShowDeathInformation( IClient attacker )
	{
		var deathInfo = GunfightGame.Current.Hud.RootPanel.AddChild<DeathInformation>();
		deathInfo.Attacker = attacker;

		var delayedKill = async () => {
			await GameTask.DelaySeconds( 3f );
			deathInfo.Delete();
		};

		_ = delayedKill();
	}
}

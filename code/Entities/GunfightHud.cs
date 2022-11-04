using Facepunch.Gunfight.UI;
using Sandbox.UI;

namespace Facepunch.Gunfight;

public partial class GunfightHud : HudEntity<HudRootPanel>
{
	public static Panel CurrentHudPanel { get; protected set; }

	[Event.Tick.Client]
	protected void DoTick()
	{
		if ( !GamemodeSystem.Current.IsValid() ) return;
		if ( CurrentHudPanel is not null ) return;

		var gamemode = GamemodeSystem.Current;
		CurrentHudPanel = gamemode.GetHudPanel();

		if ( CurrentHudPanel is not null )
			CurrentHudPanel.Parent = this.RootPanel;
	}

	[ClientRpc]
	public static void ShowDeathInformation( Client attacker )
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

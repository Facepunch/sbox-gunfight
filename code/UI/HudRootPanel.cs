using Sandbox.UI;

namespace Facepunch.Gunfight.UI;

public class HudRootPanel : RootPanel
{
	public static HudRootPanel Current;

	public HudRootPanel()
	{
		Current = this;

		StyleSheet.Load( "/resource/styles/hud.scss" );
		SetTemplate( "/resource/templates/hud.html" );

		// AddChild<DamageIndicator>();
		// AddChild<HitIndicator>();

		// AddChild<PickupFeed>();

		// AddChild<Chatbox>();
		// AddChild<KillFeed>();
		// Scoreboard = AddChild<Scoreboard>();
		// AddChild<GunVoiceList>();
		// AddChild<GunVoiceSpeaker>();
		// AddChild<HudHints>();
		// AddChild<HudMarkers>();
	}

	protected override void UpdateScale( Rect screenSize )
	{
		base.UpdateScale( screenSize );
	}

	[Event.Tick.Client]
	protected void TickCl()
	{
		GunfightHud.HudState = HudVisibilityState.Visible; 
		Event.Run( "gunfight.hudrender" );

		SetClass( "disable-hud", GunfightHud.HudState == HudVisibilityState.Invisible );
		SetClass( "almost-disable-hud", GunfightHud.HudState == HudVisibilityState.AlmostVisible );
	}
}

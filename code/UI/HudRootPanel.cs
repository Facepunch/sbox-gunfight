﻿using Sandbox.UI;

namespace Facepunch.Gunfight.UI;

public class HudRootPanel : RootPanel
{
	public static HudRootPanel Current;

	public Scoreboard Scoreboard { get; set; }

	public HudRootPanel()
	{
		Current = this;

		StyleSheet.Load( "/resource/styles/hud.scss" );
		SetTemplate( "/resource/templates/hud.html" );

		AddChild<DamageIndicator>();
		AddChild<HitIndicator>();

		AddChild<PickupFeed>();

		AddChild<Chatbox>();
		AddChild<KillFeed>();
		Scoreboard = AddChild<Scoreboard>();
		AddChild<GunVoiceList>();
		AddChild<GunVoiceSpeaker>();
		AddChild<HudHints>();
		AddChild<HudMarkers>();
	}

	protected override void UpdateScale( Rect screenSize )
	{
		base.UpdateScale( screenSize );
	}
}

﻿using Sandbox.UI;

namespace Facepunch.Gunfight;

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

		AddChild<InventoryBar>();
		AddChild<PickupFeed>();

		AddChild<ChatBox>();
		AddChild<KillFeed>();
		Scoreboard = AddChild<Scoreboard>();
		AddChild<VoiceList>();
	}

	protected override void UpdateScale( Rect screenSize )
	{
		base.UpdateScale( screenSize );
	}
}
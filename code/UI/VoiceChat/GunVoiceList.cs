using Sandbox;
using Sandbox.UI;
using System;
using System.Linq;

public class GunVoiceList : Panel
{
	public static GunVoiceList Current { get; internal set; }

	public GunVoiceList()
	{
		Current = this;
		StyleSheet.Load( "/UI/VoiceChat/GunVoiceList.scss" );
	}

	public void OnVoicePlayed( long steamId, float level )
	{
		var entry = ChildrenOfType<GunVoiceEntry>().FirstOrDefault( x => x.Friend.Id == steamId );
		if ( entry == null ) entry = new GunVoiceEntry( this, steamId );

		entry.Update( level );
	}
}

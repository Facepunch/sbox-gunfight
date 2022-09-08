using Sandbox;
using Sandbox.UI;

public class GunVoiceSpeaker : Label
{
	private float VoiceLevel = 0.0f;

	public GunVoiceSpeaker()
	{
		StyleSheet.Load( "/UI/VoiceChat/GunVoiceSpeaker.scss" );

		Text = "mic";
	}

	public override void Tick()
	{
		base.Tick();

		VoiceLevel = VoiceLevel.LerpTo( Voice.Level, Time.Delta * 40.0f );
		var tr = new PanelTransform();
		tr.AddScale( 1.0f.LerpTo( 1.2f, VoiceLevel ) );
		Style.Transform = tr;
		Style.Dirty();

		SetClass( "active", Voice.IsRecording );
	}
}

using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;

public class GunVoiceEntry : Panel
{
	public Friend Friend;

	readonly Label Name;
	readonly Label Speaker;
	readonly Image Avatar;

	private float TargetVoiceLevel = 0;

	RealTimeSince timeSincePlayed;

	public GunVoiceEntry( Panel parent, long steamId )
	{
		Parent = parent;

		Friend = new Friend( steamId );

		Avatar = Add.Image( "", "avatar" );
		Avatar.SetTexture( $"avatar:{steamId}" );

		Name = Add.Label( Friend.Name, "name" );

		Speaker = Add.Label( "volume_up", "speaker" );
	}

	public void Update( float level )
	{
		timeSincePlayed = 0;
		Name.Text = Friend.Name;
		TargetVoiceLevel = level;
	}

	public override void Tick()
	{
		base.Tick();

		if ( IsDeleting )
			return;

		var SpeakTimeout = 2.0f;
		var timeoutInv = 1 - (timeSincePlayed / SpeakTimeout);
		timeoutInv = MathF.Min( timeoutInv * 2.0f, 1.0f );

		if ( timeoutInv <= 0 )
		{
			Delete();
			return;
		}

		//VoiceLevel = VoiceLevel.LerpTo( TargetVoiceLevel, Time.Delta * 40.0f );
		//Style.Left =  VoiceLevel * -32.0f * timeoutInv;
		Style.Dirty();
	}
}

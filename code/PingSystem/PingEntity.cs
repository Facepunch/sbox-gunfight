namespace Facepunch.Gunfight;

public partial class PingEntity : ModelEntity, IHudMarker
{
	public PingType Type { get; set; } = PingType.Generic;

	public string PingMessage { get; set; }

	public string Message => string.IsNullOrEmpty( PingMessage ) ? GetPingMessage() : PingMessage;

	public async Task DeferredDeletion()
	{
		await GameTask.DelayRealtimeSeconds( 10f );

		Delete();
	}

	protected string GetPingMessage()
	{
		return Type switch
		{
			PingType.Enemy => "ENEMY",
			_ => "MARKER"
		};
	}

	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;

		_ = DeferredDeletion();
	}

	public override void TakeDamage( DamageInfo info )
	{
		base.TakeDamage( info );
	}

	string IHudMarker.GetClass() => "ping";

	bool IHudMarker.UpdateMarker( ref HudMarkerBuilder info )
	{
		if ( !this.IsValid() )
			return false;

		info.Text = "";
		info.Position = Position + Vector3.Up * 20f;
		info.Classes[Type.ToString()] = true;

		return true;
	}
}

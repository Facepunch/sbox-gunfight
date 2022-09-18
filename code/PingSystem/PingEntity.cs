namespace Facepunch.Gunfight;

public partial class PingEntity : ModelEntity, IHudMarker
{
	public PingType Type { get; set; } = PingType.Generic;

	public string PingMessage { get; set; }

	public string Message => string.IsNullOrEmpty( PingMessage ) ? GetPingMessage() : PingMessage;

	public async Task DeferredDeletion()
	{
		await GameTask.DelayRealtimeSeconds( PingSystem.GetLifetime( Type ) );

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

	protected bool DestroyMe()
	{
		Delete();
		return false;
	}

	bool IHudMarker.UpdateMarker( ref HudMarkerBuilder info )
	{
		if ( !this.IsValid() )
			return DestroyMe();
	
		if ( Parent.IsValid() && Parent.LifeState != LifeState.Alive )
			return DestroyMe();

		// Parent in this case is the entity we tagged
		if ( Parent.IsValid() && Parent.Parent.IsValid() )
			return DestroyMe();

		info.Text = "";
		info.Position = Position + Vector3.Up * 20f;
		info.Classes[Type.ToString()] = true;

		return true;
	}
}

namespace Facepunch.Gunfight;

public partial class GunfightPlayer
{
	[ClientRpc]
	protected void ClientRpcPing( Vector3 position, PingType pingType, Entity parent = null, Client sender = null )
	{
		var ping = new PingEntity();
		ping.Type = pingType;
		ping.Position = position;

		if ( sender.IsValid() )
		{
			ping.PingMessage = sender.Name.ToUpperInvariant();
		}

		if ( parent.IsValid() )
			ping.SetParent( parent );
	}

	protected void Ping()
	{
		var tr = GetPingTrace();

		var pingType = PingType.Generic;

		Vector3 position = tr.EndPosition;
		Entity taggedEntity = null;

		var entityTrace = tr.Entity;

		if ( entityTrace is PickupTrigger trigger )
		{
			entityTrace = trigger.Parent;
		}

		if ( entityTrace is GunfightPlayer enemy )
		{
			if ( TeamSystem.IsHostile( Team, enemy.Team ) )
			{
				pingType = PingType.Enemy;
				taggedEntity = tr.Entity;
				position = tr.Entity.EyePosition;
			}
		}

		if ( entityTrace is GunfightWeapon weapon )
		{
			pingType = PingType.Resource;
			taggedEntity = weapon;
			position = tr.Entity.Position;
		}

		if ( tr.Hit )
			ClientRpcPing( ToExtensions.Team( Client.GetTeam() ), position, pingType, taggedEntity, Client );
	}

	protected TraceResult GetPingTrace()
	{
		var tr = Trace.Ray( EyePosition, EyePosition + EyeRotation.Forward * 10000f ).WorldAndEntities().Ignore( this ).WithAnyTags( "solid", "player", "weapon", "trigger" ).Radius( 10f ).Run();

		return tr;
	}

	public void SimulatePing( Client cl )
	{
		if ( !IsServer ) return;

		if ( Input.Pressed( InputButton.Flashlight ) )
		{
			Ping();
		}
	}
}

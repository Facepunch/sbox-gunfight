namespace Facepunch.Gunfight;

[Library( "gunfight_ammo_static" )]
[HammerEntity, EditorModel( "models/gameplay/ammo_case_pallet/ammo_case_pallet.vmdl", FixedBounds = true )]
[Title( "Gunfight: Ammo Crate (Static)" ), Category( "Map Entities" ), Icon( "replay_circle_filled" )]
public partial class GunfightMapAmmoCrate : GamemodeSpecificEntity, IUse
{
	public bool IsUsable( Entity user )
	{
		return true;
	}

	public bool OnUse( Entity user )
	{
		var player = user as GunfightPlayer;
		if ( player.IsValid() )
		{
			foreach( var wpn in player.PlayerInventory.GetAll() )
			{
				player.GiveAmmo( wpn.AmmoType, player.MaxAmmo( wpn.AmmoType ) );
			}

			Sound.FromEntity( "sounds/interactions/loot_box.sound", this );

			UI.NotificationManager.AddNotification( To.Single( player ), UI.NotificationDockType.BottomMiddle, $"All of your ammo was refilled", 4 );
		}

		return false;
	}

	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/gameplay/ammo_case_pallet/ammo_case_pallet.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Static );
	}
}

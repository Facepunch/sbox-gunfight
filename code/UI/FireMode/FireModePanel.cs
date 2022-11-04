using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Facepunch.Gunfight;

[UseTemplate]
public class FireModePanel : Panel
{
	// @ref
	public Image ActiveImage { get; set; }
	// @ref
	public Panel FireModeList { get; set; }

	int weaponHash;

	protected string GetImage( FireMode firemode )
	{
		return firemode switch
		{
			FireMode.Semi => "/ui/firemode/semi.png",
			FireMode.FullAuto => "/ui/firemode/auto.png",
			FireMode.Burst => "/ui/firemode/burst.png",
			_ => "/ui/firemode/auto.png"
		};
	}

	[Event( "gunfight.firemode.changed" )]
	public void FireModeChanged( FireMode newFireMode )
	{
		var player = GunfightCamera.Target;
		var weapon = player?.CurrentWeapon;
		if ( !weapon.IsValid() || weapon.WeaponDefinition is null )
			return;

		Rebuild( weapon );
	}

	protected void Rebuild( GunfightWeapon weapon )
	{
		FireModeList.DeleteChildren( true );
		ActiveImage.SetTexture( GetImage( weapon.CurrentFireMode ) );

		foreach ( var firemode in weapon.WeaponDefinition.SupportedFireModes )
		{
			if ( firemode == weapon.CurrentFireMode ) continue;
			var img = FireModeList.Add.Image( GetImage( firemode ), "firemode" );
		}
	}

	public override void Tick()
	{
		base.Tick();

		var player = GunfightCamera.Target;
		var weapon = player?.CurrentWeapon;
		var inactive = !weapon.IsValid() || weapon.WeaponDefinition is null || player.LifeState != LifeState.Alive;

		SetClass( "active", !inactive );

		if ( inactive )
			return;	

		var hash = HashCode.Combine( player, weapon.WeaponDefinition );
		if ( weaponHash != hash )
		{
			weaponHash = hash;
			Rebuild( weapon );
		}
	}
}

using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Facepunch.Gunfight;

public class Grenades : Panel
{
	public Image Icon;
	public Label Inventory;
	public Panel GrenadeBar;
	public Grenades()
	{
		GrenadeBar = Add.Panel( "grenadebar" );
		Inventory = Add.Label( "1", "inventory" );
		Icon = Add.Image( "ui/weapons/grenade.png", "icon" );
	}

	public override void Tick()
	{
		var player = GunfightCamera.Target;
		var weapon = player?.CurrentWeapon;
		var inactive = !weapon.IsValid() || weapon.WeaponDefinition is null || player.LifeState != LifeState.Alive;
		Style.Display = inactive ? DisplayMode.None : DisplayMode.Flex;

		// TODO - Setup
	}
}

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
		Inventory = Add.Label( "[1]", "inventory" );
		Icon = Add.Image( "ui/weapons/grenade.png", "icon" );
	}

	int weaponHash;

	public override void Tick()
	{
		var player = Local.Pawn as Player;
		if ( player == null ) return;
	}
}

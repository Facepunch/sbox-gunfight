﻿using Sandbox.UI;

namespace Facepunch.Gunfight;

class InventoryIcon : Panel
{
	public ShooterWeapon Weapon;
	public Panel Icon;

	public InventoryIcon( ShooterWeapon weapon )
	{
		Weapon = weapon;
		Icon = Add.Panel( "icon" );

		AddClass( weapon.ClassName );
	}

	internal void TickSelection( ShooterWeapon selectedWeapon )
	{
		SetClass( "active", selectedWeapon == Weapon );
		SetClass( "empty", !Weapon?.IsUsable() ?? true );
	}

	public override void Tick()
	{
		base.Tick();

		if ( !Weapon.IsValid() || Weapon.Owner != Local.Pawn )
			Delete( true );
	}
}
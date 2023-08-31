using System;

namespace Facepunch.Gunfight;

public partial class GunfightPlayer
{
	[Net, Predicted] public bool IsHolstering { get; set; } = false;
	[Net, Predicted] public TimeUntil TimeUntilHolstered { get; set; } = -1;

	protected int GetSlotIndexFromInput( string action )
	{
		return action switch
		{
			"Slot1" => 0,
			"Slot2" => 1,
			"Slot3" => 2,
			"Slot4" => 3,
			"Slot5" => 4,
			_ => -1
		};
	}

	protected void TrySlotFromInput( string action )
	{
		if ( Input.Pressed( action ) )
		{
			Input.Clear( action );

			if ( Inventory.GetSlot( GetSlotIndexFromInput( action ) ) is Entity weapon )
				ActiveChildInput = weapon;
		}
	}

	public void BuildWeaponInput()
	{
		if ( Input.Pressed( "SwitchWeapon" ) )
		{
			Input.Clear( "SwitchWeapon" );

			var current = Inventory.Active;
			int index = 0;

			if ( current == Inventory.PrimaryWeapon )
			{
				index = 1;
			}

			if ( Inventory.GetSlot( index ) is Entity weapon )
			{
				ActiveChildInput = weapon;
			}
		}

		if ( Input.MouseWheel != 0 )
		{
			Inventory.SwitchActiveSlot( Input.MouseWheel, true );
		}
	}
}

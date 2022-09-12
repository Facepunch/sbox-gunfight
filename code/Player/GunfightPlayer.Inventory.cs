namespace Facepunch.Gunfight;

public partial class GunfightPlayer
{
	protected int GetSlotIndexFromInput( InputButton slot )
	{
		return slot switch
		{
			InputButton.Slot1 => 0,
			InputButton.Slot2 => 1,
			InputButton.Slot3 => 2,
			InputButton.Slot4 => 3,
			InputButton.Slot5 => 4,
			_ => -1
		};
	}

	protected void TrySlotFromInput( InputBuilder input, InputButton slot )
	{
		if ( Input.Pressed( slot ) )
		{
			input.SuppressButton( slot );

			if ( Inventory.GetSlot( GetSlotIndexFromInput( slot ) ) is Entity weapon )
				input.ActiveChild = weapon;
		}
	}

	public override void BuildInput( InputBuilder input )
	{

		TrySlotFromInput( input, InputButton.Slot1 );
		TrySlotFromInput( input, InputButton.Slot2 );
		TrySlotFromInput( input, InputButton.Slot3 );
		TrySlotFromInput( input, InputButton.Slot4 );
		TrySlotFromInput( input, InputButton.Slot5 );

		if ( OverrideViewAngles )
		{
			OverrideViewAngles = false;
			input.ViewAngles = NewViewAngles;
		}
		
		base.BuildInput( input );
	}
}

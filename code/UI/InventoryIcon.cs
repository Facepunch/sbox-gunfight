using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Facepunch.Gunfight;

class InventoryIcon : Panel
{
	public GunfightWeapon Weapon;
	public Image Icon;
	public Label Value;
	public Label Hint;

	// TODO - Do this in Inventory
	public InputButton ButtonFromSlot( WeaponSlot slot )
	{
		return slot switch
		{
			WeaponSlot.Primary => InputButton.Slot1,
			WeaponSlot.Secondary => InputButton.Slot2,
			_ => InputButton.Slot4
		};
	}

	public InventoryIcon( GunfightWeapon weapon )
	{
		Weapon = weapon;
		Icon = Add.Image( "", "icon" );
		Value = Add.Label( $"0", "ammocount");
		Hint = AddChild<Label>( "hint" );
		Hint.Text = Input.GetButtonOrigin( ButtonFromSlot( weapon.Slot ) );
		AddClass( weapon.ClassName );
	}

	public override void Tick()
	{
		base.Tick();

		if( !Weapon.IsValid || Weapon.Owner != Local.Pawn )
		{
			Delete( true );
			return;
		}

		var active = Weapon.Owner is GunfightPlayer pl && pl.ActiveChild == Weapon;
		var empty = !Weapon.IsUsable();

		SetClass( "active", active );
		SetClass( "empty", empty );
		Icon.SetTexture( Weapon.GunIcon.ToString() );
		if ( Weapon.WeaponDefinition.AmmoType != AmmoType.None )
		{
			Value.Text = $"{Weapon.AvailableAmmo()}";
		}
		else
		{
			Value.Text = "";
		}	
	}
}

using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Facepunch.Gunfight;

class InventoryIcon : Panel
{
	public GunfightWeapon Weapon;
	public Image Icon;
	public Label Value;
	public InputHint Hint;

	public InputButton FromBucket( int bucket )
	{
		return bucket switch
		{
			0 => InputButton.Slot1,
			1 => InputButton.Slot2,
			2 => InputButton.Slot3,
			3 => InputButton.Slot4,
			4 => InputButton.Slot5,
			5 => InputButton.Slot6,
			_ => InputButton.Slot0,
		};
	}

	public InventoryIcon( GunfightWeapon weapon )
	{
		Weapon = weapon;
		Icon = Add.Image( "", "icon" );
		Value = Add.Label( $"0", "ammocount");
		Hint = AddChild<InputHint>( "hint" );
		Hint.SetButton( FromBucket( (int)weapon.Slot ) );

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

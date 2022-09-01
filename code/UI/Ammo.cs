using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Facepunch.Gunfight;

public class Ammo : Panel
{
	public Label Inventory;
	public Panel AmmoBar;

	List<Panel> BulletPanels = new List<Panel>();

	public Ammo()
	{
		AmmoBar = Add.Panel( "ammobar" );
		Inventory = Add.Label( "100", "inventory" );
	}

	int weaponHash;

	public override void Tick()
	{
		var player = Local.Pawn as Player;
		if ( player == null ) return;

		var weapon = player.ActiveChild as GunfightWeapon;
		SetClass( "active", weapon != null );

		if ( weapon == null ) return;

		var inv = weapon.AvailableAmmo();
		Inventory.Text = $"{inv}";
		Inventory.SetClass( "active", inv >= 0 );

		var hash = HashCode.Combine( player, weapon.WeaponDefinition );
		if ( weaponHash != hash )
		{
			weaponHash = hash;
			RebuildAmmoBar( weapon );
		}

		UpdateAmmoBar( weapon );
	}

	void RebuildAmmoBar( GunfightWeapon weapon )
	{
		AmmoBar.DeleteChildren( true );
		BulletPanels.Clear();

		for ( int i = 0; i < weapon.ClipSize; i++ )
		{
			var bullet = AmmoBar.Add.Panel( "bullet" );
			BulletPanels.Add( bullet );
		}
	}

	void UpdateAmmoBar( GunfightWeapon weapon )
	{
		for ( int i = 0; i < BulletPanels.Count; i++ )
		{
			BulletPanels[i].SetClass( "empty", i >= weapon.AmmoClip );
		}
	}
}

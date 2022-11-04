using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Facepunch.Gunfight;

[UseTemplate]
public class Ammo : Panel
{
	// @ref
	public Image Icon { get; set; }
	// @ref
	public Label Inventory { get; set; }
	// @ref
	public Label CurrentAmmo { get; set; }
	// @ref
	public Label GunName { get; set; }
	// @ref
	public Panel AmmoBar { get; set; }

	List<Panel> BulletPanels = new List<Panel>();

	public Ammo()
	{
	}

	int weaponHash;

	public override void Tick()
	{
		var player = GunfightCamera.Target;
		var weapon = player?.ActiveChild as GunfightWeapon;
		SetClass( "active", weapon != null && player.LifeState == LifeState.Alive );

		if ( weapon == null ) return;

		var inv = weapon.AvailableAmmo();
		var current = weapon.AmmoClip;
		if ( weapon.WeaponDefinition.AmmoType == AmmoType.None )
		{
			CurrentAmmo.Text = "";
			Inventory.Text = "";
			Inventory.SetClass( "active", inv >= 0 );
		}
		else
		{
			CurrentAmmo.Text = $"{current}";
			Inventory.Text = $"{inv}";
			Inventory.SetClass( "active", inv >= 0 );
		}
		GunName.Text = weapon.Name;


		Icon.SetTexture( weapon.GunIcon.ToString() );

		SetClass( "low", weapon.IsLowAmmo() );

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
			BulletPanels[i].SetClass( "anim", i >= weapon.AmmoClip );
		}
	}
}

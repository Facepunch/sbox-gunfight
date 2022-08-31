﻿using Sandbox.UI;
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

		var weapon = player.ActiveChild as ShooterWeapon;
		SetClass( "active", weapon != null );

		if ( weapon == null ) return;

		var inv = weapon.AvailableAmmo();
		Inventory.Text = $"{inv}";
		Inventory.SetClass( "active", inv >= 0 );

		var hash = HashCode.Combine( player, weapon );
		if ( weaponHash != hash )
		{
			weaponHash = hash;
			RebuildAmmoBar( weapon );
		}

		UpdateAmmoBar( weapon );
	}

	void RebuildAmmoBar( ShooterWeapon weapon )
	{
		AmmoBar.DeleteChildren( true );
		BulletPanels.Clear();

		AmmoBar.SetClass( "is-crossbow", weapon is Crossbow );
		AmmoBar.SetClass( "is-shotgun", weapon is R870 );
		AmmoBar.SetClass( "is-smg", weapon is MP5 );

		for ( int i = 0; i < weapon.ClipSize; i++ )
		{
			var bullet = AmmoBar.Add.Panel( "bullet" );
			BulletPanels.Add( bullet );
		}
	}

	void UpdateAmmoBar( ShooterWeapon weapon )
	{
		for ( int i = 0; i < BulletPanels.Count; i++ )
		{
			BulletPanels[i].SetClass( "empty", i >= weapon.AmmoClip );
		}
	}
}

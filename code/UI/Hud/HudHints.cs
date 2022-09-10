using Sandbox.UI;

namespace Facepunch.Gunfight;

[UseTemplate]
public partial class HudHints : Panel
{
	// @ref
	public Panel ReloadHint { get; set; }
	// @ref
	public Panel VaultHint { get; set; }


	public override void Tick()
	{
		var player = Local.Pawn as GunfightPlayer;
		if ( !player.IsValid() ) return;

		var controller = player.Controller as PlayerController;

		VaultHint.SetClass( "show", controller.Vault.CanActivate( false ) );

		var weapon = player.ActiveChild as GunfightWeapon;
		if ( !weapon.IsValid() ) return;

		ReloadHint.SetClass( "show", weapon.IsLowAmmo() && !weapon.IsReloading );
	}
}

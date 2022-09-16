using Sandbox.UI;

namespace Facepunch.Gunfight;

[UseTemplate]
public partial class HudHints : Panel
{
	// @ref
	public Panel ReloadHint { get; set; }
	// @ref
	public Panel VaultHint { get; set; }
	// @ref
	public Panel CoverAimHint { get; set; }
	// @ref
	public Panel PickupHint { get; set; }
	// @ref
	public Label PickupLabel { get; set; }

	public override void Tick()
	{
		var player = Local.Pawn as GunfightPlayer;
		if ( !player.IsValid() ) return;

		var controller = player.Controller as PlayerController;
		if ( controller == null ) return;

		VaultHint.SetClass( "show", controller.Vault.CanActivate( false ) && !controller.Vault.IsActive );

		var weapon = player.ActiveChild as GunfightWeapon;
		if ( !weapon.IsValid() ) return;

		ReloadHint.SetClass( "show", weapon.IsLowAmmo() && !weapon.IsReloading );

		CoverAimHint.SetClass( "show", controller.CoverAim.CanMountWall() && !controller.CoverAim.IsActive );

		var cover = CoverAimHint;
		var screenPos = controller.CoverAim.MountWorldPosition.ToScreen();
		cover.Style.Left = Length.Fraction( screenPos.x );
		cover.Style.Top = Length.Fraction( screenPos.y );

		var tr = Trace.Ray( player.EyePosition, player.EyePosition + player.EyeRotation.Forward * 100000f )
			.WorldAndEntities()
			.WithAnyTags( "solid", "weapon" )
			.Run();

		if ( tr.Distance > 128f )
		{
			PickupHint.SetClass( "show", false );
		}
		else
		{
			PickupHint.SetClass( "show", tr.Hit && tr.Entity is IUse use && use.IsUsable( player ) );

			if ( tr.Entity.IsValid() && tr.Entity is GunfightWeapon wpn )
				PickupLabel.Text = wpn.Name;
		}
	}
}

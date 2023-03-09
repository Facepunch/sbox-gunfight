using Sandbox.Component;
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
	// @ref
	public Panel SpectatorHint { get; set; }
	// @text
	public string SpectatorTarget => GunfightCamera.Target?.Client?.Name ?? "nobody";

	Entity lastObserved;
	protected Entity LastObserved
	{
		get => lastObserved;
		set
		{
			if ( lastObserved != value && lastObserved.IsValid() )
			{
				var glow = lastObserved.Components.Get<Glow>();
				if ( glow != null )
					glow.Enabled = false;
			}

			lastObserved = value;

			if ( lastObserved.IsValid() )
			{
				var glow = lastObserved.Components.GetOrCreate<Glow>();
				glow.Enabled = true;
				glow.Width = 0.25f;
				glow.Color = new Color( 255, 207, 38, 0.001f );
				glow.ObscuredColor = new Color( 225, 170, 38, 0.0001f );
			}
		}
	}

	public override void Tick()
	{
		var player = Game.LocalPawn as GunfightPlayer;

		SpectatorHint.SetClass( "active", GunfightCamera.Target != Game.LocalPawn );

		if ( !player.IsValid() ) 
			return;

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

		var tr = Trace.Ray( Camera.Position, Camera.Position + Camera.Rotation.Forward * 100000f )
			.WorldAndEntities()
			.WithAnyTags( "solid", "weapon" )
			.Run();


		var isUsable = tr.Hit && tr.Entity is IUse use && use.IsUsable( player ) && tr.Distance < 128f;

		LastObserved = isUsable ? tr.Entity : null;
		PickupHint.SetClass( "show", isUsable );

		if ( isUsable )
		{
			if ( tr.Entity.IsValid() && tr.Entity is GunfightWeapon wpn )
				PickupLabel.Text = wpn.Name;
			else
				PickupLabel.Text = "Use";
		}
	}
}

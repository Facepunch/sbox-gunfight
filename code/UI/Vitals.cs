using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Facepunch.Gunfight;

public class HealthHud : Panel
{
	public IconPanel Icon;
	public HudBar Value;

	public HealthHud()
	{
		Icon = Add.Icon( "add_box", "icon" );
		Value = AddChild<HudBar>( "health" );
	}

	public override void Tick()
	{
		var player = Local.Pawn as GunfightPlayer;
		if ( player == null ) return;
		Value.Text.Text = $"{player.Health.CeilToInt()}";
		Value.InnerBar.Style.Width = Length.Fraction( Math.Max( player.Health / player.MaxHealth, 0.05f ) );
		Value.InnerBar.Style.Dirty();


		SetClass( "low", player.Health < 40.0f );
		SetClass( "empty", player.Health <= 0.0f );
		SetClass( "regen", player.IsRegen);
	}
}

public class ArmourHud : Panel
{
	public IconPanel Icon;
	public HudBar Value;

	public ArmourHud()
	{
		Icon = Add.Icon( "shield", "icon" );
		Value = AddChild<HudBar>( "armour" );
	}

	public override void Tick()
	{
		var player = Local.Pawn as GunfightPlayer;
		if ( player == null ) return;

		Value.Text.Text = $"{player.Armour.CeilToInt()}";
		Value.InnerBar.Style.Width = Length.Fraction( Math.Max( player.Armour / 100, 0.05f ) );
		Value.InnerBar.Style.Dirty();

		SetClass( "low", player.Armour < 40.0f );
		SetClass( "empty", player.Armour <= 0.0f );
	}
}

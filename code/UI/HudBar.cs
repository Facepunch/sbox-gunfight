using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Facepunch.Gunfight;
public class HudBar : Panel
{
	public Panel InnerBar;
	public Panel OuterBar;
	public Panel Icon;
	public Label Text;

	public HudBar()
	{
		StyleSheet.Load( "Resource/styles/HudBar.scss" );

		OuterBar = Add.Panel( "outerBar" );
		InnerBar = OuterBar.Add.Panel( "innerBar" );
		Icon = Add.Panel( "icon" );
		Text = Add.Label( "0", "text" );
	}
}

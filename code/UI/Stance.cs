using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Facepunch.Gunfight;

public class Stance : Panel
{
	public Image Icon;
	
	protected GunfightPlayer Player => Local.Pawn as GunfightPlayer;
	protected PlayerController PlayerController => Player?.Controller as PlayerController;
	
	public Stance()
	{
		Icon = Add.Image( "ui/stance/stand.png", "icon" );
	}

	public override void Tick()
	{
		var player = Local.Pawn as GunfightPlayer;
		if ( player == null ) return;
		if ( PlayerController == null ) return;
		if ( PlayerController.Duck.IsActive )
		{
			Icon.SetTexture( "ui/stance/duck.png" );
		}
		else if ( PlayerController.IsSprinting )
		{
			Icon.SetTexture( "ui/stance/run.png" );
		}
		else
		{
			Icon.SetTexture( "ui/stance/stand.png" );
		}
	}
}

using Sandbox.UI;
using System.ComponentModel.DataAnnotations;

namespace Facepunch.Gunfight;

[Display( Name = "Gunfight Gamemode" )]
public partial class GunfightGamemode : GamemodeEntity
{
	public override Panel GetHudPanel() => new GunfightGamemodePanel();
}

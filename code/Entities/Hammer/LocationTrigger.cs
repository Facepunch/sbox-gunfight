using System.ComponentModel.DataAnnotations;

namespace Facepunch.Gunfight;

/// <summary>
/// When the player is inside the trigger it will display the location on the hud. It will fall back to the map name.
/// </summary>
[Library( "GF_LocationTrigger")]
[AutoApplyMaterial( "materials/tools/toolsgeneric.vmat" )]
[RenderFields]
[Display( Name = "Location Trigger", GroupName = "Platformer", Description = "When the player is inside the trigger it will display the location on the hud."), Category( "Triggers" ), Icon( "follow_the_signs" )]
[HammerEntity]
internal partial class LocationTrigger : BaseTrigger
{
	/// <summary>
	/// Name of the location.
	/// </summary>
	[Property( "locationname", Title = "Location Name" )]
	public string LocationName { get; set; } = "";

	public override void Spawn()
	{
		base.Spawn();
		
		EnableTouchPersists = true;
	}
	public override void Touch( Entity other )
	{
		base.Touch( other );

		if ( !Game.IsServer ) return;
		if ( other is not GunfightPlayer pl ) return;
		pl.PlayerLocation = LocationName;
	}
}

namespace Facepunch.Gunfight;

[Library( "laser" )]
public partial class LaserSightAttachment : BarrelAddonAttachment
{
	public override Model AttachmentModel => Model.Load( "models/attachments/laser/laser_sight.vmdl" );
	public override string AimAttachment => "aim";
}

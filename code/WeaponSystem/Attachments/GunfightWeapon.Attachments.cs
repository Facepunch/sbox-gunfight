namespace Facepunch.Gunfight;

public partial class GunfightWeapon
{
	public WeaponAttachmentComponent[] Attachments => Components.GetAll<WeaponAttachmentComponent>( true ).OrderBy( x => x.Priority ).ToArray();

	/// <summary>
	/// Give / Take an attachment off this weapon
	/// </summary>
	/// <param name="identifier"></param>
	/// <param name="active"></param>
	public void SetAttachment( string identifier, bool active )
	{
		Game.AssertServer();

		Log.Info( $"Trying to SetAttachment( {identifier}, {active} )" );

		var attachment = WeaponAttachment.Get( identifier );
		if ( attachment == null )
		{
			Log.Warning( $"Tried to run SetAttachemnt( {identifier} ) but failed to find a type" );
			return;
		}

		var att = Attachments.FirstOrDefault( x => x.Identifier == identifier );
		var component = Components.GetAll<WeaponAttachmentComponent>().FirstOrDefault( x => x.Identifier == identifier );

		if ( active )
		{
			if ( component != null ) return;
			
			component = new WeaponAttachmentComponent() { AttachmentIdentifier = identifier };
			component.OnAttachmentChanged( null, identifier );

			Components.Add( component );

			Log.Info( $"Added attachment {identifier} to {this}" );
		}
		else
		{
			component?.Remove();
		}
	}

	/// <summary>
	/// Do we have this attachment?
	/// </summary>
	/// <param name="identifier"></param>
	/// <returns></returns>
	public bool HasAttachment( string identifier )
	{
		return Attachments.FirstOrDefault( x => x.Identifier == identifier ) != null;
	}
}

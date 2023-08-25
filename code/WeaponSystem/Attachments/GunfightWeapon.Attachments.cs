namespace Facepunch.Gunfight;

public partial class GunfightWeapon
{
	public WeaponAttachmentComponent[] Attachments => Components.GetAll<WeaponAttachmentComponent>( true ).OrderByDescending( x => x.Priority ).ToArray();

	public void SetAttachment( string identifier, bool active )
	{
		var desc = TypeLibrary.GetType( identifier );
		if ( desc is not null )
		{
			var att = Attachments.FirstOrDefault( x => x.Identifier == identifier );

			if ( active )
			{
				// Don't add the same attachment twice
				if ( att != null ) return;

				var component = TypeLibrary.Create<WeaponAttachmentComponent>( identifier );
				Components.Add( component );
			}
			else
			{
				att?.Remove();
			}

		}
		else
		{
			Log.Warning( $"Tried to run SetAttachemnt( {identifier} but failed to find a type" );
		}
	}

	/// <summary>
	/// Do we ahve this attachment?
	/// </summary>
	/// <param name="identifier"></param>
	/// <returns></returns>
	public bool HasAttachment( string identifier )
	{
		return Attachments.FirstOrDefault( x => x.Identifier == identifier ) != null;
	}
}

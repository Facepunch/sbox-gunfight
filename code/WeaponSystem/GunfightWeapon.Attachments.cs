namespace Facepunch.Gunfight;

public partial class GunfightWeapon
{
	/// <summary>
	/// A list of current attachments for a weapon. Should be accessible in both client and server realm.
	/// </summary>
	public List<WeaponAttachment> Attachments { get; protected set; } = new();

	public bool CanAddAttachment( WeaponAttachment attachment )
	{
		// Iterate through current attachments, to see if we have one of the same type already. If we do, bail.
		var sameTypeAttachment = Attachments.FirstOrDefault( x => x.AttachmentType == attachment.AttachmentType );
		if ( sameTypeAttachment.IsValid() ) 
			return false;

		return true;
	}

	/// <summary>
	/// Adds an attachment to a weapon.
	/// </summary>
	/// <param name="attachment"></param>
	public void AddAttachment( WeaponAttachment attachment )
	{
		// If we can't add the attachment to the weapon, delete it.
		if ( !CanAddAttachment( attachment ) )
		{
			attachment.Delete();
			return;
		}

		Attachments.Add( attachment );
	}

	/// <summary>
	/// Create and add attachment to this weapon.
	/// </summary>
	/// <param name="className"></param>
	public void CreateAttachment( string className )
	{
		var attachment = TypeLibrary.Create<WeaponAttachment>( className );
		Log.Info( $"Created weapon attachemnt {attachment}" );
		attachment.Attach( this );
	}

	/// <summary>
	/// Removes an attachment from a weapon, also deleting the entity.
	/// </summary>
	/// <param name="attachment"></param>
	public void RemoveAttachment( WeaponAttachment attachment )
	{
		Attachments.Remove( attachment );

		if ( IsServer )
		{
			// Delete the attachment as we no longer care about it.
			attachment.Delete();
		}
	}

	// Entity Virtuals
	public override void OnChildAdded( Entity child )
	{
		if ( child is not WeaponAttachment attachment )
			return;

		AddAttachment( attachment );
	}

	public override void OnChildRemoved( Entity child )
	{
		if ( child is not WeaponAttachment attachment )
			return;

		RemoveAttachment( attachment );
	}

	public void SimulateAttachments( Client cl )
	{
		foreach ( var attachment in Attachments )
		{
			attachment.Simulate( cl );
		}
	}

	public void FrameSimulateAttachments( Client cl )
	{
		foreach ( var attachment in Attachments )
		{
			attachment.FrameSimulate( cl );
		}
	}
}

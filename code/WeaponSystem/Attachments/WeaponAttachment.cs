namespace Facepunch.Gunfight;

public partial class WeaponAttachment : AnimatedEntity
{
	/// <summary>
	/// Accessor to grab the weapon as its correct type.
	/// </summary>
	public GunfightWeapon Weapon => Parent as GunfightWeapon;

	/// <summary>
	/// The model that will be used upon spawning.
	/// </summary>
	public virtual Model AttachmentModel => null;

	/// <summary>
	/// Accessor to grab the attachment's type, classes inheriting this type will need to override this property.
	/// </summary>
	public virtual AttachmentType AttachmentType => AttachmentType.Default;

	/// <summary>
	/// Attach this attachment onto a weapon.
	/// This is the best place to attach an attachment onto a weapon.
	/// </summary>
	/// <param name="weapon"></param>
	public void Attach( GunfightWeapon weapon )
	{
		SetParent( weapon, AttachmentSystem.GetModelAttachment( AttachmentType ) );
	}

	public override void Spawn()
	{
		base.Spawn();

		if ( AttachmentModel != null )
		{
			Model = AttachmentModel;
		}
	}

	public override string ToString()
	{
		return $"GunfightAttachment[{DisplayInfo.For( this ).ClassName}]";
	}
}

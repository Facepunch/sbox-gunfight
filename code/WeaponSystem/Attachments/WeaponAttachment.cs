namespace Facepunch.Gunfight;

public enum AimAttachmentStyle
{
	OnViewModel,
	OnAttachment
}

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

	public AnimatedEntity EffectEntity => ViewModelEntity.IsValid() ? ViewModelEntity : this;

	/// <summary>
	/// Attachments can override the current aim attachment
	/// </summary>
	public virtual string AimAttachment => "";

	/// <summary>
	/// Informs the aiming system on which entity to find an attachment
	/// </summary>
	public virtual AimAttachmentStyle AimAttachmentStyle => AimAttachmentStyle.OnAttachment;

	/// <summary>
	/// Attach this attachment onto a weapon.
	/// This is the best place to attach an attachment onto a weapon.
	/// </summary>
	/// <param name="weapon"></param>
	public void Attach( GunfightWeapon weapon )
	{
		SetParent( weapon, AttachmentSystem.GetModelAttachment( AttachmentType ), new Transform().WithScale( 1f ) );
	}

	public override void Spawn()
	{
		base.Spawn();

		if ( AttachmentModel != null )
		{
			Model = AttachmentModel;
		}

		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;
	}

	public override string ToString()
	{
		return $"GunfightAttachment[{DisplayInfo.For( this ).ClassName}]";
	}

	protected AnimatedEntity ViewModelEntity { get; set; }
	public void Mirror( ViewModel viewModel )
	{
		var mdl = new AnimatedEntity();
		mdl.CopyFrom( this );
		mdl.SetParent( viewModel, AttachmentSystem.GetModelAttachment( AttachmentType ), new Transform().WithScale( 1f ) );
		mdl.EnableViewmodelRendering = true;

		Log.Info( $"Created ViewModel Mirror for {this}" );

		ViewModelEntity = mdl;
	}
}

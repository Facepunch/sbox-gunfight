namespace Facepunch.Gunfight;

public partial class WeaponAttachmentComponent : EntityComponent<GunfightWeapon>
{
	[Net, Change( nameof( OnAttachmentChanged ) )] public string AttachmentIdentifier { get; set; }

	public WeaponAttachment Attachment { get; set; }
	public string Identifier => Attachment?.Identifier;
	public int Priority => Attachment?.Priority ?? 0;
	public bool IsSupported() => Attachment.IsSupported( Entity.ShortName.ToLower() );

	internal void OnAttachmentChanged( string before, string after )
	{
		Attachment = WeaponAttachment.Get( after );

		if ( Attachment is null )
		{
			Log.Warning( "Attachment is null ??" );
			return;
		}
	}

	protected void Update()
	{
		if ( Game.IsClient && Entity.ViewModelEntity.IsValid() )
		{
			Attachment.SetupViewModel( Entity.ViewModelEntity );
		}

		if ( Game.IsServer )
		{
			Log.Info( Attachment );
			Attachment.SetupWorldModel( Entity );
		}
	}

	protected override void OnActivate()
	{
		Update();
	}
}

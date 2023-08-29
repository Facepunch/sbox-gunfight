namespace Facepunch.Gunfight;

public partial class WeaponAttachment
{
	public WeaponAttachment()
	{
		//
	}

	/// <summary>
	///  A list of required attachments to be able to attach this component to a weapon
	/// </summary>
	public string[] RequiredAttachments { get; set; }

	/// <summary>
	/// Accessor for this component's identifier
	/// </summary>
	public virtual string Identifier { get; set; }

	/// <summary>
	/// UI Category for this attachment
	/// </summary>
	public virtual string Category { get; set; }

	/// <summary>
	/// Priority of this attachment, mainly for replacing weapon sounds and shit like that
	/// </summary>
	public virtual int Priority { get; set; }

	/// <summary>
	/// Progression: Is this attachment unlocked?
	/// </summary>
	/// <returns></returns>
	public virtual bool IsUnlocked()
	{
		return true;
	}

	/// <summary>
	/// Is this attachment supported on this weapon?
	/// </summary>
	/// <param name="wpn"></param>
	/// <returns></returns>
	public virtual bool IsSupported( string weapon )
	{
		return true;
	}

	/// <summary>
	/// Sound replacements via attachments
	/// </summary>
	/// <param name="key"></param>
	/// <returns></returns>
	public virtual string GetSound( string key )
	{
		return null;
	}

	protected virtual void OnActivate( string weapon )
	{
		//if ( Game.IsClient && Entity.ViewModelEntity.IsValid() )
		//{
		//	SetupViewModel( Entity.ViewModelEntity );
		//}

		//if ( Game.IsServer )
		//{
		//	SetupWorldModel( Entity );
		//}
	}

	public virtual void SetupSceneModel( SceneModel mdl )
	{
		//
	}

	public virtual void SetupViewModel( ViewModel vm )
	{
		//
	}

	public virtual void SetupWorldModel( GunfightWeapon wpn )
	{
		//
	}
}

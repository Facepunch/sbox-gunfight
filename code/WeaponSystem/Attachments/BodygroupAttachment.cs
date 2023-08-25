namespace Facepunch.Gunfight;

public partial class BodygroupAttachment : WeaponAttachmentComponent
{
	/// <summary>
	/// A list of bodygroups to be activated 
	/// </summary>
	public virtual Dictionary<int, int> Bodygroups { get; set; }

	/// <summary>
	/// A list of bodygroups to be activated on the ViewModel
	/// Is ignored if InheritBodygroups = false
	/// </summary>
	public virtual Dictionary<int, int> ViewModelBodygroups { get; set; }

	/// <summary>
	/// Disregard ViewModelBodyGroups, just set Bodygroups on both the Weapon and its ViewModel
	/// </summary>
	public virtual bool InheritBodygroups { get; set; }

	/// <summary>
	/// What weapon is this for?
	/// </summary>
	public virtual string ForWeapon { get; set; }

	public override bool IsSupported( GunfightWeapon wpn )
	{
		return ForWeapon == wpn.ShortName.ToLower();
	}

	public override void SetupViewModel( ViewModel vm )
	{
		foreach ( var kv in InheritBodygroups ? Bodygroups : ViewModelBodygroups )
		{
			vm.SetBodyGroup( kv.Key, kv.Value );
		}
	}

	public override void SetupWorldModel( GunfightWeapon wpn )
	{
		foreach ( var kv in Bodygroups )
		{
			wpn.SetBodyGroup( kv.Key, kv.Value );
		}
	}

	protected override void OnDeactivate()
	{
		foreach ( var kv in Bodygroups )
		{
			Entity?.SetBodyGroup( kv.Key, 0 );
		}
	}
}

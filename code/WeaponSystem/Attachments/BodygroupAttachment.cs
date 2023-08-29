namespace Facepunch.Gunfight;

public partial class BodygroupAttachment : WeaponAttachment
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

	public override bool IsSupported( string weapon )
	{
		return ForWeapon == weapon;
	}

	public override void SetupViewModel( ViewModel vm )
	{
		foreach ( var kv in InheritBodygroups ? Bodygroups : ViewModelBodygroups )
		{
			Log.Info( $"Setting bodygroup: {kv.Key}, {kv.Value}" );

			vm.SetBodyGroup( kv.Key, kv.Value );
		}
	}

	public override void SetupWorldModel( GunfightWeapon wpn )
	{
		foreach ( var kv in Bodygroups )
		{
			Log.Info( $"Setting bodygroup: {kv.Key}, {kv.Value}" );
			wpn.SetBodyGroup( kv.Key, kv.Value );
		}
	}
}

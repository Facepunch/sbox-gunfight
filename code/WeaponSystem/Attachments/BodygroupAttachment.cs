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
	/// Replace this 
	/// </summary>
	public virtual Dictionary<string, int> SceneModelBodygroups { get; set; }

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
		var bodygroups = InheritBodygroups ? Bodygroups : ViewModelBodygroups;
		if ( bodygroups is null ) return;

		foreach ( var kv in bodygroups )
		{
			Log.Info( $"Setting bodygroup: {kv.Key}, {kv.Value}" );
			vm.SetBodyGroup( kv.Key, kv.Value );
		}
	}

	public override void SetupWorldModel( GunfightWeapon wpn )
	{
		if ( Bodygroups is null ) return;

		foreach ( var kv in Bodygroups )
		{
			Log.Info( $"Setting bodygroup: {kv.Key}, {kv.Value}" );
			wpn.SetBodyGroup( kv.Key, kv.Value );
		}
	}

	public override void SetupSceneModel( SceneModel mdl )
	{
		if ( SceneModelBodygroups is null ) return;

		foreach ( var kv in SceneModelBodygroups )
		{
			Log.Info( $"Setting bodygroup: {kv.Key}, {kv.Value}" );
			mdl.SetBodyGroup( kv.Key, kv.Value );
		}

		mdl.Update( 0.1f );
	}
}

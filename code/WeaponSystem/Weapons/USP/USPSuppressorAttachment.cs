using System.ComponentModel.DataAnnotations;

namespace Facepunch.Gunfight;

[Library( "usp_sd" )]
public partial class USPSuppressorAttachment : BodygroupAttachment
{
	public override string ForWeapon => "usp";

	public override Dictionary<int, int> Bodygroups => new()
	{
		// USP Barrel
		{ 2, 2 }
	};

	/// <summary>
	/// Inherit bodygroups to the ViewModel
	/// </summary>
	public override bool InheritBodygroups => true;

	public override string GetSound( string key )
	{
		if ( key == "fire" ) return "usp_sd";

		return base.GetSound( key );
	}

	public override void SetupViewModel( ViewModel vm )
	{
		base.SetupViewModel( vm );

		if ( !Entity.HasAttachment( "usp_rmr" ) )
		{
			// Raised Ironsights to compensate for Suppressor
			vm.SetBodyGroup( 4, 1 );
		}
	}

	protected override void OnDeactivate()
	{
		Entity.ViewModelEntity?.SetBodyGroup( 4, 0 );
	}
}

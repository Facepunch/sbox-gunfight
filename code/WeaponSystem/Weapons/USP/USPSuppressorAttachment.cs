﻿using System.ComponentModel.DataAnnotations;

namespace Facepunch.Gunfight;

public partial class USPSuppressorAttachment : BodygroupAttachment
{
	public override string ForWeapon => "usp";
	public override string Identifier => "usp_sd";

	public override Dictionary<int, int> Bodygroups => new()
	{
		// USP Barrel
		{ 2, 2 },
		// Raised Ironsights
		{ 4, 1 }
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
}

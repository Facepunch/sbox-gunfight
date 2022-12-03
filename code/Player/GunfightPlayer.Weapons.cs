namespace Facepunch.Gunfight;

public partial class GunfightPlayer
{
	/// <summary>
	/// Accessor to grab current weapon as a GunfightWeapon
	/// </summary>
	public GunfightWeapon CurrentWeapon => ActiveChild as GunfightWeapon;

	/// <summary>
	/// Can we start changing weapons?
	/// </summary>
	/// <param name="cl"></param>
	/// <returns></returns>
	public bool CanChangeWeapon( Client cl )
	{
		if ( IsHolstering ) return false;

		var wpn = CurrentWeapon;
		if ( wpn != null )
		{
			if ( wpn.IsBurstFiring ) return false;
		}

		return true;
	}

	/// <summary>
	/// When trying to switch weapon, we'll mark the new target weapon as the queued child
	/// Then, when we're ready to switch - only then will we set ActiveChild
	/// </summary>
	protected Entity QueuedActiveChild;

	protected void SimulateWeapons( Client cl )
	{
		//
		// Input requested a weapon switch
		//
		if ( Input.ActiveChild != null && ActiveChild != Input.ActiveChild )
		{
			QueuedActiveChild = Input.ActiveChild;
		}

		// Start the holstering procedure
		if ( QueuedActiveChild.IsValid() && CanChangeWeapon( cl ) )
		{
			// Perform holster on weapon
			IsHolstering = true;
			TimeUntilHolstered = 0.5f;
			var wpn = ActiveChild as GunfightWeapon;
			wpn?.Holster();
		}

		// This is ran on Simulate, to check if we're ready to switch active child
		if ( IsHolstering )
		{
			if ( TimeUntilHolstered )
			{
				IsHolstering = false;
				ActiveChild = QueuedActiveChild;
				QueuedActiveChild = null;
			}
		}

		// Run this after everything, as we could've changed ActiveChild above
		SimulateActiveChild( cl, ActiveChild );
	}

	public override void FrameSimulate( Client cl )
	{
		base.FrameSimulate( cl );

		ActiveChild?.FrameSimulate( cl );
	}
}

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
	public bool CanChangeWeapon( IClient cl )
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
	protected Entity QueuedActiveChild { get; set; }

	protected void SimulateWeapons( IClient cl )
	{
		//
		// Input requested a weapon switch
		//
		if ( ActiveChildInput != null && ActiveChild != ActiveChildInput )
		{
			QueuedActiveChild = ActiveChildInput;
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

	/// <summary>
	/// This isn't networked, but it's predicted. If it wasn't then when the prediction system
	/// re-ran the commands LastActiveChild would be the value set in a future tick, so ActiveEnd
	/// and ActiveStart would get called multiple times and out of order, causing all kinds of pain.
	/// </summary>
	Entity LastActiveChild { get; set; }

	/// <summary>
	/// Simulated the active child. This is important because it calls ActiveEnd and ActiveStart.
	/// If you don't call these things, viewmodels and stuff won't work, because the entity won't
	/// know it's become the active entity.
	/// </summary>
	public virtual void SimulateActiveChild( IClient cl, Entity child )
	{
		if ( LastActiveChild != child )
		{
			OnActiveChildChanged( LastActiveChild, child );
			LastActiveChild = child;
		}

		if ( !LastActiveChild.IsValid() )
			return;

		if ( LastActiveChild.IsAuthority )
		{
			LastActiveChild.Simulate( cl );
		}
	}

	/// <summary>
	/// Called when the Active child is detected to have changed
	/// </summary>
	public virtual void OnActiveChildChanged( Entity previous, Entity next )
	{
		if ( previous is BaseCarriable previousBc )
		{
			previousBc?.ActiveEnd( this, previousBc.Owner != this );
		}

		if ( next is BaseCarriable nextBc )
		{
			nextBc?.ActiveStart( this );
		}
	}

	public override void FrameSimulate( IClient cl )
	{
		base.FrameSimulate( cl );

		PlayerCamera?.Update();
		ActiveChild?.FrameSimulate( cl );

		Camera.FieldOfView = Screen.CreateVerticalFieldOfView( Game.Preferences.FieldOfView );

		if ( !Camera.FirstPersonViewer.IsValid() )
			return;

		GunfightGame.AddedCameraFOV = 0f;
		if ( Controller != null )
		{
			if ( Controller.IsSprinting )
				GunfightGame.AddedCameraFOV += 3f;

			if ( Controller.IsAiming )
				GunfightGame.AddedCameraFOV += -10f;
		}
	}
}

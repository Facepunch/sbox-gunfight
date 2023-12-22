namespace Gunfight;

/// <summary>
/// A base for a player controller mechanic.
/// </summary>
public abstract partial class BasePlayerControllerMechanic : Component
{
	[Property] public PlayerController PlayerController { get; set; }

	/// <summary>
	/// A priority for the controller mechanic.
	/// </summary>
	[Property] public virtual int Priority { get; set; } = 0;

	private bool isActive; 

	/// <summary>
	/// Is this mechanic active?
	/// </summary>
	[Property, System.ComponentModel.ReadOnly( true )] public bool IsActive
	{
		get => isActive;
		set
		{
			var before = isActive;
			isActive = value;

			if ( isActive != before )
				OnActiveChanged( before, isActive );
		}
	}

	protected override void OnAwake()
	{
		// If we don't have the player controller defined, let's have a look for it
		if ( !PlayerController.IsValid() )
		{
			PlayerController = Components.Get<PlayerController>( FindMode.EverythingInSelfAndAncestors );
		}
	}

	/// <summary>
	/// Return a list of tags to be used by the player controller / other mechanics.
	/// </summary>
	/// <returns></returns>
	public virtual IEnumerable<string> GetTags()
	{
		return Enumerable.Empty<string>();
	}
	
	/// <summary>
	/// An accessor to see if the player controller has a tag.
	/// </summary>
	/// <param name="tag"></param>
	/// <returns></returns>
	public bool HasTag( string tag ) => PlayerController.HasTag( tag );

	/// <summary>
	/// An accessor to see if the player controller has all matched tags.
	/// </summary>
	/// <param name="tags"></param>
	/// <returns></returns>
	public bool HasAllTags( params string[] tags ) => PlayerController.HasAllTags( tags );

	/// <summary>
	/// Called when <see cref="IsActive"/> changes.
	/// </summary>
	/// <param name="before"></param>
	/// <param name="after"></param>
	protected virtual void OnActiveChanged( bool before, bool after )
	{
		//
	}

	/// <summary>
	/// Called by <see cref="PlayerController"/>, treat this like a Tick/Update.
	/// </summary>
	public virtual void UpdateMechanic()
	{
		//
	}

	/// <summary>
	/// Should we be ticking this mechanic at all?
	/// </summary>
	/// <returns></returns>
	public virtual bool ShouldUpdateMechanic()
	{
		return false;
	}

	/// <summary>
	/// Mechanics can override the player's movement speed.
	/// </summary>
	/// <returns></returns>
	public virtual float? GetSpeed()
	{
		return null;
	}

	/// <summary>
	/// Mechanics can override the player's eye height.
	/// </summary>
	/// <returns></returns>
	public virtual float? GetEyeHeight()
	{
		return null;
	}

	/// <summary>
	/// Mechanics can override the player's ground friction.
	/// </summary>
	public virtual float? GetGroundFriction()
	{
		return null;
	}
}

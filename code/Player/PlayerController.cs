namespace Gunfight;

public sealed class PlayerController : Component
{
	/// <summary>
	/// A reference to the player's body (the GameObject)
	/// </summary>
	[Property] public GameObject Body { get; set; }

	/// <summary>
	/// The current character controller for this player.
	/// </summary>
	CharacterController CharacterController { get; set; }

	protected override void OnAwake()
	{
		// Try to find the character controller.
		CharacterController = Components.Get<CharacterController>();
		// Throw a big tantrum if we can't find it, since the game can't operate without it.
		if ( CharacterController == null ) throw new ComponentNotFoundException();
	}
}

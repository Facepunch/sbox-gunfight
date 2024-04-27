namespace Gunfight;

public partial class PlayerController
{
	[RequireComponent] HealthComponent HealthComponent { get; set; }

	public void Kill()
	{
		NetDePossess();

		// TODO: Turn off the body (or a death anim)
		// Kill player inventory
	}

	public void Respawn()
	{
		// TODO: Turn on the body
		// Set up player inventory
		// Move to spawnpoint
	}
}

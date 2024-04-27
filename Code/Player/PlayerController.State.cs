namespace Gunfight;

public partial class PlayerController
{
	[RequireComponent] HealthComponent HealthComponent { get; set; }

	public void Kill()
	{
		NetDePossess();

		// TODO: Turn off the body (or a death anim)
		// Kill player inventory

		SetBodyVisible( false );
		HealthComponent.State = LifeState.Respawning;
	}

	public void SetBodyVisible( bool visible )
	{
		Body.GameObject.Enabled = visible;
	}

	public void Respawn()
	{
		// TODO: Turn on the body
		// Set up player inventory
		// Move to spawnpoint

		HealthComponent.Health = 100;
		SetBodyVisible( true );
	}
}

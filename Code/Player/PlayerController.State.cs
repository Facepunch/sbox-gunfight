namespace Gunfight;

public partial class PlayerController
{
	/// <summary>
	/// The player's health component
	/// </summary>
	[RequireComponent] HealthComponent HealthComponent { get; set; }

	/// <summary>
	/// The player's inventory, items, etc.
	/// </summary>
	[RequireComponent] PlayerInventory Inventory { get; set; }

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
		HealthComponent.Health = 100;

		Inventory.Clear();
		Inventory.Setup();

		SetBodyVisible( true );

		var spawn = GameUtils.GetSpawnPoints()
			.FirstOrDefault();

		if ( spawn.IsValid() )
		{
			Transform.Position = spawn.Transform.Position;
		}
	}
}

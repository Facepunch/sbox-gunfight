namespace Gunfight;

public static class GameObjectExtensions
{
	/// <summary>
	/// Gets a health component from a specific GameObject.
	/// </summary>
	/// <param name="go">The GameObject in question.</param>
	/// <param name="create">Should we make one if it doesn't exist?</param>
	/// <returns></returns>
	private static HealthComponent GetHealthComponent( this GameObject go, bool create = false )
	{
		if ( create ) return go.Components.GetOrCreate<HealthComponent>();
		return go.Components.Get<HealthComponent>();
	}

	/// <summary>
	/// Gets the health of a GameObject if it has a Health Component,
	/// If we don't have a Health Component - we'll make one.
	/// </summary>
	/// <param name="go"></param>
	/// <returns></returns>
	public static float GetHealth( this GameObject go )
	{
		return GetHealthComponent( go )?.Health ?? 0;
	}

	/// <summary>
	/// Sets the health of a GameObject if it has a Health Component
	/// If we don't have a Health Component - we'll make one.
	/// </summary>
	/// <param name="go"></param>
	/// <param name="newHealth"></param>
	public static void SetHealth( this GameObject go, float newHealth )
	{
		var healthComponent = GetHealthComponent( go, true );
		healthComponent.Health = newHealth;
	}
}

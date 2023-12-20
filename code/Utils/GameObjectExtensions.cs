namespace Gunfight;

public static class GameObjectExtensions
{
	private static HealthComponent GetHealthComponent( this GameObject go )
	{
		return go.Components.GetOrCreate<HealthComponent>();
	}

	/// <summary>
	/// Gets the health of a GameObject if it has a Health Component,
	/// If we don't have a Health Component - we'll make one.
	/// </summary>
	/// <param name="go"></param>
	/// <returns></returns>
	public static float GetHealth( this GameObject go )
	{
		return GetHealthComponent( go ).Health;
	}

	/// <summary>
	/// Sets the health of a GameObject if it has a Health Component
	/// If we don't have a Health Component - we'll make one.
	/// </summary>
	/// <param name="go"></param>
	/// <param name="newHealth"></param>
	public static void SetHealth( this GameObject go, float newHealth )
	{
		var healthComponent = GetHealthComponent( go );
		healthComponent.Health = newHealth;
	}
}

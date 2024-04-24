namespace Gunfight;

public static partial class GameObjectExtensions
{
	/// <summary>
	/// Inflict damage on a GameObject.
	/// </summary>
	/// <param name="go"></param>
	/// <param name="info"></param>
	public static void TakeDamage( this GameObject go, ref DamageInfo info )
	{
		foreach ( var damageable in go.Components.GetAll<Component.IDamageable>( FindMode.EnabledInSelfAndDescendants ) )
		{
			damageable.OnDamage( info );
		}
	}
}

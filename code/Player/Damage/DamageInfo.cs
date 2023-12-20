namespace Gunfight;

/// <summary>
/// A damage info struct. When inflicting damage on GameObjects, this is what we'll pass around.
/// </summary>
public struct DamageInfo
{
	public GameObject Attacker { get; set; }
	public GameObject Victim { get; set; }
	public GameObject Inflictor { get; set; }
	public float Damage { get; set; }

	/// <summary>
	/// Creates a generic DamageInfo struct.
	/// </summary>
	/// <param name="damage"></param>
	/// <param name="attacker"></param>
	/// <param name="victim"></param>
	/// <param name="inflictor"></param>
	/// <returns></returns>
	public static DamageInfo Generic( float damage, GameObject attacker = null, GameObject victim = null, GameObject inflictor = null )
	{
		return new DamageInfo
		{
			Damage = damage,
			Attacker = attacker,
			Victim = victim,
			Inflictor = inflictor
		};
	}
}

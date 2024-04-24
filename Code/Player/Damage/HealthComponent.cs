using System.Runtime.Intrinsics.X86;

namespace Gunfight;

/// <summary>
/// A health component for any kind of GameObject.
/// </summary>
public partial class HealthComponent : Component, Component.IDamageable
{
	private float health = 100f;
	private LifeState state = LifeState.Alive;

	/// <summary>
	/// An action (mainly for ActionGraphs) to respond to when a GameObject's health changes.
	/// </summary>
	[Property] public Action<float, float> OnHealthChanged { get; set; }

	/// <summary>
	/// An action to respond to when a GameObject's life state changes.
	/// </summary>
	[Property] public Action<LifeState, LifeState> OnLifeStateChanged { get; set; }

	/// <summary>
	/// What's our health?
	/// </summary>
	[Property, ReadOnly]
	public float Health
	{
		get => health;
		set
		{
			var old = health;
			if ( old == value ) return;

			health = value;
			HealthChanged( old, health );
		}
	}

	/// <summary>
	/// What's our life state?
	/// </summary>
	[Property, ReadOnly, Group( "Life State" )]
	public LifeState State
	{
		get => state;
		set
		{
			var old = state;
			if ( old == value ) return;

			state = value;
			LifeStateChanged( old, state );
		}
	}


	/// <summary>
	/// Called when Health is changed.
	/// </summary>
	/// <param name="oldValue"></param>
	/// <param name="newValue"></param>
	protected void HealthChanged( float oldValue, float newValue )
	{
		OnHealthChanged?.Invoke( oldValue, newValue );
	}

	protected void LifeStateChanged( LifeState oldValue, LifeState newValue )
	{
		OnLifeStateChanged?.Invoke( oldValue, newValue );
	}

	/// <summary>
	/// Called when this GameObject is damaged by something/someone.
	/// </summary>
	/// <param name="info"></param>
	public void OnDamage( in Sandbox.DamageInfo info )
	{
		Health -= info.Damage;
	}

	/// <summary>
	/// The component's life state.
	/// </summary>
	public enum LifeState
	{
		Alive,
		Respawning,
		Dead
	}
}

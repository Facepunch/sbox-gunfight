namespace Gunfight;

public partial class PlayerController
{
	/// <summary>
	/// Maintains a list of mechanics that are associated with this player controller.
	/// </summary>
	public IEnumerable<BasePlayerControllerMechanic> Mechanics => Components.GetAll<BasePlayerControllerMechanic>( FindMode.EnabledInSelfAndDescendants );

	float? CurrentSpeedOverride;
	float? CurrentEyeHeightOverride;
	float? CurrentFrictionOverride;

	/// <summary>
	/// Called on <see cref="OnUpdate"/>.
	/// </summary>
	protected void OnUpdateMechanics()
	{
		var sortedMechanics = Mechanics.OrderBy( x => x.Priority ).Where( x => x.ShouldUpdateMechanic() );

		float? speedOverride = null;
		float? eyeHeightOverride = null;
		float? frictionOverride = null;

		foreach ( var mechanic in sortedMechanics )
		{
			mechanic.UpdateMechanic();

			var eyeHeight = mechanic.GetEyeHeight();
			var speed = mechanic.GetSpeed();
			var friction = mechanic.GetGroundFriction();

			if ( speed is not null ) speedOverride = speed;
			if ( eyeHeight is not null ) eyeHeightOverride = eyeHeight;
			if ( friction is not null ) frictionOverride = friction;
		}

		CurrentSpeedOverride = speedOverride;
		CurrentEyeHeightOverride = eyeHeightOverride;
		CurrentFrictionOverride = frictionOverride;
	}
}

namespace Facepunch.Gunfight;

internal class GunfightCamera : CameraMode
{
	private static GunfightPlayer target;
	public static GunfightPlayer Target
	{
		get => target;
		set
		{
			if ( target == value ) return;

			var oldTarget = target;
			target = value;

			Event.Run( "gunfight.spectator.changedtarget", oldTarget, target );
		}
	}

	public static bool IsSpectator => Target.IsValid() && !Target.IsLocalPawn;
	public static bool IsLocal => !IsSpectator;

	public virtual IEnumerable<GunfightPlayer> GetPlayers()
	{
		return Entity.All.OfType<GunfightPlayer>().Where( x => x.LifeState == LifeState.Alive );
	}

	public override void Update()
	{
		if ( !Target.IsValid() )
			Target = GetPlayers().FirstOrDefault();

		var target = Target;
		if ( !target.IsValid() )
			return;

		Position = target.EyePosition;

		if ( IsLocal )
			Rotation = target.EyeRotation;
		else
			Rotation = Rotation.Slerp( Rotation, target.EyeRotation, Time.Delta * 20f );

		Viewer = target;
	}
}

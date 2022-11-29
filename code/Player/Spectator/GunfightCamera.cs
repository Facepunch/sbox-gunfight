namespace Facepunch.Gunfight;

public class GunfightCamera : CameraMode
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

	public static Transform? CameraOverride { get; set; }

	public static bool IsSpectator => Target.IsValid() && !Target.IsLocalPawn;
	public static bool IsLocal => !IsSpectator;

	public static bool IsThirdPerson { get; set; } = false;

	public virtual IEnumerable<GunfightPlayer> GetPlayers()
	{
		return Entity.All.OfType<GunfightPlayer>().Where( x => x.LifeState == LifeState.Alive );
	}

	public float CurrentDistance { get; set; } = 0f;
	public float CameraDistance => 60f;
	public float RightOffset => 4f;
	public float CameraChangeSpeed { get; set; } = 10f;

	private void UpdateDistance()
	{
		var player = Target;

		CurrentDistance = CurrentDistance.LerpTo( IsThirdPerson ? CameraDistance : 0, Time.Delta * CameraChangeSpeed );

		if ( CurrentDistance < 8 )
		{
			Viewer = player;
		}
		else
		{
			Viewer = null;
		}
	}

	public override void Update()
	{
		if ( CameraOverride != null )
		{
			Position = CameraOverride.Value.Position;
			Rotation = CameraOverride.Value.Rotation;
			Viewer = null;
			Sound.Listener = CameraOverride;

			return;
		}

		if ( !Target.IsValid() )
			Target = GetPlayers().FirstOrDefault();

		var target = Target;
		if ( !target.IsValid() )
			return;

		UpdateDistance();

		Position = Target.EyePosition;

		float distance = CurrentDistance * Target.Scale;
		var targetPos = Position + Input.Rotation.Right * ((Target.CollisionBounds.Maxs.x + RightOffset) * Target.Scale) * (CurrentDistance / CameraDistance);
		targetPos += Input.Rotation.Forward * -distance;

		Position = targetPos;

		if ( IsLocal )
			Rotation = target.EyeRotation;
		else
			Rotation = Rotation.Slerp( Rotation, target.EyeRotation, Time.Delta * 20f );

		Sound.Listener = new()
		{
			Position = Target.EyePosition,
			Rotation = Target.EyeRotation
		};
	}
}

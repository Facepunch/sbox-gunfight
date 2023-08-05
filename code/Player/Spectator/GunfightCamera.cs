namespace Facepunch.Gunfight;

public class GunfightCamera
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
	public float RightOffset => 8f;
	public float CameraChangeSpeed { get; set; } = 10f;

	private void UpdateDistance()
	{
		CurrentDistance = CurrentDistance.LerpTo( IsThirdPerson ? CameraDistance : 0, Time.Delta * CameraChangeSpeed );
		Camera.FirstPersonViewer = CurrentDistance < 8 ? Target : null;
	}

	private float LerpedEyeHeight = 64f;
	public virtual void Update()
	{
		if ( CameraOverride != null )
		{
			Camera.Position = CameraOverride.Value.Position;
			Camera.Rotation = CameraOverride.Value.Rotation;
			Camera.FirstPersonViewer = null;
			Sound.Listener = CameraOverride;

			return;
		}
		
		if ( Game.LocalPawn is GunfightPlayer player )
			Target = player;

		if ( !Target.IsValid() )
			Target = GetPlayers().FirstOrDefault();

		var target = Target;
		if ( !target.IsValid() )
			return;

		UpdateDistance();

		if ( target.Controller != null )
		{
			LerpedEyeHeight = LerpedEyeHeight.LerpTo( target.Controller.CurrentEyeHeight, Time.Delta * 10f );
			Camera.Position = Target.Position + Vector3.Up * LerpedEyeHeight;
		}

		var rotation = Rotation.LookAt( Target.AimRay.Forward );
		if ( Target.IsLocalPawn )
			rotation = Target.LookInput.ToRotation();

		float distance = CurrentDistance * Target.Scale;
		var targetPos = Camera.Position + rotation.Right * ((Target.CollisionBounds.Maxs.x + RightOffset) * Target.Scale) * (CurrentDistance / CameraDistance);
		targetPos += rotation.Forward * -distance;

		Camera.Position = targetPos;

		if ( IsLocal )
			Camera.Rotation = rotation;
		else
			Camera.Rotation = Rotation.Slerp( Camera.Rotation, rotation, Time.Delta * 20f );

		Sound.Listener = new()
		{
			Position = Camera.Position,
			Rotation = Camera.Rotation
		};
	}

	public virtual void BuildInput()
	{
		if ( Target.IsAiming )
			Input.AnalogLook *= 0.5f;

		if ( CameraOverride != null )
			Input.AnalogLook = Angles.Zero;
	}
}

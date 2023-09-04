namespace Facepunch.Gunfight;

// TODO - Set up this
public partial class GunfightDeathCamera : GunfightCamera
{
	public GunfightDeathCamera( ModelEntity killer )
	{
		FocusEntity = killer;
	}
	
	public ModelEntity FocusEntity { get; set; }
	Vector3 FocusPoint => FocusEntity?.AimRay.Position ?? Camera.Position;
	Rotation FocusRotation => Rotation.LookAt( FocusEntity?.AimRay.Forward ?? Vector3.Forward );

	public virtual Vector3 GetViewOffset()
	{
		return FocusRotation.Forward * 200f + Vector3.Down * 10f;
	}

	public override void Update()
	{
		ModelEntity focusEntity = FocusEntity ?? GunfightCamera.Target as ModelEntity;
		bool isRagdoll = false;

		if ( focusEntity is GunfightPlayer focusPlayer )
		{
			if ( focusPlayer.RagdollEntity.IsValid() )
			{
				focusEntity = focusPlayer.RagdollEntity;
				isRagdoll = true;
			}
		}

		var delta = Time.Delta * 10f;

		if ( isRagdoll )
		{
			Camera.Position = Camera.Position.LerpTo( focusEntity.PhysicsBody.Position + (focusEntity.Rotation.Forward * 200f + Vector3.Up * 15f), delta );

			Camera.Rotation = Rotation.Lerp( Camera.Rotation, Rotation.LookAt( -focusEntity.Rotation.Forward, Vector3.Up ), delta );
		}
		else
		{
			Camera.Position = Camera.Position.LerpTo( FocusPoint + GetViewOffset(), delta );
			Camera.Rotation = Rotation.Lerp( Camera.Rotation, Rotation.LookAt( -FocusRotation.Forward, Vector3.Up ), delta );
		}

		Camera.FirstPersonViewer = null;
		Camera.FieldOfView = Camera.FieldOfView.LerpTo( 55, Time.Delta * 3f );
		Camera.ZNear = 0.5f;
	}
}

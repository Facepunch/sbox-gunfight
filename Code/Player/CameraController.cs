using Sandbox;

namespace Gunfight;

public sealed class CameraController : Component
{
	/// <summary>
	/// A reference to the camera component we're going to be doing stuff with.
	/// </summary>
	[Property] public CameraComponent Camera { get; set; }

	[Property] public PlayerController Player { get; set; }

	/// <summary>
	/// Constructs a ray using the camera's GameObject
	/// </summary>
	public Ray AimRay => new Ray( Camera.Transform.Position + Camera.Transform.Rotation.Forward * 25f, Camera.Transform.Rotation.Forward );

	private float FieldOfViewOffset = 0f;
	private float TargetFieldOfView = 90f;

	public void AddFieldOfViewOffset( float degrees )
	{
		FieldOfViewOffset -= degrees;
	}

	public void SetActive( bool isActive )
	{
		Camera.GameObject.Enabled = isActive;
		ShowBodyParts( !isActive );
	}

	public void ShowBodyParts( bool show )
	{
		// Disable the player's body so it doesn't render.
		var skinnedModels = Player.Body.Components.GetAll<SkinnedModelRenderer>( FindMode.EnabledInSelfAndDescendants );

		foreach ( var skinnedModel in skinnedModels )
		{
			skinnedModel.RenderType = show ? 
				ModelRenderer.ShadowRenderType.On : 
				ModelRenderer.ShadowRenderType.ShadowsOnly;
		}
	}

	/// <summary>
	/// Updates the camera's position, from player code
	/// </summary>
	/// <param name="eyeHeight"></param>
	internal void UpdateFromEyes( float eyeHeight )
	{
		Camera.Transform.Rotation = Player.EyeAngles.ToRotation();
		Camera.Transform.LocalPosition = Vector3.Zero.WithZ( eyeHeight );
		ViewBob();
	}

	float walkBob = 0;

	Rotation lerpedRotation = Rotation.Identity;
	Vector3 lerpedPosition = Vector3.Zero;

	/// <summary>
	/// Bob the view!
	/// This could be better, but it doesn't matter really.
	/// </summary>
	void ViewBob()
	{
		var targetRotation = Rotation.Identity;
		var targetPosition = Vector3.Zero;

		var bobSpeed = Player.CharacterController.Velocity.Length.LerpInverse( 0, 300 );
		if ( !Player.IsGrounded ) bobSpeed *= 0.1f;
		if ( Player.HasTag( "slide" ) )
		{
			// Slide the camera
			targetRotation *= Rotation.FromPitch( 2f );
			targetRotation *= Rotation.FromRoll( -2f );
			targetPosition += Vector3.Right * 5f;
			targetPosition += Vector3.Down * 5f;

			bobSpeed *= 0.1f;
		}

		walkBob += Time.Delta * 10.0f * bobSpeed;

		var yaw = MathF.Sin( walkBob ) * 0.5f;
		var pitch = MathF.Cos( -walkBob * 2f ) * 0.5f;

		Camera.Transform.LocalRotation *= Rotation.FromYaw( -yaw * bobSpeed );
		Camera.Transform.LocalRotation *= Rotation.FromPitch( -pitch * bobSpeed * 0.5f );

		lerpedRotation = Rotation.Lerp( lerpedRotation, targetRotation, Time.Delta * 5f );
		lerpedPosition = lerpedPosition.LerpTo( targetPosition, Time.Delta * 5f );

		Camera.Transform.LocalRotation *= lerpedRotation;
		Camera.Transform.LocalPosition += lerpedPosition;
	}

	protected override void OnUpdate()
	{
		var baseFov = Preferences.FieldOfView;

		TargetFieldOfView = TargetFieldOfView.LerpTo( baseFov + FieldOfViewOffset, Time.Delta * 5f );
		FieldOfViewOffset = 0;
		Camera.FieldOfView = TargetFieldOfView;

		ApplyRecoil();
	}

	void ApplyRecoil()
	{
		if ( !Player.IsLocallyControlled )
		{
			return;
		}

		if ( Player.CurrentWeapon.IsValid() && Player.CurrentWeapon?.GetFunction<RecoilFunction>() is { } fn )
		{
			Player.EyeAngles += fn.Current;
		}

		if ( Player.CurrentWeapon.IsValid() && Player.CurrentWeapon?.GetFunction<SwayFunction>() is { } sFn )
		{
			Player.EyeAngles += sFn.Current;
		}
	}
}

namespace Gunfight;

public sealed class CameraController : Component
{
	/// <summary>
	/// A reference to the camera component we're going to be doing stuff with.
	/// </summary>
	[Property] public CameraComponent Camera { get; set; }

	[Property] public PlayerController PlayerController { get; set; }

	/// <summary>
	/// Constructs a ray using the camera's GameObject
	/// </summary>
	public Ray AimRay => new Ray( Camera.Transform.Position + Camera.Transform.Rotation.Forward * 25f, Camera.Transform.Rotation.Forward );

	public void SetActive( bool isActive )
	{
		Camera.Enabled = isActive;
		ShowBodyParts( !isActive );
	}

	public void ShowBodyParts( bool show )
	{
		var playerController = Components.Get<PlayerController>();
		if ( playerController == null ) throw new ComponentNotFoundException( "CameraController - couldn't find PlayerController component." );

		// Disable the player's body so it doesn't render.
		var skinnedModels = playerController.Body.Components.GetAll<SkinnedModelRenderer>( FindMode.EnabledInSelfAndDescendants );

		foreach ( var skinnedModel in skinnedModels )
		{
			skinnedModel.RenderType = ModelRenderer.ShadowRenderType.ShadowsOnly;
		}
	}

	protected override void OnUpdate()
	{
		ApplyRecoil();
	}

	void ApplyRecoil()
	{
		if ( PlayerController.CurrentWeapon.GetFunction<RecoilFunction>() is var fn )
		{
			PlayerController.EyeAngles += fn.Current;
		}
	}
}

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

	protected override void OnUpdate()
	{
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
	}
}

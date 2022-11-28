namespace Facepunch.Gunfight.War;

public partial class SpawnOverview
{
	[Event( "gunfight.hudrender.post" )]
	protected void HudRender()
	{
		GunfightHud.HudState = HudVisibilityState.Invisible;

		var cameraTarget = GamemodeMarker.WithTag( "spawn_overview" ).FirstOrDefault()?.Transform ?? new Transform()
		{
			Position = new Vector3( -495, 3154, 5436 ),
			Rotation = Rotation.From( 90, 0, 0 )
		};

		GunfightCamera.CameraOverride = cameraTarget;
	}

	public override void OnDeleted()
	{
		base.OnDeleted();
		GunfightCamera.CameraOverride = null;
	}
}

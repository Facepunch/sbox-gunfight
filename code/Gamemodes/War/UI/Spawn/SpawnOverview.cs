namespace Facepunch.Gunfight.War;

public partial class SpawnOverview
{
	public static SpawnOverview This;

	public SpawnOverview()
	{
		This = this;
	}

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
		GunfightCamera.CameraOverride = null;
		This = null;
		base.OnDeleted();
	}

	public static void Create()
	{
		if ( This != null )
		{
			Log.Info( $"SpawnOverview already exists. What? {This}" );
			return;
		}

		GunfightGame.Current.Hud.RootPanel.AddChild<SpawnOverview>();
	}

	[ClientRpc]
	public static void Send()
	{
		Create();
	}

	public override void Tick()
	{
		var localPawn = Game.LocalPawn as GunfightPlayer;
		if ( localPawn.IsValid() )
		{
			if ( localPawn.LifeState == LifeState.Alive )
			{
				Log.Info( "Delete, i'm alive!" );
				Delete();
				This = null;
			}
		}
	}
}

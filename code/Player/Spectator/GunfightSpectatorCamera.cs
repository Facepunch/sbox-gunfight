namespace Facepunch.Gunfight;

internal class GunfightSpectatorCamera : GunfightCamera
{
	[ConCmd.Admin( "gunfight_debug_togglespectator", Help = "Toggles spectator mode" )]
	public static void ToggleSpectator()
	{
		//var cl = ConsoleSystem.Caller;
		//cl.Pawn.Components.Add( new GunfightSpectatorCamera() );
	}

	public bool IsFree { get; set; } = false;

	protected virtual float BaseMoveSpeed => 800f;

	// TODO - Input modifiers
	protected float MoveMultiplier = 1f;

	int playerIndex = 0;
	public GunfightPlayer SelectPlayerIndex( int index )
	{
		var players = GetPlayers()
			.ToList();

		playerIndex = index;

		if ( playerIndex >= players.Count )
			playerIndex = 0;

		if ( playerIndex < 0 )
			playerIndex = players.Count - 1;

		var player = players[playerIndex];
		Target = player;

		// Force freecam off
		IsFree = false;

		return player;
	}

	public GunfightPlayer SpectateNextPlayer( bool asc = true )
	{
		return SelectPlayerIndex( asc ? playerIndex + 1 : playerIndex - 1 );
	}

	protected void ToggleFree()
	{
		IsFree ^= true;

		if ( IsFree )
		{
			if ( Target.IsValid() )
				Camera.Position = Target.AimRay.Position;

			vm?.Delete();
			cachedWeapon = null;
			Camera.FirstPersonViewer = null;
		}
		else
		{
			Camera.FirstPersonViewer = Target;
		}
	}

	float GetSpeedMultiplier()
	{
		if ( Input.Down( InputButton.Run ) )
			return 2f;
		if ( Input.Down( InputButton.Duck ) )
			return 0.3f;

		return 1f;
	}

	public override void BuildInput()
	{
		if ( Input.Pressed( InputButton.Jump ) )
			ToggleFree();

		if ( Input.Pressed( InputButton.Menu ) )
			SpectateNextPlayer( false );

		if ( Input.Pressed( InputButton.Use ) )
			SpectateNextPlayer();

		MoveMultiplier = GetSpeedMultiplier();

		if ( IsFree )
		{
			MoveInput = Input.AnalogMove;
			LookAngles += Input.AnalogLook;
			LookAngles.roll = 0;
		}
		else
		{
			base.BuildInput();
		}
	}

	Angles LookAngles;
	Vector3 MoveInput;

	protected BaseViewModel vm;
	protected GunfightWeapon cachedWeapon;

	protected void UpdateViewModel( GunfightWeapon weapon )
	{
		if ( IsSpectator )
		{
			vm?.Delete();
			vm = null;

			if ( weapon.IsValid() )
			{
				weapon?.CreateViewModel();
				vm = weapon.ViewModelEntity;
			}
		}
		else
		{
			vm?.Delete();
		}
	}

	[Event( "boomer.spectator.changedtarget" )]
	protected void OnTargetChanged( GunfightPlayer oldTarget, GunfightPlayer newTarget )
	{
		var curWeapon = newTarget?.ActiveChild as GunfightWeapon;
		cachedWeapon = curWeapon;

		UpdateViewModel( curWeapon );
	}

	public override void Update()
	{
		if ( !Target.IsValid() )
		{
			IsFree = true;
		}

		if ( IsFree )
		{
			var mv = MoveInput.Normal * BaseMoveSpeed * RealTime.Delta * Camera.Rotation * MoveMultiplier;
			Camera.Position += mv;
			Camera.Rotation = Rotation.From( LookAngles );
		}
		else
		{
			var curWeapon = Target?.ActiveChild as GunfightWeapon;
			if ( curWeapon.IsValid() && curWeapon != cachedWeapon )
			{
				cachedWeapon = curWeapon;
				UpdateViewModel( curWeapon );
			}

			base.Update();
		}
	}
}

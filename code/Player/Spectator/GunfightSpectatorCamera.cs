namespace Facepunch.Gunfight;

internal class GunfightSpectatorCamera : GunfightCamera
{
	[ConCmd.Admin( "gunfight_debug_togglespectator", Help = "Toggles spectator mode" )]
	public static void ToggleSpectator()
	{
		var cl = ConsoleSystem.Caller;
		cl.Pawn.Components.Add( new GunfightSpectatorCamera() );
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

	public void ResetInterpolation()
	{
		// Force eye rotation to avoid lerping when switching targets
		if ( Target.IsValid() )
			Rotation = Target.EyeRotation;
	}

	protected void ToggleFree()
	{
		IsFree ^= true;

		if ( IsFree )
		{
			if ( Target.IsValid() )
				Position = Target.EyePosition;

			vm?.Delete();
			cachedWeapon = null;
			Viewer = null;
		}
		else
		{
			ResetInterpolation();
			Viewer = Target;
		}
	}

	float GetSpeedMultiplier( InputBuilder input )
	{
		if ( input.Down( InputButton.Run ) )
			return 2f;
		if ( input.Down( InputButton.Duck ) )
			return 0.3f;

		return 1f;
	}

	public override void BuildInput( InputBuilder input )
	{
		if ( input.Pressed( InputButton.Jump ) )
			ToggleFree();

		if ( input.Pressed( InputButton.Menu ) )
			SpectateNextPlayer( false );

		if ( input.Pressed( InputButton.Use ) )
			SpectateNextPlayer();

		MoveMultiplier = GetSpeedMultiplier( input );

		if ( IsFree )
		{
			MoveInput = input.AnalogMove;
			LookAngles += input.AnalogLook;
			LookAngles.roll = 0;
		}
		else
		{
			base.BuildInput( input );
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

		ResetInterpolation();
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
			var mv = MoveInput.Normal * BaseMoveSpeed * RealTime.Delta * Rotation * MoveMultiplier;
			Position += mv;
			Rotation = Rotation.From( LookAngles );
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

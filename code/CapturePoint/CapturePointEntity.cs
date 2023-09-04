using System.ComponentModel.DataAnnotations;

namespace Facepunch.Gunfight;

[Library( "gunfight_capture_point" )]
[HammerEntity]
[Display( Name = "Capture Point"), Icon( "flag_circle" )]
[Sphere( "TriggerRadius", 255, 255, 255, true )]
[Solid]
public partial class CapturePointEntity : BaseTrigger, IHudMarker, ISpawnPoint
{
	protected static int ArraySize => Enum.GetNames( typeof( Team ) ).Length - 1;
	public Team Team { get => TeamComponent.Team; set => TeamComponent.Team = value; }

	[BindComponent] protected TeamComponent TeamComponent { get; }

	[Category( "Capture Point" ), Property] public float TriggerRadius { get; set; } = 386f;
	[Category( "Capture Point" ), Property] public float CaptureTime { get; set; } = 10f;

	[Net, Category( "Capture Point" ), Property] public string Identity { get; set; }
	[Net, Category( "Capture Point" ), Property] public string NiceName { get; set; } = "ZONE";
	[Net, Category( "Capture Point" )] public Team HighestTeam { get; set; } = Team.Unassigned;
	[Net, Category( "Capture Point" )] public IList<int> OccupantCounts { get; set; }
	[Net, Category( "Capture Point" )] public float Captured { get; set; } = 0;
	[Net, Category( "Capture Point" ), Change( "OnStateChanged" )] public CaptureState CurrentState { get; set; }

	public TimeSince TimeSinceStateChanged { get; protected set; } = 0;

	protected void OnStateChanged( CaptureState then, CaptureState now )
	{
		TimeSinceStateChanged = 0;
	}

	// @Server
	public Dictionary<Team, HashSet<GunfightPlayer>> Occupants { get; protected set; } = new();

	public void Initialize()
	{
		if ( Game.IsServer )
		{
			// Create a TeamComponent
			Components.GetOrCreate<TeamComponent>();

			Team = Team.Unassigned;
			HighestTeam = Team;
			Captured = 0;
			CurrentState = CaptureState.None;
			OccupantCounts.Clear();
			Occupants.Clear();
			TimeSinceStateChanged = 0;

			for ( int i = 0; i < ArraySize; i++ )
				OccupantCounts.Add( 0 );

			// Initialize the dictionary's list values.
			foreach ( Team team in Enum.GetValues( typeof( Team ) ) )
			{
				if ( team == Team.Unassigned )
					continue;

				Occupants[team] = new();
			}

			SetModel( "models/gameplay/flag_pole_base/flag_pole_base.vmdl" );
		}
	}
	public override void Spawn()
	{
		base.Spawn();

		// Set the default size
		SetTriggerSize( TriggerRadius );
		Transmit = TransmitType.Always;

		Initialize();
	}

	/// <summary>
	/// Set the trigger radius. Default is 16.
	/// </summary>
	public void SetTriggerSize( float radius )
	{
		SetupPhysicsFromSphere( PhysicsMotionType.Keyframed, Vector3.Zero, radius );
	}

	internal void AddPlayer( GunfightPlayer player )
	{
		// Already in the list!
		if ( Occupants[player.Team].Contains( player ) )
			return;

		// Already in another cap point.
		if ( player.CapturePoint is not null && player.CapturePoint != this )
			return;

		player.CapturePoint = this;

		Occupants[player.Team].Add( player );
		OccupantCounts[(int)player.Team]++;
	}

	internal void RemovePlayer( GunfightPlayer player )
	{
		if ( !Occupants.ContainsKey( player.Team ) )
			return;

		if ( !Occupants[player.Team].Contains( player ) )
			return;

		if ( player.CapturePoint == this )
			player.CapturePoint = null;

		Occupants[player.Team].Remove( player );
		OccupantCounts[(int)player.Team]--;
	}

	public override void StartTouch( Entity other )
	{
		base.StartTouch( other );

		if ( Game.IsServer && other is GunfightPlayer player )
		{
			AddPlayer( player );
			player.PlayerLocation = NiceName;
		}
	}

	public override void EndTouch( Entity other )
	{
		base.EndTouch( other );

		if ( Game.IsServer && other is GunfightPlayer player )
		{
			RemovePlayer( player );

			if ( player.PlayerLocation == NiceName )
				player.PlayerLocation = "";
		}
	}

	public int GetCount( Team team )
	{
		return OccupantCounts[(int)team];
	}

	[GameEvent.Tick.Server]
	public void Tick()
	{
		if ( Occupants is null || OccupantCounts is null )
			return;

		if ( Occupants.Count == 0 || OccupantCounts.Count == 0 )
			return;

		var lastCount = 0;
		var highest = Team.Unassigned;
		var contested = false;
		for ( int i = 0; i < OccupantCounts.Count; i++ )
		{
			var team = (Team)i;
			var count = OccupantCounts[i];

			if ( lastCount > 0 && count > 0 )
			{
				contested = true;
				break;
			}

			if ( count > 0 )
			{
				lastCount = count;
				highest = team;
			}
		}

		HighestTeam = highest;

		// nobody is fighting for this point (which shouldn't really happen)
		if ( highest == Team.Unassigned )
		{
			CurrentState = CaptureState.None;
			return;
		}

		// Don't do anythig while we're contested
		if ( contested )
		{
			CurrentState = CaptureState.Contested;
			return;
		}
		else
		{
			CurrentState = CaptureState.None;
		}

		// A team is trying to cap. Let's reverse this shit.
		if ( Team != Team.Unassigned && highest != Team )
		{
			float attackMultiplier = MathF.Sqrt( lastCount ); // Somewhat random sub-linear scale
			Captured = MathX.Clamp( Captured - Time.Delta * attackMultiplier / CaptureTime, 0, 1 );

			if ( Captured == 0f )
			{
				Team = Team.Unassigned;
			}
			else
			{
				CurrentState = CaptureState.Capturing;
			}
		}
		else
		{
			float attackMultiplier = MathF.Sqrt( lastCount ); // Somewhat random sub-linear scale


			var last = Captured;

			Captured = MathX.Clamp( Captured + Time.Delta * attackMultiplier / CaptureTime, 0, 1 );

			if ( Captured == 1f )
			{
				if ( last != Captured )
				{
					GamemodeSystem.Current?.OnFlagCaptured( this, highest );
				}
				Team = highest;
			}
			else
			{
				CurrentState = CaptureState.Capturing;
				Team = Team.Unassigned;
			}
		}
	}

	public Dictionary<string, bool> GetUIClasses()
	{
		var classes = new Dictionary<string, bool>();
		var friendState = TeamSystem.GetFriendState( Team, TeamSystem.MyTeam );

		// This isn't great. But it'll do.
		classes["friendly"] = friendState == TeamSystem.FriendlyStatus.Friendly;
		classes["enemy"] = friendState == TeamSystem.FriendlyStatus.Hostile;
		classes["contested"] = CurrentState == CaptureState.Contested;
		classes["capturing"] = CurrentState == CaptureState.Capturing;

		return classes;
	}

	public string UIClasses => string.Join( " ", GetUIClasses().Where( x => x.Value ).Select( x => x.Key ) );

	string IHudMarker.GetClass() => "capturepoint";
	bool IHudMarker.UpdateMarker( ref HudMarkerBuilder info )
	{
		if ( !this.IsValid() )
			return false;

		if ( GunfightCamera.Target is GunfightPlayer player && player.CapturePoint == this )
			return false;

		info.Text = Identity;
		info.Position = Position + Rotation.Up * 150f;
		info.StayOnScreen = true;
		info.Classes = GetUIClasses();

		return true;
	}

	string ISpawnPoint.GetIdentity() => NiceName;
	int ISpawnPoint.GetSpawnPriority() => 10;
	Transform? ISpawnPoint.GetSpawnTransform() => SpawnPointSystem.GetSuitableSpawn( Transform );

	bool ISpawnPoint.IsValidSpawn( GunfightPlayer player )
	{
		return CurrentState == CaptureState.None && TeamSystem.IsFriendly( player.Team, Team ) && ( GamemodeSystem.Current?.CapturePointsAreSpawnPoints ?? false );
	}

	public enum CaptureState
	{
		None,
		Contested,
		Capturing
	}


	[ConCmd.Admin( "gunfight_capturepoint_test" )]
	public static void Test()
	{
		var capturePoints = Entity.All.OfType<CapturePointEntity>().ToList();
		foreach ( var team in Enum.GetValues<Team>() )
		{
			capturePoints[(int)team].Captured = 1f;
			capturePoints[(int)team].Team = team;
		}
	}
}

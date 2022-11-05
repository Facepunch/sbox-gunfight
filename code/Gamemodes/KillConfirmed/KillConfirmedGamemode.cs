using Sandbox.UI;
using System.ComponentModel.DataAnnotations;

namespace Facepunch.Gunfight;

[Display( Name = "Kill Confirmed Gamemode" )]
public partial class KillConfirmedGamemode : Gamemode
{
	[Net] public GameState State { get; protected set; }
	[Net] public TimeSince TimeSinceStateChanged { get; protected set; }
	[Net] public TimeUntil TimeUntilNextState { get; protected set; }
	[Net] public Team WinningTeam { get; protected set; }
	[Net] public Loadout CurrentLoadout { get; set; }

	public TimeSpan TimeRemaining => TimeSpan.FromSeconds( TimeUntilNextState );
	public string FormattedTimeRemaining => TimeRemaining.ToString( @"mm\:ss" );

	protected string CachedTimeRemaining { get; set; }

	public override List<Team> TeamSetup => new() { Team.BLUFOR, Team.OPFOR };

	// Stats
	protected int MinimumPlayers => 2;
	protected float RoundCountdownLength => 10f;
	protected float RoundLength => 600f;
	protected float GameWonLength => 30f;

	public override Panel GetHudPanel() => new UI.KillConfirmedHud();

	public override void Spawn()
	{
		base.Spawn();
		RandomizeLoadout();
		Scores.MaximumScore = 75;
	}

	public override void OnClientJoined( Client cl )
	{
		base.OnClientJoined( cl );

		var teamComponent = cl.Components.GetOrCreate<TeamComponent>();
		teamComponent.Team = TeamSystem.GetLowestCount();

		UI.Chatbox.AddChatEntry( To.Everyone, cl.Name, $"joined {teamComponent.Team.GetName()}", cl.PlayerId, null, false );
		
		VerifyEnoughPlayers();
	}

	protected void VerifyEnoughPlayers()
	{
		if ( State == GameState.WaitingForPlayers )
		{
			if ( PlayerCount >= MinimumPlayers )
			{
				SetGameState( GameState.RoundCountdown );
			}
		}
	}

	protected static Loadout GetRandomLoadout()
	{
		var loadouts = Loadout.WithTag( "gunfight" ).ToList();
		var index = Rand.Int( 1, loadouts.Count() ) - 1;
		var loadout = loadouts[index];

		return loadout;
	}

	protected void RandomizeLoadout()
	{
		var oldLoadout = CurrentLoadout;
		
		Loadout newLoadout = null;
		while ( newLoadout == null )
		{
			var randomLoadout = GetRandomLoadout();
			if ( randomLoadout == oldLoadout ) continue;

			newLoadout = randomLoadout;
		}

		CurrentLoadout = newLoadout;
	}

	public override bool PlayerLoadout( GunfightPlayer player )
	{
		CurrentLoadout?.Give( player );
		GunfightStatusPanel.RpcUpdate( To.Everyone );

		return true;
	}

	public override bool AllowMovement()
	{
		return State != GameState.RoundCountdown;
	}

	public override bool AllowDamage()
	{
		return State != GameState.RoundCountdown;
	}

	public override bool AllowSpectating()
	{
		return true;
	}

	public override bool CanPlayerRegenerate( GunfightPlayer player )
	{
		return true;
	}

	public override void PreSpawn( GunfightPlayer player )
	{
		player.SpawnPointTag = player.Team == Team.BLUFOR ? "one" : "two";
	}

	public void SetGameState( GameState newState )
	{
		var old = State;
		State = newState;

		Log.Info( $"Game State Changed to {newState}" );

		OnGameStateChanged( old, newState );
	}

	public string GetGameStateLabel()
	{
		return State switch
		{
			GameState.WaitingForPlayers => $"NEED {MinimumPlayers - PlayerCount} PLAYER(S)",
			GameState.GameWon => $"{WinningTeam.GetName()} won",
			_ => $"{GetTimeLeftLabel()}"
		};
	}

	public override string GetTimeLeftLabel()
	{
		return State switch
		{
			_ => FormattedTimeRemaining
		};
	}

	protected void OnGameStateChanged( GameState before, GameState after )
	{
		TimeSinceStateChanged = 0;

		GunfightStatusPanel.RpcUpdate( To.Everyone );

		if ( after == GameState.WaitingForPlayers )
		{
			WinningTeam = Team.Unassigned;
			var scores = GunfightGame.Current.Scores;
			scores.Reset();

			ResetStats();
			VerifyEnoughPlayers();
		}
		if ( after == GameState.RoundCountdown )
		{
			CleanupMap();
			TimeUntilNextState = RoundCountdownLength;
			RandomizeLoadout();
			RespawnAllPlayers();

			UI.LoadoutPanel.RpcShow( To.Everyone );
		}
		else if ( after == GameState.RoundActive )
			TimeUntilNextState = RoundLength;
		else if ( after == GameState.GameWon )
		{
			TimeUntilNextState = GameWonLength;
			ChatBox.AddInformation( To.Everyone, $"{WinningTeam.GetName()} won the match!" );
		}

		Event.Run( "gunfight.gamestate.changed", before, after );
	}
	
	protected void OnSecond()
	{
		CachedTimeRemaining = FormattedTimeRemaining;
		OnSecondElapsed();
	}

	protected virtual void OnSecondElapsed()
	{
		if ( State != GameState.RoundCountdown ) return;

		if ( TimeUntilNextState <= 6f && TimeUntilNextState >= 0f )
		{
			if ( TimeUntilNextState <= 1f )
			{
				Sound.FromScreen( "countdown.start" );
			}
			else
			{
				Sound.FromScreen( "countdown.beep" );
			}
		}
	}

	[Event.Frame]
	protected void Frame()
	{
		if ( CachedTimeRemaining != FormattedTimeRemaining )
		{
			OnSecond();
		}
	}

	public override void Simulate( Client cl )
	{
		if ( !IsServer ) return;

		if ( TimeUntilNextState )
		{
			// If the round counts down to 0, start the round
			if ( State == GameState.RoundCountdown )
				SetGameState( GameState.RoundActive );
			else if ( State == GameState.RoundActive )
				SetGameState( GameState.GameWon );
			else if ( State == GameState.GameWon )
				SetGameState( GameState.WaitingForPlayers );
		}
	}

	public override void PostProcessTick()
	{
		//var postProcess = Map.Camera.FindOrCreateHook<Sandbox.Effects.ScreenEffects>();
		//postProcess.colo.Enabled = State == GameState.RoundCountdown;
		//postProcess.ChromaticAberration.Scale = State == GameState.RoundCountdown ? 1 : 0;
		//if ( State == GameState.RoundCountdown )
		//{
		//	postProcess.ColorOverlay.Color = new Color( 0, 0, 0.3f );
		//	postProcess.ColorOverlay.Mode = StandardPostProcess.ColorOverlaySettings.OverlayMode.Mix;
		//	postProcess.ColorOverlay.Amount = 0.1f;

		//	postProcess.ChromaticAberration.Offset = new Vector3( -0.0007f, -0.0007f, 0f );
		//}
	}

	public override void OnScoreChanged( Team team, int score, bool maxReached = false )
	{
		if ( maxReached )
		{
			WinningTeam = team;
			SetGameState( GameState.GameWon );
		}
	}

	public override void OnPlayerKilled( GunfightPlayer player, DamageInfo damageInfo, out LifeState lifeState )
	{
		lifeState = LifeState.Respawning;
	}

	public override void PostPlayerKilled( GunfightPlayer player, DamageInfo lastDamage )
	{
		var scores = GunfightGame.Current.Scores;
		var attacker = lastDamage.Attacker as GunfightPlayer;
		if ( attacker.IsValid() )
		{
			var newScore = scores.AddScore( attacker.Team, 1 );
		}
    }

	public override void CleanupMap()
	{
		foreach( var weapon in Entity.All.OfType<GunfightWeapon>() )
		{
			if ( !weapon.Parent.IsValid() )
			{
				weapon.Delete();
			}
		}
	}


	public enum GameState
	{
		WaitingForPlayers, // to round countdown
		RoundCountdown, // to round active
		RoundActive, // to game won
		GameWon
	}
}

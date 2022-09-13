using Sandbox.UI;

namespace Facepunch.Gunfight;

public abstract partial class GamemodeEntity : Entity
{
	/// <summary>
	/// A quick accessor to get how many people are in the game
	/// </summary>
	public int PlayerCount { get; private set; }

	/// <summary>
	/// Can specify a panel to be created when the gamemode is made.
	/// </summary>
	/// <returns></returns>
	public virtual Panel GetHudPanel() => null;

	/// <summary>
	/// Gamemodes can define what pawn to create
	/// </summary>
	/// <param name="cl"></param>
	/// <returns></returns>
	public virtual GunfightPlayer GetPawn( Client cl ) => new GunfightPlayer();

	public virtual void CreatePawn( Client cl )
	{
		cl.Pawn?.Delete();

		var pawn = GetPawn( cl );
		cl.Pawn = pawn;
		pawn.Respawn();
	}

	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;
	}

	/// <summary>
	/// Called when a client joins the game
	/// </summary>
	/// <param name="cl"></param>
	public virtual void OnClientJoined( Client cl )
	{
		PlayerCount++;
	}

	public virtual void Initialize()
	{
	}

	/// <summary>
	/// Called when a client leaves the game
	/// </summary>
	/// <param name="cl"></param>
	/// <param name="reason"></param>
	public virtual void OnClientLeft( Client cl, NetworkDisconnectionReason reason )
	{
		PlayerCount--;
	}

	/// <summary>
	/// Called when the score changes for any team
	/// </summary>
	/// <param name="team"></param>
	/// <param name="score"></param>
	public virtual void OnScoreChanged( Team team, int score )
	{
	}

	/// <summary>
	/// Called when a player takes damage
	/// </summary>
	/// <param name="player"></param>
	/// <param name="damageInfo"></param>
	public virtual void OnPlayerKilled( GunfightPlayer player, DamageInfo damageInfo )
	{
	}

	/// <summary>
	/// Allows gamemodes to override player spawn locations
	/// </summary>
	/// <param name="player"></param>
	/// <returns></returns>
	public virtual Transform? GetSpawn( GunfightPlayer player )
	{
		return null;
	}

	/// <summary>
	/// Decides whether or not players can move
	/// </summary>
	/// <returns></returns>
	public virtual bool AllowMovement()
	{
		return true;
	}

	/// <summary>
	/// Decides whether or not players can respawn
	/// </summary>
	/// <returns></returns>
	public virtual bool AllowRespawning()
	{
		return true;
	}

	/// <summary>
	/// Decides whether or not players can take damage
	/// </summary>
	/// <returns></returns>
	public virtual bool AllowDamage()
	{
		return true;
	}

	/// <summary>
	/// Called on Client Tick, allows gamemodes to define custom post processing
	/// </summary>
	/// <param name="postProcess"></param>
	public virtual void PostProcessTick( StandardPostProcess postProcess )
	{
	}

	public virtual void CleanupMap()
	{
		//
	}
}

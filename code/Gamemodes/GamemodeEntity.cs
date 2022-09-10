using Sandbox.UI;

namespace Facepunch.Gunfight;

public abstract partial class GamemodeEntity : Entity
{
	public static GamemodeEntity Current { get; set; }

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

		var pawn = Current?.GetPawn( cl );
		cl.Pawn = pawn;
		pawn.Respawn();
	}

	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;

		// There can be only one gamemode running at a time.
		if ( Current.IsValid() && Current != this )
		{
			Delete();
			Log.Warning( "There can be only one gamemode running at one time. Please make sure there's only 1 gamemode entity on a level." );

			return;
		}

		Current = this;
	}

	public override void ClientSpawn()
	{
		base.ClientSpawn();
		Current = this;
	}

	/// <summary>
	/// Called when a client joins the game
	/// </summary>
	/// <param name="cl"></param>
	public virtual void OnClientJoined( Client cl )
	{
		PlayerCount++;
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

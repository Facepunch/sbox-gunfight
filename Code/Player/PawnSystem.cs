namespace Gunfight;

public interface IPawn
{
	public void OnPossess();
	public void OnDePossess();

	/// <summary>
	/// Are we possessing this pawn right now? (Clientside)
	/// </summary>
	public bool IsPossessed
	{
		get
		{
			// Is the viewer the same as this Pawn?
			return Game.ActiveScene.GetSystem<PawnSystem>().Viewer == this;
		}
	}

	/// <summary>
	/// Possess the pawn.
	/// </summary>
	public void Possess()
	{
		Game.ActiveScene.GetSystem<PawnSystem>()
			.Possess( this );
	}

	/// <summary>
	/// De possesses the pawn.
	/// </summary>
	public void DePossess()
	{
		Game.ActiveScene.GetSystem<PawnSystem>()
			.DePossess( this );
	}
}

/// <summary>
/// The system that holds data about what pawn we're looking through the eyes of.
/// If we don't have network authority, we'll try to spectate that pawn.
/// </summary>
public partial class PawnSystem : GameObjectSystem
{
	public IPawn Viewer { get; private set; }

	/// <summary>
	/// Tries to possess a pawn. If you don't own it, spectate!
	/// </summary>
	public void Possess( IPawn pawn )
	{
		DePossess( Viewer );
		Viewer = pawn;

		pawn?.OnPossess();
	}

	public void DePossess( IPawn pawn )
	{
		pawn?.OnDePossess();
		Viewer = null;
	}

	public PawnSystem( Scene scene ) : base( scene )
	{
	}
}

namespace Gunfight;

public interface IPawn
{
	public void OnPossess();
	public void OnUnPossess();

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
}

public partial class PawnSystem : GameObjectSystem
{
	public IPawn Viewer { get; private set; }

	/// <summary>
	/// Tries to possess a pawn. If you don't own it, spectate!
	/// </summary>
	public void Possess( IPawn pawn )
	{
		Viewer?.OnUnPossess();
		Viewer = pawn;

		pawn?.OnPossess();
	}

	public PawnSystem( Scene scene ) : base( scene )
	{
	}
}

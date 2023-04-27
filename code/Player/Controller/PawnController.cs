
namespace Facepunch.Gunfight;

public partial class PawnController : BaseNetworkable
{
	internal HashSet<string> Events;
	internal HashSet<string> Tags;

	public Entity Pawn { get; protected set; }
	public IClient Client { get; protected set; }
	public Vector3 Position { get; set; }
	public Rotation Rotation { get; set; }
	public Vector3 Velocity { get; set; }
	public Rotation EyeRotation { get; set; }
	[Net, Predicted] public Vector3 EyeLocalPosition { get; set; }
	public Vector3 BaseVelocity { get; set; }
	public Entity GroundEntity { get; set; }
	public Vector3 GroundNormal { get; set; }

	public Vector3 WishVelocity { get; set; }

	public void UpdateFromEntity( Entity entity )
	{
		Position = entity.Position;
		Rotation = entity.Rotation;
		Velocity = entity.Velocity;

		if ( entity is GunfightPlayer player )
		{
			EyeRotation = player.Controller.EyeRotation;
			EyeLocalPosition = player.Controller.EyeLocalPosition;
		}

		BaseVelocity = entity.BaseVelocity;
		GroundEntity = entity.GroundEntity;
		WishVelocity = entity.Velocity;
	}

	public void UpdateFromController( PawnController controller )
	{
		Pawn = controller.Pawn;
		Client = controller.Client;

		Position = controller.Position;
		Rotation = controller.Rotation;
		Velocity = controller.Velocity;
		EyeRotation = controller.EyeRotation;
		GroundEntity = controller.GroundEntity;
		BaseVelocity = controller.BaseVelocity;
		EyeLocalPosition = controller.EyeLocalPosition;
		WishVelocity = controller.WishVelocity;
		GroundNormal = controller.GroundNormal;

		Events = controller.Events;
		Tags = controller.Tags;
	}

	public void Finalize( Entity target )
	{
		target.Position = Position;
		target.Velocity = Velocity;
		target.Rotation = Rotation;
		target.GroundEntity = GroundEntity;
		target.BaseVelocity = BaseVelocity;

		if ( target is GunfightPlayer player )
		{
			player.Controller.EyeLocalPosition = EyeLocalPosition;
			player.Controller.EyeRotation = EyeRotation;
		}
	}

	/// <summary>
	/// This is what your logic should be going in
	/// </summary>
	public virtual void Simulate()
	{
		// Nothing
	}

	/// <summary>
	/// This is called every frame on the client only
	/// </summary>
	public virtual void FrameSimulate()
	{
		Game.AssertClient();
	}

	/// <summary>
	/// Call OnEvent for each event
	/// </summary>
	public virtual void RunEvents( PawnController additionalController )
	{
		if ( Events == null ) return;

		foreach ( var e in Events )
		{
			OnEvent( e );
			additionalController?.OnEvent( e );
		}
	}

	/// <summary>
	/// An event has been triggered - maybe handle it
	/// </summary>
	public virtual void OnEvent( string name )
	{

	}

	/// <summary>
	/// Returns true if we have this event
	/// </summary>
	public bool HasEvent( string eventName )
	{
		if ( Events == null ) return false;
		return Events.Contains( eventName );
	}

	/// <summary>
	/// </summary>
	public bool HasTag( string tagName )
	{
		if ( Tags == null ) return false;
		return Tags.Contains( tagName );
	}


	/// <summary>
	/// Allows the controller to pass events to other systems
	/// while staying abstracted.
	/// For example, it could pass a "jump" event, which could then
	/// be picked up by the playeranimator to trigger a jump animation,
	/// and picked up by the player to play a jump sound.
	/// </summary>
	public void AddEvent( string eventName )
	{
		// TODO - shall we allow passing data with the event?

		if ( Events == null ) Events = new HashSet<string>();

		if ( Events.Contains( eventName ) )
			return;

		Events.Add( eventName );
	}


	/// <summary>
	/// </summary>
	public void SetTag( string tagName )
	{
		// TODO - shall we allow passing data with the event?

		Tags ??= new HashSet<string>();

		if ( Tags.Contains( tagName ) )
			return;

		Tags.Add( tagName );
	}

	/// <summary>
	/// Allow the controller to tweak input. Empty by default.
	/// </summary>
	public virtual void BuildInput()
	{

	}

	public void Simulate( IClient client, Entity pawn )
	{
		Events?.Clear();
		Tags?.Clear();

		Pawn = pawn;
		Client = client;

		UpdateFromEntity( pawn );

		Simulate();

		Finalize( pawn );
	}
	
	public void FrameSimulate( IClient client, Entity pawn )
	{
		Pawn = pawn;
		Client = client;

		UpdateFromEntity( pawn );

		FrameSimulate();

		Finalize( pawn );
	}

	public static bool Debug { get; set; } = false;

	/// <summary>
	/// Any bbox traces we do will be offset by this amount.
	/// todo: this needs to be predicted
	/// </summary>
	public Vector3 TraceOffset;

	/// <summary>
	/// Traces the bbox and returns the trace result.
	/// LiftFeet will move the start position up by this amount, while keeping the top of the bbox at the same 
	/// position. This is good when tracing down because you won't be tracing through the ceiling above.
	/// </summary>
	public virtual TraceResult TraceBBox( Vector3 start, Vector3 end, Vector3 mins, Vector3 maxs, float liftFeet = 0.0f )
	{
		if ( liftFeet > 0 )
		{
			start += Vector3.Up * liftFeet;
			maxs = maxs.WithZ( maxs.z - liftFeet );
		}

		var tr = Trace.Ray( start + TraceOffset, end + TraceOffset )
					.Size( mins, maxs )
					.WithAnyTags( "solid", "playerclip", "passbullets", "player" )
					.Ignore( Pawn )
					.Run();

		tr.EndPosition -= TraceOffset;
		return tr;
	}

	/// <summary>
	/// This calls TraceBBox with the right sized bbox. You should derive this in your controller if you 
	/// want to use the built in functions
	/// </summary>
	public virtual TraceResult TraceBBox( Vector3 start, Vector3 end, float liftFeet = 0.0f )
	{
		return TraceBBox( start, end, Vector3.One * -1, Vector3.One, liftFeet );
	}

	/// <summary>
	/// This is temporary, get the hull size for the player's collision
	/// </summary>
	public virtual BBox GetHull()
	{
		return new BBox( -10, 10 );
	}
}

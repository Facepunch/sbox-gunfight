using Sandbox.Html;
using Sandbox.UI;

namespace Facepunch.Gunfight.MainMenu;

public class WeaponViewer : ScenePanel
{
	public SceneModel Model { get; set; }
	public List<SceneModel> Models = new List<SceneModel>();

	/// <summary>
	/// Scale the model's animation time by this amount
	/// </summary>
	public float TimeScale { get; set; } = 1.0f;

	public WeaponViewer()
	{
		World = new SceneWorld();

	//	new SceneSunLight( World, Rotation.From( 70, -180, 0 ), Color.Green * 0.2f );

		new SceneLight( World, Vector3.Left * 50f + Vector3.Up * 150.0f, 512, new Color( 1f, 0.4f, 0.4f ) * 4.0f );
		new SceneLight( World, Vector3.Right * 50f + Vector3.Up * 150.0f, 512, new Color( 0.4f, 0.4f, 1f ) * 4.0f );


		//new SceneLight( World, Vector3.Up * 150.0f, 200.0f, Color.White * 2.0f );
		new SceneLight( World, Vector3.Up * 150.0f + Vector3.Backward * 100.0f, 512, new Color( 0.7f, 0.8f, 1 ) * 3.0f );
		//new SceneLight( World, Vector3.Up * 10.0f + Vector3.Right * 100.0f, 200, Color.White * 4.0f );
		// new SceneLight( World, Vector3.Up * 10.0f + Vector3.Left * 100.0f, 200, Color.White * 4.0f );
	}

	public override void OnDeleted()
	{
		base.OnDeleted();

		Model?.Delete();
		Model = null;

		World?.Delete();
		World = null;
	}


	protected float MouseWidthNormal;
	protected float MouseHeightNormal;

	public override void Tick()
	{
		base.Tick();

		if ( !IsVisible ) return;

		foreach ( var model in Models )
		{
			model.Update( Time.Delta * TimeScale );
		}


		var mdW = MousePosition.x / Screen.Width;
		var mdH = MousePosition.y / Screen.Height;

		MouseWidthNormal = MouseWidthNormal.LerpTo( mdW, Time.Delta * 2f );
		MouseHeightNormal = MouseHeightNormal.LerpTo( mdH, Time.Delta * 2f );

		Model.Rotation = Model.Rotation.Angles()
			.WithYaw( 90 + (( MouseWidthNormal - 0.5f ) * 50f) )
			.WithRoll( 0 + (( MouseHeightNormal - 0.5f) * -10f))
			.ToRotation();
	}

	public SceneModel AddModel( Model model, Transform transform = default )
	{
		transform = transform.WithScale( 1f );
		var o = new SceneModel( World, model, transform );
		Models.Add( o );
		o.Update( 0.1f );

		Log.Info( o );

		return o;
	}

	public void RemoveModel( SceneModel model )
	{
		Models.Remove( model );
		model?.Delete();
	}

	public void AddModels( IEnumerable<SceneModel> models )
	{
		Models.AddRange( models );
	}

	internal void SetModel( Model model )
	{
		if ( Model != null )
		{
			Models.Remove( Model );
			Model.Delete();
			Model = null;
		}

		Model = new SceneModel( World, model, Transform.Zero.WithRotation( Rotation.From( 0, 90, 0 ) ) );
		Model.SetBodyGroup( "barrel", 2 );
		Model.SetBodyGroup( "sights", 1 );

		Models.Add( Model );
		Model.Update( 0.1f );
	}

	internal void SetModel( string modelName )
	{
		if ( Model != null )
		{
			Models.Remove( Model );
			Model.Delete();
			Model = null;
		}

		var model = Sandbox.Model.Load( modelName );
		Model = new SceneModel( World, model, Transform.Zero );

		Models.Add( Model );
		Model.Update( 0.1f );
	}
}


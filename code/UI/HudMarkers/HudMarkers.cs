using Sandbox.UI;

namespace Facepunch.Gunfight;

public partial class HudMarker : Panel
{
	public HudMarker( IHudMarker entity )
	{
		Label = AddChild<Label>( "label" );
		Icon = AddChild<Panel>( "icon" );

		Entity = entity;
	}

	public Label Label { get; set; }
	public Panel Icon { get; set; }
	public IHudMarker Entity { get; set; }

	public bool StayOnScreen { get; set; } = false;
	public Vector2 SafetyBounds { get; set; } = new Vector2( 0.02f, 0.02f );

	public bool IsFocused { get; set; } = true;

	public Vector3 Position { get; set; } = new();

	public float MaxDistance { get; set; } = 0f;
	public float DistanceScale { get; set; } = 1f;
	public float CurrentDistance { get; set; } = 0f;

	public void Apply( HudMarkerBuilder info )
	{
		Label.Text = info.Text;
		Position = info.Position;
		StayOnScreen = info.StayOnScreen;
		MaxDistance = info.MaxDistance;
		DistanceScale = info.DistanceScale;

		foreach ( var kv in info.Classes )
			SetClass( kv.Key, kv.Value );

		PositionAtWorld();
	}

	public bool PositionAtWorld()
	{
		var screenpos = GetScreenPoint();

		var cachedX = screenpos.x;
		var cachedY = screenpos.y;

		var isFocused = cachedX.AlmostEqual( 0.5f, 0.04f ) && cachedY.AlmostEqual( 0.5f, 0.1f );

		IsFocused = isFocused;
		SetClass( "nofocus", !isFocused );
		SetClass( "isfocused", isFocused );

		if ( StayOnScreen )
		{
			var safetyX = SafetyBounds.x;
			var safetyY = SafetyBounds.y;

			screenpos.x = screenpos.x.Clamp( safetyX, 1 - safetyX );
			screenpos.y = screenpos.y.Clamp( safetyY, 1 - safetyY );
		}

		Style.Left = Length.Fraction( screenpos.x );
		Style.Top = Length.Fraction( screenpos.y );

		if ( MaxDistance != 0 )
		{
			var tr = new PanelTransform();
			var scale = 1 - (CurrentDistance / MaxDistance) * DistanceScale;
			tr.AddScale( scale );
			tr.AddTranslateX( Length.Percent( -50f ) );
			Style.Transform = tr;
		}

		return cachedX < 0 || cachedX > 1 || cachedY < 0 || cachedY > 1;
	}

	public Vector3 GetScreenPoint()
	{
		var worldPoint = Position;
		var screenPoint = worldPoint.ToScreen();

		return screenPoint;
	}
}

public partial class HudMarkers : Panel
{
	public static HudMarkers Current;

	public HudMarkers()
	{
		Current = this;
		StyleSheet.Load( "/UI/HudMarkers/HudMarkers.scss" );
	}

	protected List<HudMarker> Markers { get; set; } = new();

	public void Clear( HudMarker marker )
	{
		if ( marker is null )
			return;
		if ( !Markers.Contains( marker ) )
			return;

		Markers.Remove( marker );
		marker.Delete();

		return;
	}

	protected void ValidateEntity( IHudMarker entity )
	{
		var info = new HudMarkerBuilder();
		var styleClass = entity.GetClass();
		var current = Markers.Where( x => x.Entity == entity ).FirstOrDefault();

		if ( !entity.UpdateMarker( ref info ) )
		{
			Clear( current );
			return;
		}

		var maxDistance = info.MaxDistance;
		if ( current is not null && maxDistance != 0 )
		{
			var dist = Camera.Position.DistanceSquared( info.Position );
			current.CurrentDistance = dist;

			if ( dist > maxDistance )
			{
				Clear( current );
				return;
			}
		}

		if ( current is null )
		{
			current = new HudMarker( entity )
			{
				Parent = this
			};

			// Add style class
			current.AddClass( styleClass );
			Markers.Add( current );
		}

		current.Apply( info );
	}

	protected void UpdateHudMarkers()
	{
		var existingMarkers = Markers.Select( x => x.Entity ).ToList();

		Entity.All.OfType<IHudMarker>()
						.Concat( existingMarkers )
						.ToList()
						.ForEach( x => ValidateEntity( x ) );
	}

	public override void Tick()
	{
		base.Tick();
		UpdateHudMarkers();
	}
}

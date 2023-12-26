using Editor;
using Sandbox;
using System.Collections.Generic;
using System.Linq;

namespace Gunfight.Editor;

public partial class RecoilPatternInstance : GraphicsItem
{
	public Vector2 RangeX { get; set; }
	public Vector2 RangeY { get; set; }

	public List<RecoilPatternKey> Keys { get; set; } = new();

	public RecoilPatternInstance( GraphicsItem parent ) : base( parent )
	{
		HoverEvents = true;
		Clip = true;
	}

	RecoilPattern _value;

	public RecoilPattern Value
	{
		get => _value;
		set
		{
			_value = value;
		}
	}

	internal void Refresh()
	{
		foreach ( var key in Keys )
		{
			key.Destroy();
		}

		Keys.Clear();

		foreach ( var point in Value.Points )
		{
			AddKey( DeserializePoint( point ) );
		}
	}

	protected override void OnPaint()
	{
		Paint.SetPen( Color.Gray.WithAlpha( 0.5f ), 2, PenStyle.Dot );
		Paint.DrawLine( Keys.Select( x => x.Position + new Vector2( 8.0f, 8.0f ) ) );
	}

	Vector2 DeserializePoint( Vector2 point )
	{
		var x = point.x;
		var y = point.y;

		var remappedX = x.Remap( RangeX.x, RangeX.y, 0, 1 );
		var remappedY = y.Remap( RangeY.x, RangeY.y, 0, 1 );

		return new Vector2( remappedX * Size.x, remappedY * Size.y ); ;
	}

	RecoilPattern Serialize()
	{
		var pattern = new RecoilPattern()
		{
			Points = Keys.Select( x => x.Evaluate( RangeX, RangeY ) ).ToList()
		};
		return pattern;
	}

	internal void AddKey( Vector2 pos )
	{
		Keys.Add( new RecoilPatternKey( this, pos ) );
		Update();

		Value = Serialize();
	}

	internal void RemoveKey( RecoilPatternKey key )
	{
		Keys.Remove( key );
		key.Destroy();
		Update();

		Value = Serialize();
	}

	protected override void OnMousePressed( GraphicsMouseEvent e )
	{
		if ( e.LeftMouseButton )
		{
			if ( Keys.FirstOrDefault( x => x.Hovered ) is RecoilPatternKey key )
			{
				return;
			}

			AddKey( e.LocalPosition );
		}
	}
}

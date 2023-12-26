using Editor;
using Sandbox;

namespace Gunfight.Editor;

public partial class RecoilPatternKey : GraphicsItem
{
	RecoilPatternInstance Instance => Parent as RecoilPatternInstance;

	/// <summary>
	/// Gets the index of this key from the recoil pattern instance
	/// </summary>
	/// <returns></returns>
	public int GetIndex()
	{
		if ( Instance is not null )
		{
			return Instance.Keys.FindIndex( x => x == this );
		}

		return 1;
	}

	public Vector2 Evaluate( Vector2 rangeX, Vector2 rangeY )
	{
		var x = Position.x / Instance.Size.x;
		var y = Position.y / Instance.Size.y;

		var remappedX = x.Remap( 0, 1, rangeX.x, rangeX.y );
		var remappedY = y.Remap( 0, 1, rangeY.x, rangeY.y );

		return new Vector2( remappedX, remappedY );
	}

	public RecoilPatternKey( GraphicsItem parent, Vector2 key ) : base( parent )
	{
		HoverEvents = true;
		Clip = true;
		Cursor = CursorShape.DragCopy;
		Movable = true;
		//Selectable = true;
		Size = new( 32.0f, 16.0f );
		Position = key;
	}

	protected override void OnPaint()
	{
		var rect = LocalRect;

		Paint.SetPen( Hovered ? Theme.Selection : Color.Gray.Darken( 0.2f ), 2 );
		Paint.SetBrush( Color.Gray.Darken( 0.5f ) );

		Paint.DrawRect( rect, 4 );

		Paint.SetPen( Color.Gray.Lighten( 0.2f ) );
		Paint.DrawIcon( rect, "close", 16, Sandbox.TextFlag.LeftCenter );
		Paint.DrawText( rect.Shrink( 4, 0 ), $"{GetIndex() + 1}", Sandbox.TextFlag.RightCenter );
	}

	protected override void OnMousePressed( GraphicsMouseEvent e )
	{
		base.OnMousePressed( e );

		if ( e.RightMouseButton )
		{
			Instance.RemoveKey( this );
		}
	}
}

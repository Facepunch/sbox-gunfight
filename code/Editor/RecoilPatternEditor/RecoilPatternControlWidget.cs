using Editor;
using Sandbox;

namespace Gunfight.Editor;

[CustomEditor( typeof( RecoilPattern ) )]
public class CurveControlWidget : ControlWidget
{
	public Color HighlightColor = Theme.Yellow;

	public CurveControlWidget( SerializedProperty property ) : base( property )
	{
		SetSizeMode( SizeMode.Default, SizeMode.Default );

		Layout = Layout.Column();
		Layout.Spacing = 2;
		Cursor = CursorShape.Finger;
	}

	protected override Vector2 SizeHint() => new Vector2( 10000, 22 );

	protected override void PaintOver()
	{


		var rect = LocalRect.Shrink( 8, 0 );
		var alpha = Paint.HasMouseOver ? 1f : 0.7f;

		Paint.SetBrush( Theme.ButtonDefault.Darken( 0.2f ) );
		Paint.DrawRect( LocalRect, 2 );

		// icon
		{
			Paint.SetPen( Theme.ControlText.WithAlphaMultiplied( alpha ) );
			var r = Paint.DrawIcon( rect, "track_changes", 17, TextFlag.LeftCenter );
			rect.Left += r.Width + 8;
		}

		Paint.SetPen( Theme.ControlText.WithAlphaMultiplied( alpha ) );
		Paint.DrawText( rect, "Open Recoil Editor", TextFlag.LeftCenter );
	}

	protected override void OnMousePress( MouseEvent e )
	{
		base.OnMousePress( e );

		if ( e.LeftMouseButton )
		{
			// open our editor
			var editor = new RecoilPatternPopup( this, SerializedProperty.GetValue<RecoilPattern>() );
			editor.Visible = true;
			editor.Position = e.ScreenPosition - editor.Size * new Vector2( 1, 0.5f );
			editor.ConstrainToScreen();
			editor.OnValueChanged += ( v ) =>
			{
				SerializedProperty.SetValue( v );
				Update();
			};
		}
	}
}

using System;
using Editor;
using Editor.GraphicsItems;
using static Editor.GraphicsItems.ChartBackground;

namespace Gunfight.Editor;

/// <summary>
/// A widget which contains an editable curve
/// </summary>
public class RecoilPatternEditor : GraphicsView
{
	ChartBackground Background;

	public RecoilPatternEditor( Widget parent ) : base( parent )
	{
		SceneRect = new( 0, Size );
		HorizontalScrollbar = ScrollbarMode.Off;
		VerticalScrollbar = ScrollbarMode.Off;
		Scale = 1;
		CenterOn( new Vector2( 100, 10 ) );

		Background = new ChartBackground
		{
			Size = SceneRect.Size,
			RangeX = new Vector2( -5, 5 ),
			RangeY = new Vector2( 0, 5 ),
			AxisX = new AxisConfig { LineColor = Theme.White.WithAlpha( 0.2f ), Ticks = 11, Width = 30.0f, LabelColor = Theme.White.WithAlpha( 0.5f ) },
			AxisY = new AxisConfig { LineColor = Theme.White.WithAlpha( 0.2f ), Ticks = 11, Width = 20.0f, LabelColor = Theme.White.WithAlpha( 0.5f ) }
		};

		Add( Background );
	}

	public void AddInstance( Func<RecoilPattern> get, Action<RecoilPattern> set )
	{
		var instance = new RecoilPatternInstance( null )
		{
			RangeX = Background.RangeX,
			RangeY = Background.RangeY,
		};

		instance.Bind( "Value" ).From( get, set );

		var c = get();
		instance.Value = c;

		Add( instance );
	}

	protected override void DoLayout()
	{
		base.DoLayout();

		SceneRect = new( 0, Size );
		Background.Size = SceneRect.Size;
		
		foreach ( var i in Items )
		{
			if ( i is RecoilPatternInstance instance )
			{
				if ( instance.SceneRect == Background.ChartRect ) continue;
				instance.SceneRect = Background.ChartRect;
				instance.Refresh();
			}
		}
	}
}

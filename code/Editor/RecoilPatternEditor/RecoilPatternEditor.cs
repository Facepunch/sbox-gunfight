using System;
using System.Linq;
using System.Security.Cryptography;
using Editor;
using Editor.GraphicsItems;
using Sandbox;

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
			RangeY = new Vector2( -5, 5 )
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

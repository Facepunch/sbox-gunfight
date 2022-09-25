namespace Facepunch.Gunfight;

public enum CrosshairType
{
	Default
}

public partial class CrosshairRender
{
	[ConVar.Client( "gunfight_crosshair_always_show" )]
	public static bool AlwaysShow { get; set; } = false;

	public Color StandardColor => ThemeColor;
	public Color DisabledColor => Color.Red;

	public static CrosshairRender From( CrosshairType type )
	{
		return type switch
		{
			CrosshairType.Default => new CrosshairRender(),
			_ => new CrosshairRender(),
		};
	}

	float alpha = 0;

	public Color ThemeColor => Color.Parse( "#ffffda" ) ?? Color.Red;
	public virtual void RenderCrosshair( in Vector2 center, float lastAttack, float lastReload, float speed, bool ads = false )
	{
		var draw = Render.Draw2D;

		speed = speed.LerpInverse( 0, 400, true );
		alpha = alpha.LerpTo( ads && !AlwaysShow ? 0 : 1, Time.Delta * 20f );

		var shootEase = Easing.EaseIn( lastAttack.LerpInverse( 0.2f, 0.0f ) );
		var color = Color.Lerp( DisabledColor, StandardColor, lastAttack.LerpInverse( 0.0f, 0.4f ) );

		draw.BlendMode = BlendMode.Lighten;
		draw.Color = color.WithAlpha( ( 0.4f + lastAttack.LerpInverse( 1.2f, 0 ) * 0.5f ) * alpha );

		var length = 12.0f - shootEase * 2.0f;
		var gap = 20.0f + shootEase * 30.0f;

		gap += 50 * speed;
		length += 8 * speed;

		var thickness = 2.0f;

		draw.Line( thickness, center + Vector2.Left * gap, center + Vector2.Left * (length + gap) );
		draw.Line( thickness, center - Vector2.Left * gap, center - Vector2.Left * (length + gap) );

		draw.Line( thickness, center + Vector2.Up * gap, center + Vector2.Up * (length + gap) );
		draw.Line( thickness, center - Vector2.Up * gap, center - Vector2.Up * (length + gap) );

		var reload = lastReload.Clamp( 0, 1 );
		if ( reload < 1f && reload > 0f )
		{
			draw.BlendMode = BlendMode.Normal;
			draw.Color = Color.Black.WithAlpha( 0.3f );

			var circleSize = 13f;
			var startAng = -0 * 360f;
			var finishAng = 1f * 360f;

			var offset = 30f + (30f * speed);
			var circleCenter = center + Vector2.Up * offset + Vector2.Left * offset;

			draw.Color = ThemeColor.WithAlpha( 0.01f );
			draw.Circle( circleCenter, circleSize, 32 );
			draw.Color = ThemeColor.WithAlpha( 1f );
			draw.CircleEx( circleCenter, circleSize, 0, 32, startAng, finishAng * lastReload );

		}

		draw.Circle( center, thickness, 32 );
	}
}


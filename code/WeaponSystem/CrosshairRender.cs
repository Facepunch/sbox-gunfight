namespace Facepunch.Gunfight;

public enum CrosshairType
{
	Default
}

public partial class CrosshairRender
{
	public Color StandardColor => Color.White;
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
	public virtual void RenderCrosshair( in Vector2 center, float lastAttack, float lastReload, float speed, bool ads = false )
	{
		var draw = Render.Draw2D;

		speed = speed.LerpInverse( 0, 400, true );
		alpha = alpha.LerpTo( ads ? 0 : 1, Time.Delta * 20f );

		var shootEase = Easing.EaseIn( lastAttack.LerpInverse( 0.2f, 0.0f ) );
		var color = Color.Lerp( DisabledColor, StandardColor, lastReload.LerpInverse( 0.0f, 0.4f ) );

		draw.BlendMode = BlendMode.Lighten;
		draw.Color = color.WithAlpha( ( 0.4f + lastAttack.LerpInverse( 1.2f, 0 ) * 0.5f ) * alpha );

		var length = 8.0f - shootEase * 2.0f;
		var gap = 10.0f + shootEase * 30.0f;

		gap += 20 * speed;
		length += 4 * speed;

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

			var circleSize = 34f + ( 20 * speed );
			var startAng = -0 * 360f;
			var finishAng = 0.75f * 360f;

			draw.CircleEx( center, circleSize, circleSize - 4f, 32, startAng, finishAng );
			draw.Color = Color.White;
			draw.CircleEx( center, circleSize, circleSize - 4f, 32, startAng, finishAng * lastReload );
		}
	}
}


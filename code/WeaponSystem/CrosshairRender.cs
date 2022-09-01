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

	public virtual void RenderCrosshair( in Vector2 center, float lastAttack, float lastReload, float speed )
	{
		speed = speed.LerpInverse( 0, 400, true );

		var draw = Render.Draw2D;

		var shootEase = Easing.EaseIn( lastAttack.LerpInverse( 0.2f, 0.0f ) );
		var color = Color.Lerp( DisabledColor, StandardColor, lastReload.LerpInverse( 0.0f, 0.4f ) );

		draw.BlendMode = BlendMode.Lighten;
		draw.Color = color.WithAlpha( 0.4f + lastAttack.LerpInverse( 1.2f, 0 ) * 0.5f );

		var length = 8.0f - shootEase * 2.0f;
		var gap = 10.0f + shootEase * 30.0f;

		gap += 20 * speed;
		length += 4 * speed;

		var thickness = 2.0f;

		draw.Line( thickness, center + Vector2.Left * gap, center + Vector2.Left * (length + gap) );
		draw.Line( thickness, center - Vector2.Left * gap, center - Vector2.Left * (length + gap) );

		draw.Line( thickness, center + Vector2.Up * gap, center + Vector2.Up * (length + gap) );
		draw.Line( thickness, center - Vector2.Up * gap, center - Vector2.Up * (length + gap) );
	}
}


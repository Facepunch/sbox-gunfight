namespace Facepunch.Gunfight;

public partial class PistolCrosshair : CrosshairRender
{
	float alpha = 0;

	public override void RenderCrosshair( Vector2 center, float lastAttack, float lastReload, float speed, bool ads = false )
	{
		if ( !GunfightCamera.Target.IsValid() )
			return;

		var ctrl = GunfightCamera.Target.Controller as PlayerController;
		if ( ctrl == null )
			return;

		var walkBob = MathF.Sin( Time.Now * 5f ) * GunfightCamera.Target.Velocity.Length.LerpInverse( 0, 350 );

		var newCenter = center + new Vector2( walkBob * -5, walkBob * 0.5f );

		var draw = Render.Draw2D;

		var dontShow = ads && !AlwaysShow || GunfightHud.HudState == HudVisibilityState.Invisible;

		speed = speed.LerpInverse( 0, 400, true );
		alpha = alpha.LerpTo( dontShow ? 0 : 1, Time.Delta * 20f );

		var shootEase = Easing.EaseIn( lastAttack.LerpInverse( 0.2f, 0.0f ) );
		var color = Color.Lerp( DisabledColor, StandardColor, lastAttack.LerpInverse( 0.0f, 0.4f ) );
		var regularColor = color.WithAlpha( (0.4f + lastAttack.LerpInverse( 1.2f, 0 ) * 0.5f) * alpha );

		draw.BlendMode = BlendMode.Lighten;
		draw.Color = regularColor;

		var length = 12.0f - shootEase * 2.0f;
		var gap = 20.0f + shootEase * 30.0f;

		gap += 50 * speed;
		length += 8 * speed;

		var thickness = 2.0f;

		var hideLines = ctrl.IsSprinting || !GunfightCamera.Target.GroundEntity.IsValid();
		if ( !hideLines )
		{
			draw.Line( thickness, newCenter + Vector2.Left * gap, newCenter + Vector2.Left * (length + gap) );
			draw.Line( thickness, newCenter - Vector2.Left * gap, newCenter - Vector2.Left * (length + gap) );

			draw.Line( thickness, newCenter + Vector2.Up * gap, newCenter + Vector2.Up * (length + gap) );
		}

		var reload = lastReload.Clamp( 0, 1 );
		if ( reload < 1f && reload > 0f )
		{
			draw.BlendMode = BlendMode.Normal;
			draw.Color = Color.Black.WithAlpha( 0.3f );

			var circleSize = 13f;
			var startAng = -0 * 360f;
			var finishAng = 1f * 360f;

			var offset = 30f + (30f * speed);
			var circleCenter = newCenter + Vector2.Up * offset + Vector2.Left * offset;

			draw.Color = ThemeColor.WithAlpha( 0.01f );
			draw.Circle( circleCenter, circleSize, 32 );
			draw.Color = ThemeColor.WithAlpha( 1f );
			draw.CircleEx( circleCenter, circleSize, 0, 32, startAng, finishAng * lastReload );

		}

		draw.Circle( newCenter, thickness, 32 );
	}
}


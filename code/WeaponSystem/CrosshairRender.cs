using Microsoft.Win32.SafeHandles;

namespace Facepunch.Gunfight;

public enum CrosshairType
{
	Default,
	Pistol,
	Shotgun
}

public partial class CrosshairRender
{
	[ConVar.Client( "gunfight_crosshair_always_show" )]
	public static bool AlwaysShow { get; set; } = false;

	public virtual Color StandardColor => ThemeColor;
	public virtual Color DisabledColor => Color.Red;
	public Color ThemeColor => Color.Parse( "#ffffda" ) ?? Color.Red;

	public static CrosshairRender From( CrosshairType type )
	{
		return type switch
		{
			_ => new CrosshairRender()
		};
	}

	float alpha = 0;

	public virtual void RenderCrosshair( Vector2 center, float lastAttack, float lastReload, float speed, bool ads = false )
	{
	}
}


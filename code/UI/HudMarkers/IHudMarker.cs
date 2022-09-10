namespace Facepunch.Gunfight;

public class HudMarkerBuilder
{
	public Dictionary<string, bool> Classes { get; set; } = new();
	public Vector3 Position { get; set; } = new();
	public Rotation Rotation { get; set; } = new();
	public string Text { get; set; } = "";
	public bool StayOnScreen { get; set; } = false;

	public float DistanceScale { get; set; } = 1;
	public float MaxDistance { get; set; } = 0;
}

public interface IHudMarker
{
	public string GetClass();
	public bool UpdateMarker( ref HudMarkerBuilder builder );
}

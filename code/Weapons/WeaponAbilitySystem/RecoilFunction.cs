namespace Gunfight;

public partial class RecoilFunction : WeaponFunction
{
	[Property] public Vector2 HorizontalRecoil { get; set; }
	[Property] public Vector2 VerticalRecoil { get; set; }

	Angles CurrentFrame;

	/// <summary>
	/// get the recoil to pass to the camera this frame
	/// </summary>
	/// <returns></returns>
	public Angles GetFrame()
	{
		var frame = CurrentFrame;
		return frame;
	}

	internal void Shoot()
	{
		var timeDelta = Time.Delta;
		CurrentFrame = new( VerticalRecoil.GetBetween() * timeDelta, HorizontalRecoil.GetBetween() * timeDelta, 0 );
	}

	protected override void OnUpdate()
	{
		CurrentFrame = CurrentFrame.LerpTo( Angles.Zero, Time.Delta * 10f );
	}

	internal override void UpdateStats()
	{
		HorizontalRecoil = Stats.HorizontalRecoil;
		VerticalRecoil = Stats.VerticalRecoil;
	}
}

public static class Vector2Extensions
{
	public static float GetBetween( this Vector2 self )
	{
		return Game.Random.Float( self.x, self.y );
	}
}

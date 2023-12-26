namespace Gunfight;

public partial class RecoilFunction : WeaponFunction
{
	[Property, Category( "Recoil" )] public RecoilPattern RecoilPattern { get; set; }
	[Property, Category( "Recoil" )] public float HorizontalScale { get; set; } = 1f;
	[Property, Category( "Recoil" )] public float VerticalScale { get; set; } = 1f;

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

	TimeSince TimeSinceLastShot;
	int currentFrame = 0;

	internal void Shoot()
	{
		if (  TimeSinceLastShot > 1f  )
		{
			currentFrame = 0;
		}

		TimeSinceLastShot = 0;

		var timeDelta = Time.Delta;
		var point = RecoilPattern.GetPoint( currentFrame );
		var newAngles = new Angles( -point.y * VerticalScale * timeDelta, point.x * HorizontalScale * timeDelta, 0 );
		CurrentFrame = CurrentFrame + newAngles;

		currentFrame++;
	}

	protected override void OnUpdate()
	{
		CurrentFrame = CurrentFrame.LerpTo( Angles.Zero, Time.Delta * 10f );
	}

	internal override void UpdateStats()
	{
		//HorizontalRecoil = Stats.HorizontalRecoil;
		//VerticalRecoil = Stats.VerticalRecoil;
	}
}

public static class Vector2Extensions
{
	public static float GetBetween( this Vector2 self )
	{
		return Game.Random.Float( self.x, self.y );
	}
}

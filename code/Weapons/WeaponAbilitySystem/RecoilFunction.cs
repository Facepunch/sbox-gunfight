namespace Gunfight;

public partial class RecoilFunction : WeaponFunction
{
	[Property, Category( "Recoil" )] public RecoilPattern RecoilPattern { get; set; }
	[Property, Category( "Recoil" )] public float RecoilResetTime { get; set; } = 0.3f;
	[Property, Category( "Recoil" )] public Vector2 HorizontalSpread { get; set; } = 0f;
	[Property, Category( "Recoil" )] public Vector2 VerticalSpread { get; set; } = 0f;


	[Property, Category( "Scaling" )] public float PlayerVelocityLimit { get; set; } = 300f;
	[Property, Category( "Scaling" )] public float PlayerVelocitySpread { get; set; } = 1f;

	internal Angles Current { get; private set; }

	TimeSince TimeSinceLastShot;
	int currentFrame = 0;

	private float HorizontalScale
	{
		get
		{
			var scale = 1f;
			// TODO: better accessor for stuff like this, this is mega shit
			var velLen = Weapon.PlayerController.CharacterController.Velocity.Length;
			scale += velLen.Remap( 0, PlayerVelocityLimit, 0, 1, true ) * PlayerVelocitySpread;

			return scale;
		}
	}

	private float VerticalScale
	{
		get
		{
			var scale = 1f;
			// TODO: better accessor for stuff like this, this is mega shit
			var velLen = Weapon.PlayerController.CharacterController.Velocity.Length;
			scale += velLen.Remap( 0, PlayerVelocityLimit, 0, 1, true ) * PlayerVelocitySpread;

			Log.Info( scale );

			return scale;
		}
	}

	internal void Shoot()
	{
		if ( TimeSinceLastShot > RecoilResetTime )
		{
			RecoilPattern.Reset();
			currentFrame = 0;
		}

		TimeSinceLastShot = 0;

		var timeDelta = Time.Delta;
		var point = RecoilPattern.GetPoint( ref currentFrame );
		var newAngles = new Angles( ( -point.y - VerticalSpread.GetBetween() ) * VerticalScale * timeDelta, ( point.x + HorizontalSpread.GetBetween() ) * HorizontalScale * timeDelta, 0 );

		Current = Current + newAngles;
		currentFrame++;
	}

	protected override void OnUpdate()
	{
		Current = Current.LerpTo( Angles.Zero, Time.Delta * 10f );
	}

	internal override void UpdateStats()
	{
		HorizontalSpread = Stats.HorizontalSpread;
		VerticalSpread = Stats.VerticalSpread;
	}
}

public static class Vector2Extensions
{
	public static float GetBetween( this Vector2 self )
	{
		return Game.Random.Float( self.x, self.y );
	}
}

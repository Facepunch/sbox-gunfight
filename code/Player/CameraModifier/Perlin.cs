namespace Facepunch.Gunfight.ScreenShake;

public class Perlin : CameraModifier
{
	float Length;
	float Speed;
	float Size;
	float RotationAmount;
	float NoiseZ;

	TimeSince lifeTime = 0;
	float pos = 0;

	public Perlin( float length = 1.0f, float speed = 1.0f, float size = 1.0f, float rotation = 0.6f )
	{
		Length = length;
		Speed = speed;
		Size = size;
		RotationAmount = rotation;
		NoiseZ = Rand.Float( -10000, 10000 );

		pos = Rand.Float( 0, 100000 );
	}

	public override bool Update( ref CameraSetup cam )
	{
		var delta = ((float)lifeTime).LerpInverse( 0, Length, true );
		delta = Easing.EaseOut( delta );
		var invdelta = 1 - delta;

		pos += Time.Delta * 10 * invdelta * Speed;

		float x = Noise.Perlin( pos, 0, NoiseZ );
		float y = Noise.Perlin( pos, 3.0f, NoiseZ );

		cam.Position += (cam.Rotation.Right * x + cam.Rotation.Up * y) * invdelta * Size;
		cam.Rotation *= Rotation.FromAxis( Vector3.Up, x * Size * invdelta * RotationAmount );
		cam.Rotation *= Rotation.FromAxis( Vector3.Right, y * Size * invdelta * RotationAmount );

		return lifeTime < Length;
	}
}

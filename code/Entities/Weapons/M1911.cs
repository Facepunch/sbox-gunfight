namespace Facepunch.Gunfight;

[Library( "gunfight_m1911" ), HammerEntity]
[EditorModel( "models/m1911/w_m1911.vmdl" )]
[Title( "M1911" ), Category( "Weapons" )]
partial class M1911 : GunfightWeapon
{
	public static readonly Model WorldModel = Model.Load( "models/m1911/w_m1911.vmdl" );
	public override string ViewModelPath => "models/m1911/fp_m1911.vmdl";

	public override float ReloadTime => 3.0f;

	public override int Bucket => 0;

	public override void Spawn()
	{
		base.Spawn();

		Model = WorldModel;
		AmmoClip = 12;
	}

	public override bool CanPrimaryAttack()
	{
		return base.CanPrimaryAttack() && Input.Pressed( InputButton.PrimaryAttack );
	}

	public override void AttackPrimary()
	{
		TimeSincePrimaryAttack = 0;

		if ( !TakeAmmo( 1 ) )
		{
			DryFire();

			if ( AvailableAmmo() > 0 )
				Reload();

			return;
		}

		//
		// Tell the clients to play the shoot effects
		//
		ShootEffects();
		PlaySound( "pistol_shoot" );

		//
		// Shoot the bullets
		//
		ShootBullet( 0.05f, 1, 12.0f, 2.0f );

	}

	public override void RenderCrosshair( in Vector2 center, float lastAttack, float lastReload )
	{
		var draw = Render.Draw2D;

		var shootEase = Easing.EaseIn( lastAttack.LerpInverse( 0.2f, 0.0f ) );
		var color = Color.Lerp( Color.Red, Color.White, lastReload.LerpInverse( 0.0f, 0.4f ) );

		draw.BlendMode = BlendMode.Lighten;
		draw.Color = color.WithAlpha( 0.2f + lastAttack.LerpInverse( 1.2f, 0 ) * 0.5f );

		var length = 8.0f - shootEase * 2.0f;
		var gap = 10.0f + shootEase * 30.0f;
		var thickness = 2.0f;

		draw.Line( thickness, center + Vector2.Left * gap, center + Vector2.Left * (length + gap) );
		draw.Line( thickness, center - Vector2.Left * gap, center - Vector2.Left * (length + gap) );

		draw.Line( thickness, center + Vector2.Up * gap, center + Vector2.Up * (length + gap) );
		draw.Line( thickness, center - Vector2.Up * gap, center - Vector2.Up * (length + gap) );
	}

}

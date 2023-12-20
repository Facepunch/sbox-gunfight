namespace Gunfight;

[Title( "Shoot Weapon Ability" ), Icon( "track_changes" )]
public partial class ShootWeaponAbility : InputActionWeaponAbility
{
	public override string Name => "Shoot";

	[Property, Category( "Bullet" )] public float BaseDamage { get; set; } = 25.0f;

	[Property, Category( "Bullet" )] public float WeaponShootDelay { get; set; } = 0.2f;

	[Property, Category( "Bullet" )] public float MaxEffectiveRange { get; set; } = 1024000;

	[Property, Category( "Bullet" )] public float BulletSize { get; set; } = 1.0f;

	[Property, Category( "Effects" )] public ParticleSystem MuzzleEffect { get; set; }
	[Property, Category( "Effects" )] public ParticleSystem TrailEffect { get; set; }
	[Property, Category( "Effects" )] public SoundFile Sound { get; set; }

	protected override void OnWeaponAbility()
	{
		Log.Info( "BANG!" );
	}
}

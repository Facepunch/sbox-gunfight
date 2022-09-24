namespace Facepunch.Gunfight;

[Library( "laser" )]
public partial class LaserSightAttachment : BarrelAddonAttachment
{
	public override Model AttachmentModel => Model.Load( "models/attachments/laser/laser_sight.vmdl" );
	public override string AimAttachment => "aim";

	public Particles LaserParticles { get; private set; }
	public Particles DotParticles { get; private set; }

	public Color LaserColor { get; set; } = new( 1f, 0, 0, 1 );

	public override void OnNewModel( Model model )
	{
		base.OnNewModel( model );

		if ( IsClient )
		{
			DotParticles = Particles.Create( "particles/laserdot.vpcf" );
			DotParticles.SetEntity( 0, this, true );

			LaserParticles = Particles.Create( "particles/laserline.vpcf" );
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		if ( IsClient )
		{
			DotParticles?.Destroy( true );
			LaserParticles?.Destroy( true );
		}
	}

	protected void Update()
	{
		if ( IsClient )
		{
			var attachment = Weapon.EffectEntity.GetAttachment( "laser" );
			if ( !attachment.HasValue ) return;

			var position = attachment.Value.Position;
			var rotation = Weapon.Owner.EyeRotation;

			if ( Weapon.IsReloading )
			{
				rotation = attachment.Value.Rotation;
			}

			var trace = Trace.Ray( position, position + rotation.Forward * 4096f )
				.UseHitboxes()
				.Radius( 2f )
				.Ignore( Weapon )
				.Ignore( this )
				.Run();

			var start = attachment.Value.Position;
			var end = trace.EndPosition;

			LaserParticles.SetPosition( 2, LaserColor * 255f );
			DotParticles.SetPosition( 2, LaserColor * 255f );

			DotParticles.SetPosition( 0, end );

			LaserParticles.SetPosition( 0, start );
			LaserParticles.SetPosition( 1, end );
		}
	}

	public override void FrameSimulate( Client cl )
	{
		Update();
	}
}

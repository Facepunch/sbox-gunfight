namespace Facepunch.Gunfight;

[Library( "laser" )]
public partial class LaserSightAttachment : BarrelAddonAttachment
{
	public override Model AttachmentModel => Model.Load( "models/attachments/laser/laser_sight.vmdl" );
	public override string AimAttachment => "laser_aim";
	public override AimAttachmentStyle AimAttachmentStyle => AimAttachmentStyle.OnViewModel;

	public Particles LaserParticles { get; private set; }
	public Particles DotParticles { get; private set; }

	public Color LaserColor { get; set; } = new( 1f, 0, 0, 1 );

	public override void OnNewModel( Model model )
	{
		base.OnNewModel( model );

		if ( IsClient )
		{
			DotParticles = Particles.Create( "particles/laserdot.vpcf" );
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

	[Event.Frame]
	protected void Update()
	{
		if ( Weapon.EnableDrawing )
		{
			DotParticles ??= Particles.Create( "particles/laserdot.vpcf" );
			LaserParticles ??= Particles.Create( "particles/laserline.vpcf" );

			var attachment = EffectEntity.GetAttachment( "laser", true );
			if ( !attachment.HasValue ) return;

			var position = attachment.Value.Position;
			var rotation = attachment.Value.Rotation;

			var player = Weapon.Owner as GunfightPlayer;
			if ( !player.IsValid() || player != Local.Pawn || Weapon.TimeSinceDeployed < 1f || player.IsHolstering )
			{
				// Do nothing
			}
			else
			{
				rotation = player.EyeRotation;

				var ctrl = player.Controller as PlayerController;
				if ( ctrl != null && Weapon.IsValid() && Weapon.IsReloading || ctrl.IsSprinting || ctrl.Slide.IsActive )
				{
					rotation = attachment.Value.Rotation;
				}
			}

			var trace = Trace.Ray( position, position + rotation.Forward * 4096f )
				.UseHitboxes()
				.WithAnyTags( "solid", "player" )
				.Radius( 1 )
				.Ignore( Weapon )
				.Ignore( EffectEntity )
				.Run();

			var start = attachment.Value.Position;
			var end = trace.EndPosition;

			LaserParticles.SetPosition( 2, LaserColor * 255f );
			DotParticles.SetPosition( 2, LaserColor * 255f );

			DotParticles.SetPosition( 0, end );

			LaserParticles.SetPosition( 0, start );
			LaserParticles.SetPosition( 1, end );
		}
		else
		{
			DotParticles?.Destroy( true );
			DotParticles = null;
			LaserParticles?.Destroy( true );
			LaserParticles = null;
		}
	}
}

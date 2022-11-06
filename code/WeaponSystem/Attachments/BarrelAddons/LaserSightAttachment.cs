namespace Facepunch.Gunfight;

[Library( "laser"), Title( "Laser Sight" )]
public partial class LaserSightAttachment : BarrelAddonAttachment
{
	public override Model AttachmentModel => Model.Load( "models/attachments/laser/laser_sight.vmdl" );
	public override string AimAttachment => "laser_aim";
	public override AimAttachmentStyle AimAttachmentStyle => AimAttachmentStyle.OnViewModel;
	public override float AimSpeedModifier => -0.1f;

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

	// Where the trace starts
	protected Vector3 TraceStartPosition;
	// Where the trace ends if we're going forward from the Attachment
	protected Vector3 AttachmentEndPosition;
	// Where the trace ends if we're going forward from the Attachment towards the player's EyeRotation
	protected Vector3 EyeEndPosition;
	// When we decide that the laser needs to move, we'll set this and use the fraction to lerp between both Vectors
	protected TimeUntil UntilPositionChanged;
	// Stored state for checking the last frame's decision
	protected bool NoEyeOrigin;

	protected void UpdateAttachmentTrace()
	{
		var attachment = EffectEntity.GetAttachment( "laser", true );
		if ( !attachment.HasValue ) return;

		var pos = attachment.Value.Position;
		var rot = attachment.Value.Rotation;

		var trace = Trace.Ray( pos, pos + rot.Forward * 4096f )
			.UseHitboxes()
			.WithAnyTags( "solid", "player" )
			.Radius( 1 )
			.Ignore( Weapon )
			.Ignore( EffectEntity )
			.Run();
		
		TraceStartPosition = pos;
		AttachmentEndPosition = trace.EndPosition;
	}

	protected void UpdateEyeTrace()
	{
		if ( !Weapon.IsValid() || !Weapon.Owner.IsValid() ) return;

		var pos = TraceStartPosition;
		var rot = Weapon.Owner.EyeRotation;

		var trace = Trace.Ray( pos, pos + rot.Forward * 4096f )
			.UseHitboxes()
			.WithAnyTags( "solid", "player" )
			.Radius( 1 )
			.Ignore( Weapon )
			.Ignore( EffectEntity )
			.Run();

		EyeEndPosition = trace.EndPosition;
	}

	[Event.Frame]
	protected void Update()
	{
		if ( Weapon.IsValid() && Weapon.EnableDrawing )
		{
			DotParticles ??= Particles.Create( "particles/laserdot.vpcf" );
			LaserParticles ??= Particles.Create( "particles/laserline.vpcf" );

			UpdateAttachmentTrace();
			UpdateEyeTrace();

			var player = Weapon.Owner as GunfightPlayer;
			var ctrl = player?.Controller as PlayerController;
			var noEyeOrigin = ( Weapon.IsValid() && Weapon.IsReloading ) || ctrl != null && ( ctrl.IsSprinting || ctrl.Slide.IsActive );

			// Laser end position preference changed
			if ( NoEyeOrigin != noEyeOrigin )
			{
				NoEyeOrigin = noEyeOrigin;
				UntilPositionChanged = 0.2f;
			}

			// What the fuck am I doing here
			var end = Vector3.Lerp( 
				NoEyeOrigin ? EyeEndPosition : AttachmentEndPosition, 
				NoEyeOrigin ? AttachmentEndPosition : EyeEndPosition, 
				UntilPositionChanged.Fraction 
			);
			
			LaserParticles.SetPosition( 2, LaserColor * 255f );
			DotParticles.SetPosition( 2, LaserColor * 255f );
			DotParticles.SetPosition( 0, end );
			LaserParticles.SetPosition( 0, TraceStartPosition );
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

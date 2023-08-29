namespace Facepunch.Gunfight;

[Library( "mp5" )]
public partial class MP5 : GunfightWeapon
{
	public static Model MP5Model = Cloud.Model( "https://asset.party/facepunch/w_mp5" );
	static Model MP5ViewModel = Cloud.Model( "https://asset.party/facepunch/v_mp5" );
	
	public override Model WeaponModel => MP5Model;
	public override Model WeaponViewModel => MP5ViewModel;

	public override void Spawn()
	{
		base.Spawn();
		LocalScale = 1.5f;
	}

	protected override void InitializeWeapon( WeaponDefinition def )
	{
		base.InitializeWeapon( def );

		if ( Game.IsServer )
		{
			SetAttachment( "mp5_rail", true );
		}
	}

	public override ViewModel CreateViewModel()
	{
		ViewModelEntity = new ViewModel();
		ViewModelEntity.Weapon = this;
		ViewModelEntity.Position = Position;
		ViewModelEntity.Owner = Owner;
		ViewModelEntity.EnableViewmodelRendering = true;
		//ViewModelEntity.SetModel( "models/weapons/sbox_smg_mp5/v_mp5.vmdl" );
		ViewModelEntity.Model = MP5ViewModel;

		var arms = new AnimatedEntity( "models/first_person/first_person_arms.vmdl" );
		arms.SetParent( ViewModelEntity, true );
		arms.EnableViewmodelRendering = true;

		return ViewModelEntity;
	}

	public override void Simulate( IClient cl )
	{
		base.Simulate( cl );

		var held = AmmoClip > 0 && IsTriggerHeld ? TimeSinceStartFiring : 0;
		
		(Owner as AnimatedEntity)?.SetAnimParameter( "attack_hold", held );
		ViewModelEntity?.SetAnimParameter( "attack_hold", held );

		if ( (Owner as GunfightPlayer)?.IsAiming ?? false )
		{
			ViewModelEntity?.SetAnimParameter( "ironsights", 1 );
			ViewModelEntity?.SetAnimParameter( "ironsights_fire_scale", 0.3f );
		}
		else
		{
			ViewModelEntity?.SetAnimParameter( "ironsights", 0 );
			ViewModelEntity?.SetAnimParameter( "ironsights_fire_scale", 0.5f );
		}
	}
}

namespace Facepunch.Gunfight;

[Library( "usp" )]
public partial class USP : GunfightWeapon
{
	private static Model USPModel = Cloud.Model( "https://asset.party/facepunch/w_usp" );
	private static Model USPViewModel = Cloud.Model( "https://asset.party/facepunch/v_usp" );
	
	public override Model WeaponModel => USPModel;
	public override Model WeaponViewModel => USPViewModel;

	public override void Spawn()
	{
		base.Spawn();
		LocalScale = 1.5f;
	}

	protected override void InitializeWeapon( WeaponDefinition def )
	{
		base.InitializeWeapon( def );
			
		SetBodyGroup( 2, 1 );
		SetBodyGroup( 4, 1 );
	}

	public override ViewModel CreateViewModel()
	{
		ViewModelEntity = new ViewModel();
		ViewModelEntity.Weapon = this;
		ViewModelEntity.Position = Position;
		ViewModelEntity.Owner = Owner;
		ViewModelEntity.EnableViewmodelRendering = true;
		ViewModelEntity.Model = USPViewModel;

		var arms = new AnimatedEntity( "models/first_person/first_person_arms.vmdl" );
		arms.SetParent( ViewModelEntity, true );
		arms.EnableViewmodelRendering = true;
		
		ViewModelEntity.SetBodyGroup( 2, 1 );
		ViewModelEntity.SetBodyGroup( 4, 2 );

		return ViewModelEntity;
	}
}

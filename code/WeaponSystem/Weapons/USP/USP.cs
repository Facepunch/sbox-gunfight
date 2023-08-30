namespace Facepunch.Gunfight;

[Library( "usp" )]
public partial class USP : GunfightWeapon
{
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
			SetAttachment( "usp_rmr", true );
			SetAttachment( "usp_sd", true );
		}
	}

	public override ViewModel CreateViewModel()
	{
		ViewModelEntity = new ViewModel();
		ViewModelEntity.Weapon = this;
		ViewModelEntity.Position = Position;
		ViewModelEntity.Owner = Owner;
		ViewModelEntity.EnableViewmodelRendering = true;
		ViewModelEntity.Model = WeaponDefinition.CachedViewModel;

		var arms = new AnimatedEntity( "models/first_person/first_person_arms.vmdl" );
		arms.SetParent( ViewModelEntity, true );
		arms.EnableViewmodelRendering = true;
		
		return ViewModelEntity;
	}
}

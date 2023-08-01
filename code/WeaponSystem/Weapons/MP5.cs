namespace Facepunch.Gunfight;

[Library( "mp5" )]
public partial class MP5 : GunfightWeapon
{
	private static Model MP5Model = Cloud.Model( "https://asset.party/facepunch/w_mp5" );
	private static Model MP5ViewModel = Cloud.Model( "https://asset.party/facepunch/v_mp5" );
	
	public override Model WeaponModel => MP5Model;
	public override Model WeaponViewModel => MP5ViewModel;
	
	public override void CreateViewModel()
	{
		ViewModelEntity = new ViewModel();
		ViewModelEntity.Weapon = this;
		ViewModelEntity.Position = Position;
		ViewModelEntity.Owner = Owner;
		ViewModelEntity.EnableViewmodelRendering = true;
		ViewModelEntity.Model = MP5ViewModel;

		var arms = new AnimatedEntity( "models/first_person/first_person_arms.vmdl" );
		arms.SetParent( ViewModelEntity, true );
		arms.EnableViewmodelRendering = true;
		
		Log.Info(  MP5ViewModel );
		
		Log.Info(arms.Parent);
	}
	public override void Simulate( IClient cl )
	{
		base.Simulate( cl );
		
		(Owner as AnimatedEntity)?.SetAnimParameter( "attack_hold", IsTriggerHeld );
		ViewModelEntity?.SetAnimParameter( "attack_hold", IsTriggerHeld );
	}
}

namespace Facepunch.Gunfight;

[Title( "Weapon" ), Icon( "luggage" )]
public partial class BaseWeapon : BaseCarriable
{
	public virtual WeaponSlot Slot => WeaponDefinition?.Slot ?? WeaponSlot.Primary;


	[Net, Change( nameof( OnWeaponDefinitionChanged ) )]
	protected WeaponDefinition _WeaponDefinition { get; set; }
	public WeaponDefinition WeaponDefinition
	{
		get
		{
			return _WeaponDefinition;
		}
		set
		{
			_WeaponDefinition = value;
			InitializeWeapon( _WeaponDefinition );
		}
	}

	protected void OnWeaponDefinitionChanged( WeaponDefinition oldDef, WeaponDefinition newDef )
	{
		InitializeWeapon( newDef );
	}

	protected virtual void InitializeWeapon( WeaponDefinition def )
	{
		Log.Info( $"{Host.Name}: Set up this weapon {def}" );
		Model = def.CachedModel;
	}

	public override void Spawn()
	{
		base.Spawn();

		Tags.Add( "weapon", "debris" );
		PhysicsEnabled = true;
		UsePhysicsCollision = true;
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;
	}

	public override bool CanCarry( Entity carrier )
	{
		return true;
	}

	public override void OnCarryStart( Entity carrier )
	{
		if ( IsClient ) return;

		SetParent( carrier, true );
		Owner = carrier;
		EnableAllCollisions = false;
		EnableDrawing = false;
	}

	public override void SimulateAnimator( PawnAnimator anim )
	{
		anim.SetAnimParameter( "holdtype", WeaponDefinition?.HoldType.ToInt() ?? 1 );
		anim.SetAnimParameter( "aim_body_weight", 1.0f );
		anim.SetAnimParameter( "holdtype_handedness", 0 );
	}

	public override void OnCarryDrop( Entity dropper )
	{
		if ( IsClient ) return;

		SetParent( null );
		Owner = null;
		EnableDrawing = true;
		EnableAllCollisions = true;
	}

	/// <summary>
	/// This entity has become the active entity. This most likely
	/// means a player was carrying it in their inventory and now
	/// has it in their hands.
	/// </summary>
	public override void ActiveStart( Entity ent )
	{
		EnableDrawing = true;

		if ( ent is Player player )
		{
			var animator = player.GetActiveAnimator();
			if ( animator != null )
			{
				SimulateAnimator( animator );
			}
		}

		//
		// If we're the local player (clientside) create viewmodel
		// and any HUD elements that this weapon wants
		//
		if ( IsLocalPawn )
		{
			DestroyViewModel();
			DestroyHudElements();

			CreateViewModel();
			CreateHudElements();
		}
	}

	/// <summary>
	/// This entity has stopped being the active entity. This most
	/// likely means a player was holding it but has switched away
	/// or dropped it (in which case dropped = true)
	/// </summary>
	public override void ActiveEnd( Entity ent, bool dropped )
	{
		//
		// If we're just holstering, then hide us
		//
		if ( !dropped )
		{
			EnableDrawing = false;
		}

		if ( IsClient )
		{
			DestroyViewModel();
			DestroyHudElements();
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		if ( IsClient && ViewModelEntity.IsValid() )
		{
			DestroyViewModel();
			DestroyHudElements();
		}
	}

	/// <summary>
	/// Create the viewmodel. You can override this in your base classes if you want
	/// to create a certain viewmodel entity.
	/// </summary>
	public override void CreateViewModel()
	{
		Host.AssertClient();

		ViewModelEntity?.Delete();

		var vm = new ViewModel();
		ViewModelEntity = vm;
		vm.Weapon = this;

		if ( WeaponDefinition != null )
			vm.Model = WeaponDefinition.CachedViewModel;

		ViewModelEntity.Position = Position;
		ViewModelEntity.Owner = Owner;
		ViewModelEntity.EnableViewmodelRendering = true;

		vm.Initialize();
	}

	/// <summary>
	/// We're done with the viewmodel - delete it
	/// </summary>
	public override void DestroyViewModel()
	{
		ViewModelEntity?.Delete();
		ViewModelEntity = null;
	}

	public override void CreateHudElements()
	{
	}

	public override void DestroyHudElements()
	{

	}

	/// <summary>
	/// Utility - return the entity we should be spawning particles from etc
	/// </summary>
	public override ModelEntity EffectEntity => (ViewModelEntity.IsValid() && IsFirstPersonMode) ? ViewModelEntity : this;
}

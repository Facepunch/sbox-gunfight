using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Facepunch.Gunfight.Entities.Hammer
{
	/// <summary>
	/// This ent is used to set if a model should be visible or not for certain map sizes depending on what the gamemode requires.
	/// Example, The 2v2 Gamemode will require a tiny map, so the model will be visible for that gamemode.
	/// </summary>
	
	[Model]
	[Library( "GF_PropGameMode" )]
	[HammerEntity, RenderFields]
	[VisGroup( VisGroup.Dynamic )]
	[Title( "Prop Gamemode" ), Category( "Gameplay" ), Icon( "brush" )]
	public partial class GamemodeProp : ModelEntity
	{

		[Net, Property( "spawnA", Title = "Large Map Should Spawn" )]
		public bool SpawnLarge { get; set; }

		[Net, Property( "spawnB", Title = "Medium Map Should Spawn" )]
		public bool SpawnMedium { get; set; }

		[Net, Property( "spawnC", Title = "Small Map Should Spawn" )]
		public bool SpawnSmall { get; set; }
		
		[Net, Property( "spawnD", Title = "Tiny Map Should Spawn" )]
		public bool SpawnTiny { get; set; }

		public override void Spawn()
		{
			base.Spawn();
			SetupPhysicsFromModel( PhysicsMotionType.Static );
			EnableAllCollisions = true;
		}
	}
}

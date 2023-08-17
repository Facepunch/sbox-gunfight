namespace Facepunch.Gunfight;

public partial class PlayerLevelComponent : PersistenceComponent
{
	[Net] public int Level { get; set; }
}

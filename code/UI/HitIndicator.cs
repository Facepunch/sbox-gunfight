using Sandbox.UI;

namespace Facepunch.Gunfight;

public partial class HitIndicator : Panel
{
	public static HitIndicator Current;

	public HitIndicator()
	{
		Current = this;
	}

	public override void Tick()
	{
		base.Tick();
	}

	public void OnHit( Vector3 pos, float amount )
	{
		new HitPoint( amount, pos, this );
	}

	public class HitPoint : Panel
	{
		public HitPoint( float amount, Vector3 pos, Panel parent )
		{
			Parent = parent;
			_ = Lifetime();
		}

		async Task Lifetime()
		{
			await Task.Delay( 200 );
			Delete();
		}
	}
}



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

	public void OnHit( Vector3 pos, float amount, bool isHeadshot )
	{
		new HitPoint( amount, pos, this, isHeadshot );
	}

	public class HitPoint : Panel
	{
		public HitPoint( float amount, Vector3 pos, Panel parent, bool isHeadshot )
		{
			Parent = parent;
			SetClass( "headshot", isHeadshot );
			_ = Lifetime();
		}

		async Task Lifetime()
		{
			await Task.Delay( 200 );
			Delete();
		}
	}
}



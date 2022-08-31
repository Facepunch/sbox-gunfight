using Sandbox.UI;

namespace Facepunch.Gunfight;

public partial class KillFeed : Panel
{
	public static KillFeed Current;

	public KillFeed()
	{
		Current = this;

		StyleSheet.Load( "/ui/killfeed/KillFeed.scss" );
	}

	public virtual Panel AddEntry( long lsteamid, string left, long rsteamid, string right, string method )
	{
		var e = Current.AddChild<KillFeedEntry>();

		e.Left.Text = left;
		e.Left.SetClass( "me", lsteamid == Local.PlayerId );

		e.AddClass( method );

		e.Method.Text = method;

		e.Right.Text = right;
		e.Right.SetClass( "me", rsteamid == Local.PlayerId );

		return e;
	}
}

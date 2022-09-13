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

	protected Client GetClient( long steamId )
	{
		return Client.All.FirstOrDefault( x => x.PlayerId == steamId );
	}

	public virtual Panel AddEntry( long lsteamid, string left, long rsteamid, string right, string method )
	{
		var e = Current.AddChild<KillFeedEntry>();

		e.Left.Text = left;
		e.Left.SetClass( "me", lsteamid == Local.PlayerId );

		e.AddClass( method );

		var gun = WeaponDefinition.Find( method );
		if ( gun != null )
		{
			e.Method.Text = gun.WeaponName;
		}
		else
			e.Method.Text = method;

		e.Right.Text = right;
		e.Right.SetClass( "me", rsteamid == Local.PlayerId );

		if ( lsteamid != 0 )
		{
			var leftFriendState = TeamSystem.GetFriendState( GetClient( lsteamid ), TeamSystem.MyTeam );
			e.Left.SetClass( "friendly", leftFriendState == TeamSystem.FriendlyStatus.Friendly );
			e.Left.SetClass( "enemy", leftFriendState == TeamSystem.FriendlyStatus.Hostile );
		}

		if ( rsteamid != 0 )
		{
			var rightFriendState = TeamSystem.GetFriendState( GetClient( rsteamid ), TeamSystem.MyTeam );
			e.Right.SetClass( "friendly", rightFriendState == TeamSystem.FriendlyStatus.Friendly );
			e.Right.SetClass( "enemy", rightFriendState == TeamSystem.FriendlyStatus.Hostile );
		}

		return e;
	}
}

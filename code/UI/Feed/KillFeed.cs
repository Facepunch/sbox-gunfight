using Sandbox.UI;

namespace Facepunch.Gunfight.UI;

public partial class KillFeed : Panel
{
	public static KillFeed Current;

	public KillFeed()
	{
		Current = this;

		StyleSheet.Load( "/ui/feed/KillFeed.scss" );
	}

	protected IClient GetClient( long steamId )
	{
		return Game.Clients.FirstOrDefault( x => x.SteamId == steamId );
	}

	public virtual Panel AddEntry( IClient attacker, IClient victim, GunfightWeapon weapon = null, string method = null, bool isHeadshot = false )
	{
		var e = Current.AddChild<KillFeedEntry>();

		e.Attacker = attacker;
		e.IsHeadshot = isHeadshot;

		var wpn = new CreateAClass.Weapon
		{
			Name = weapon.WeaponDefinition.WeaponShortName,
			Attachments = weapon.Attachments.Select( x => x.Identifier ).ToList()
		};

		e.Weapon = wpn;
		e.Victim = victim;

		return e;
	}
}

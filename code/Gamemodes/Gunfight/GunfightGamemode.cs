using Sandbox.UI;
using System.ComponentModel.DataAnnotations;

namespace Facepunch.Gunfight;

[Display( Name = "Gunfight Gamemode" )]
public partial class GunfightGamemode : GamemodeEntity
{
	public override Panel GetHudPanel() => new GunfightGamemodePanel();

	public override void OnClientJoined( Client cl )
	{
		var teamComponent = cl.Components.GetOrCreate<TeamComponent>();
		teamComponent.Team = TeamSystem.GetLowestCount();

		ChatBox.AddInformation( ToExtensions.Team( teamComponent.Team ), $"{cl.Name} joined {TeamSystem.GetTeamName( teamComponent.Team )}" );
	}
}

using Sandbox.UI;

namespace Facepunch.Gunfight;

[UseTemplate]
public partial class LoadoutPanel : Panel
{
	public static LoadoutPanel Current { get; set; }

	// @ref
	public Panel MainPanel { get; set; }

	[ConCmd.Client( "gunfight_debug_loadoutpanel" )]
	public static void Show()
	{
		if ( Current is not null ) Current.Delete( true );

		Current = GunfightGame.Current.Hud.RootPanel.AddChild<LoadoutPanel>();
	}

	[ClientRpc]
	public static void RpcShow()
	{
		LoadoutPanel.Show();
	}

	protected override void PostTemplateApplied()
	{
		Setup();
	}

	protected void Setup()
	{
		var gamemode = GamemodeSystem.Current as GunfightGamemode;
		if ( !gamemode.IsValid() ) return;

		if ( gamemode.CurrentLoadout is null ) return;

		MainPanel.DeleteChildren( true );

		if ( gamemode.CurrentLoadout.PrimaryWeapon.Definition != null )
			AddEntry( gamemode.CurrentLoadout.PrimaryWeapon );

		if ( gamemode.CurrentLoadout.SecondaryWeapon.Definition != null )
			AddEntry( gamemode.CurrentLoadout.SecondaryWeapon );
	}

	protected void AddEntry( LoadoutSlot slot )
	{
		var pnl = MainPanel.Add.Panel( "entry" );
		pnl.AddChild<Image>( "icon" ).SetTexture( slot.Definition.Icon );
		pnl.AddChild<Label>( "name" ).Text = slot.Definition.WeaponName;

		if ( slot.Attachments != null && slot.Attachments.Count > 0  )
		{
			pnl.AddClass( "has-attachments" );
			var attachmentContainer = pnl.Add.Panel( "attachments" );

			var title = attachmentContainer.AddChild<Label>( "title" );
			title.Text = "settings";

			foreach ( var attachment in slot.Attachments )
			{
				var library = TypeLibrary.GetDescription( attachment );
				if ( library is null ) continue;

				var attachmentPanel = attachmentContainer.Add.Panel( "attachment" );
				attachmentPanel.AddChild<Label>( "name" ).Text = library.Title;
			}
		}
	}

	TimeSince TimeSinceCreated = 0;
	public override void Tick()
	{
		if ( TimeSinceCreated > 10f && !IsDeleting )
		{
			Delete();
		}
	}
}

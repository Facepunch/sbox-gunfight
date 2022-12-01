using Sandbox.UI;

namespace Facepunch.Gunfight;

public class InventoryBar : Panel
{
	public bool IsOpen { get; private set; }

	private GunfightWeapon SelectedWeapon { get; set; }
	private List<GunfightWeapon> Weapons { get; set; } = new();
	private GunfightWeapon LastWeapon { get; set; }
	private Panel Container { get; set; }

	public InventoryBar()
	{
		Container = Add.Panel( "container" );
	}

	public override void Tick()
	{
		base.Tick();

		SetClass( "active", IsOpen );

		var player = GunfightCamera.Target;
		if ( player == null ) return;

		Weapons.Clear();
		Weapons.AddRange( player.Children.Select( x => x as GunfightWeapon ).Where( x => x.IsValid() ) );

		if ( Weapons.Count != Container.ChildrenCount )
		{
			Container.DeleteChildren( true );
		}
	}

	[Event.BuildInput]
	public void ProcessClientInput()
	{
		bool wantOpen = IsOpen;
		var localPlayer = Local.Pawn as Player;

		wantOpen = wantOpen || Input.MouseWheel != 0;
		wantOpen = wantOpen || Input.Pressed( InputButton.Menu );
		wantOpen = wantOpen || Input.Pressed( InputButton.Slot1 );
		wantOpen = wantOpen || Input.Pressed( InputButton.Slot2 );
		wantOpen = wantOpen || Input.Pressed( InputButton.Slot3 );
		wantOpen = wantOpen || Input.Pressed( InputButton.Slot4 ) || Input.Pressed( InputButton.Flashlight );
		wantOpen = wantOpen || Input.Pressed( InputButton.Slot5 ) || Input.Pressed( InputButton.Reload );
		wantOpen = wantOpen || Input.Pressed( InputButton.Slot6 ) || Input.Pressed( InputButton.Use );

		if ( Weapons.Count == 0 )
		{
			IsOpen = false;
			return;
		}

		if ( IsOpen != wantOpen )
		{
			SelectedWeapon = localPlayer?.ActiveChild as GunfightWeapon;
			IsOpen = true;
		}

		if ( !IsOpen ) return;

		var oldSelected = SelectedWeapon;
		var wantedIndex = SlotPressInput();

		var slotDirection = Input.MouseWheel;
		if ( Input.Pressed( InputButton.SlotPrev ) )
			slotDirection = 1;
		if ( Input.Pressed( InputButton.SlotNext ) )
			slotDirection = -1;

		// If the wanted index is -1, it means we're not pressing any slot keys
		if ( wantedIndex == -1 )
		{
			// Let's check other ways of input
			var sortedWeapons = Weapons.OrderBy( x => x.Slot ).ToList();

			// Support switching to our last used weapon
			if ( Input.Pressed( InputButton.Menu ) )
			{
				if ( LastWeapon.IsValid() && sortedWeapons.Contains( LastWeapon ) )
				{
					SelectedWeapon = LastWeapon;
					localPlayer.ActiveChildInput = SelectedWeapon;
				}
			}
			// Otherwise, check to see if we're using the scroll wheel to switch.
			else if ( slotDirection != 0 )
			{
				var currentIndex = sortedWeapons.IndexOf( SelectedWeapon );
				currentIndex -= slotDirection;
				// Wrap around the array of weapons if we go too far
				currentIndex = currentIndex.UnsignedMod( Weapons.Count );

				var wishedWeapon = sortedWeapons.ElementAtOrDefault( currentIndex );
				if ( wishedWeapon.IsValid() && wishedWeapon != SelectedWeapon )
				{
					SelectedWeapon = wishedWeapon;
					localPlayer.ActiveChildInput = SelectedWeapon;
				}
			}
		}
		else
		{
			// We want to change weapon with slot keys
			var chosenWeapon = Weapons.FirstOrDefault( x => (int)x.Slot == wantedIndex );
			if ( chosenWeapon != SelectedWeapon && chosenWeapon.IsValid() )
			{
				SelectedWeapon = chosenWeapon;
				localPlayer.ActiveChildInput = SelectedWeapon;
			}
		}

		if ( oldSelected != SelectedWeapon )
		{
			Sound.FromScreen( "weapon.swap" );
			LastWeapon = oldSelected;
		}
		
		Input.MouseWheel = 0;
	}

	private int SlotPressInput()
	{
		int index = -1;

		if ( Input.Pressed( InputButton.Slot1 ) ) index = 0;
		if ( Input.Pressed( InputButton.Slot2 ) ) index = 1;
		if ( Input.Pressed( InputButton.Slot3 ) ) index = 2;
		if ( Input.Pressed( InputButton.Slot4 ) ) index = 3;
		if ( Input.Pressed( InputButton.Slot5 ) ) index = 4;
		if ( Input.Pressed( InputButton.Slot6 ) ) index = 5;
		if ( Input.Pressed( InputButton.Use ) ) index = 5;
		if ( Input.Pressed( InputButton.Reload ) ) index = 4;
		if ( Input.Pressed( InputButton.Flashlight ) ) index = 3;

		return index;
	}
}

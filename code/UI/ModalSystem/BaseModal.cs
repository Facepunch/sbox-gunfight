using System;
using Sandbox.UI;

namespace Facepunch.Gunfight.UI;

public class BaseModal : Panel
{
	internal Action<bool> OnClosed;

	public BaseModal()
	{
		AddClass( "modal" );

		var bg = AddChild<Panel>( "modal-background" );
		bg.AddEventListener( "onmousedown", () => CloseModal( false ) );
	}

	protected void CloseModal( bool success )
	{
		OnClosed?.Invoke( success );
	}
}

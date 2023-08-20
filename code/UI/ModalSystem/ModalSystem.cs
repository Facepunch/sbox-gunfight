namespace Facepunch.Gunfight.UI;

public static class ModalSystem
{
	private static List<BaseModal> Modals = new();

	public static void Push( BaseModal modal )
	{
		Modals.Add( modal );
		ModalOverlay.Instance.AddChild( modal );

		modal.OnClosed += ( s ) => OnModalClosing( modal, s );
	}

	static void OnModalClosing( BaseModal modal, bool success )
	{
		modal.Delete();
		Modals.Remove( modal );
	}

	public static void DestroyAll()
	{
		foreach ( var modal in Modals )
		{
			modal.Delete();
		}

		Modals = new();
	}

	public static void Pop()
	{
		var modal = Modals.LastOrDefault();
		modal?.Delete();
	}
}

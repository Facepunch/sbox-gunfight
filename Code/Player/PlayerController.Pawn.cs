namespace Gunfight;

public partial class PlayerController
{
	public void OnPossess()
	{
		CreateViewModel();
		SetupCamera();
	}

	[Broadcast]
	public void NetPossess()
	{
		// Don't own? Go away
		if ( IsProxy )
			return;

		(this as IPawn ).Possess();
	}

	void SetupCamera()
	{
		CameraController.SetActive( true );
	}

	public void OnUnPossess()
	{
		CameraController.SetActive( false );
		ClearViewModel();
	}
}

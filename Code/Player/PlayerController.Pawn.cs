namespace Gunfight;

public partial class PlayerController
{
	public void OnPossess()
	{
		// When possessing a player, we want to make the viewmodel, if possible.
		CreateViewModel();
		SetupCamera();
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

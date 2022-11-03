namespace Facepunch.Gunfight.UI;

public partial class GunfightStatus
{
	[Event( "gunfight.scores.changed" )]
	public void Update()
	{
		StateHasChanged();
	}
}
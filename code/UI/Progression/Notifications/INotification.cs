namespace Facepunch.Gunfight.UI;

public interface INotification
{
	public float Lifetime { get; set; }
	public void Destroy();
}

namespace Facepunch.Gunfight.Models;

public class BaseModel
{
	public DateTimeOffset CreateTime { get; set; }
	public DateTimeOffset LastUpdateTime { get; set; }

	public BaseModel()
	{
		CreateTime = DateTimeOffset.Now;
		LastUpdateTime = DateTimeOffset.Now;
	}

	public virtual void MarkAsUpdated()
	{
		LastUpdateTime = DateTimeOffset.UtcNow;
	}
}

namespace Gunfight;

public partial class ComponentNotFoundException : Exception
{
	string CustomMessage { get; set; } = null;

	public override string Message => string.IsNullOrEmpty( CustomMessage ) ? "Couldn't find a component." : CustomMessage;

	public ComponentNotFoundException( string message = null )
	{
		CustomMessage = message;
	}
}

namespace Facepunch.Gunfight.Auth;

public partial class AuthSystem
{
	public static async Task<string> TokenFetchAsync()
	{
		return await Sandbox.Services.Auth.GetToken( "facepunch.gunfight" );
	}
}

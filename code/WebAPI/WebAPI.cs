using System.Net.Http;
using Facepunch.Gunfight.Auth;

namespace Facepunch.Gunfight;

public partial class WebAPI
{
	// TODO - Set this to something proper
	public static string BaseUrl => "http://localhost:8080/api/";

	private static async Task<Dictionary<string, string>> GetHeaders()
	{
		var token = await AuthSystem.TokenFetchAsync();

		var headers = new Dictionary<string, string>
		{
			{ "X-Auth-Token", token }, { "X-Auth-Id", Game.SteamId.ToString() }
		};

		return headers;
	}
	
	public static async Task<T> HttpGet<T>( string endpoint, HttpContent content = null )
	{
		var headers = await GetHeaders();
		return await Http.RequestJsonAsync<T>( $"{BaseUrl}{endpoint}", "GET", null, headers );
	}
	
	public static async Task<T> HttpPost<T>( string endpoint, HttpContent content = null )
	{
		var headers = await GetHeaders();
		return await Http.RequestJsonAsync<T>( $"{BaseUrl}{endpoint}", "POST", content, headers );
	}
		
	public static async Task<T> HttpPut<T>( string endpoint, HttpContent content = null )
	{
		var headers = await GetHeaders();
		return await Http.RequestJsonAsync<T>( $"{BaseUrl}{endpoint}", "PUT", content, headers );
	}
}

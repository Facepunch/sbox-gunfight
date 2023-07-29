using System.Collections.Immutable;

namespace Facepunch.Gunfight;

public partial class GunfightLobby
{
	#region I don't like any of this.
		public Task<bool> JoinAsync() => _lobby.JoinAsync();
		public Friend Owner => _lobby.Owner;
		public int MemberCount => _lobby.MemberCount;
		public IEnumerable<Friend> Members => _lobby.Members;
		public int MaxMembers => _lobby.MaxMembers;
		public string Map { get => _lobby.Map; set => _lobby.Map = value; }
		public Action<Friend, string> OnChatMessage { get => _lobby.OnChatMessage; set => _lobby.OnChatMessage = value; }
		public void SendChat( string message ) => _lobby.SendChat( message );
		public void Leave() => _lobby.Leave();
		public void SetData( string key, string value ) => _lobby.SetData( key, value );
		public Task LaunchGameAsync() => _lobby.LaunchGameAsync();
		public ImmutableDictionary<string, string> Data { get => _lobby.Data; }
	#endregion
}

using Sandbox.Diagnostics;
using System.Text.Json.Nodes;

namespace Gunfight;

public static class PrefabUtility
{
	// TODO: Kill this shit off
	public static GameObject CreateGameObject( PrefabFile prefabFile, GameObject parent = null )
	{
		Assert.NotNull( Game.ActiveScene, "No Active Scene" );

		var json = prefabFile.RootObject;

		var go = new GameObject();
		go.Deserialize( json );
		go.SetPrefabSource( prefabFile.ResourcePath );

		if ( parent != null )
		{
			go.Parent = parent;
		}

		return go;
	}
}

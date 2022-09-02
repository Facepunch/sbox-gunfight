namespace Facepunch.Gunfight;

public struct RecoilSetup
{
	public Vector2 Decay { get; set; }
	public Vector2 BaseRecoilMinimum { get; set; }
	public Vector2 BaseRecoilMaximum { get; set; }
	public float ViewModelScale { get; set; }
	public float ViewModelRecoverySpeed { get; set; }
}

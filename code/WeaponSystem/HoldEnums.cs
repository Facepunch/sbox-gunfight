namespace Facepunch.Gunfight;

public enum HoldType
{
	None = 0,
	Pistol = 1,
	Rifle = 2,
	Shotgun = 3,
	Item = 4,
	MeleePunch = 5,
	MeleeWeapons = 6
}

public static class HoldTypeExtensions
{
	public static int ToInt( this HoldType holdType )
	{
		return (int)holdType;
	}

	public static int ToInt( this HoldHandedness handedness )
	{
		return (int)handedness;
	}
}

public enum HoldHandedness
{
	TwoHands = 0,
	RightHand = 1,
	LeftHand = 2
}

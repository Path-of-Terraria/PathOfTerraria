namespace PathOfTerraria.Common;





public enum Influence
{
	None,
	Solar,
	Lunar
}

[Flags]
public enum PlayerClass
{
	None = 0,
	Melee = 1,
	Ranged = 2,
	Magic = 3,
	Summoner = 4
}
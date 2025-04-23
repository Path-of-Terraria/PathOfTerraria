namespace PathOfTerraria.Common.Mechanics;

internal interface ILevel
{
	/// <summary> Corresponds to the current, and max level respectively. </summary>
	public (int, int) LevelRange { get; set; }
}

namespace PathOfTerraria.Common.Enums;

public enum ItemRarity
{  
	/// <summary>
	/// Used to mark values that should not be used directly, such as optional parameters with custom handling.
	/// </summary>
	Invalid = -1,

	Normal = 0,
	Magic,
	Rare,
	Unique
}
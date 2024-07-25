using Terraria.DataStructures;

namespace PathOfTerraria.Content.Buffs;

public class ArmorShred : ModBuff
{
	public const float DefenseReductionPercent = 25f;
	
	public const float DefenseMultiplier = 1f - DefenseReductionPercent / 100f;
}
using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.Subworlds;
using PathOfTerraria.Core.Items;
using SubworldLibrary;

namespace PathOfTerraria.Common.Systems.MobSystem;

public static class PoTMobHelper
{
	public const float AreaLevelScalingCap = 80f;

	public static int GetAffixCount(ItemRarity rarity, int areaLevel)
	{
		(int min, int max) = rarity switch
		{
			ItemRarity.Magic when areaLevel <= 20 => (1, 1),
			ItemRarity.Magic => (1, 2),
			ItemRarity.Rare when areaLevel <= 20 => (2, 2),
			ItemRarity.Rare when areaLevel <= 40 => (2, 3),
			ItemRarity.Rare => (3, 4),
			_ => (0, 0)
		};

		if (max == 0)
		{
			return 0;
		}

		return Main.rand.Next(min, max + 1);
	}

	public static int GetAreaLevel()
	{
		if (SubworldSystem.Current is MappingWorld && MappingWorld.AreaLevel > 0)
		{
			return MappingWorld.AreaLevel;
		}

		return PoTItemHelper.PickItemLevel(clampHardmode: false);
	}

	public static float GetStatScaling()
	{
		float progress = MathHelper.Clamp((GetAreaLevel() - 1f) / AreaLevelScalingCap, 0f, 1f);
		return progress * progress * (3f - 2f * progress);
	}
}

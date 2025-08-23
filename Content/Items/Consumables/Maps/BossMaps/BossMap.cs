using PathOfTerraria.Common.Subworlds.BossDomains.Hardmode;
using PathOfTerraria.Core.Items;
using SubworldLibrary;
using Terraria.Localization;

namespace PathOfTerraria.Content.Items.Consumables.Maps.BossMaps;

internal abstract class BossMap(int tier, int level, Func<bool> defeatCondition, bool hardMode) : Map
{
	public override int MaxUses => GetBossUseCount();
	public override int WorldLevel
	{
		get
		{
			if (hardMode)
			{
				return WorldLevelBasedOnTier(tier) + 1;
			}

			return level;
		}
	}

	public override bool CanDrop
	{
		get
		{
			if (hardMode && PoTItemHelper.PickItemLevel() < WorldLevelBasedOnTier(tier))
			{
				return false;
			}
			return defeatCondition();
		}
	}
	public override int SetMapTier(int _)
	{
		if (!hardMode)
		{
			return 1;
		}
		// Hardmode boss maps can only drop with their own tier
		return tier;
	}
}

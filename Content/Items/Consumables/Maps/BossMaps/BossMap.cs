using PathOfTerraria.Common.Items;
using PathOfTerraria.Core.Items;

namespace PathOfTerraria.Content.Items.Consumables.Maps.BossMaps;

internal abstract class BossMap(int tier, int level, Func<bool> defeatCondition, bool hardMode) : Map, ITemporaryItem
{
	public override int MaxUses => GetBossUseCount();
	public override int WorldLevel => level;

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

	public override int GetMapTier(int _)
	{
		if (!hardMode)
		{
			return 1;
		}
		// Hardmode boss maps can only drop with their own tier
		return tier;
	}

	bool ITemporaryItem.DespawnCondition()
	{
		return CanDrop;
	}
}

internal abstract class PreHardmodeBossMap(int level, Func<bool> defeatCondition) : BossMap(tier: 1, level: level, defeatCondition, hardMode: false)
{
}

internal abstract class HardmodeBossMap(int tier, Func<bool> defeatCondition) : BossMap(tier: tier, level: WorldLevelBasedOnTier(tier) + 1, defeatCondition, hardMode: true)
{
}
using PathOfTerraria.Common.Subworlds.BossDomains.Hardmode;
using PathOfTerraria.Core.Items;
using SubworldLibrary;
using Terraria.Localization;

namespace PathOfTerraria.Content.Items.Consumables.Maps.BossMaps;

internal abstract class BossMap(int tier, int level, Func<bool> defeatCondition, bool hardMode) : Map
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
}

internal abstract class PreHardmodeBossMap : BossMap
{
	public PreHardmodeBossMap(int level, Func<bool> defeatCondition)
		: base(tier: 1, level: level, defeatCondition, hardMode: false) { }
}

internal abstract class HardmodeBossMap : BossMap
{
	public HardmodeBossMap(int tier, Func<bool> defeatCondition)
		: base(tier: tier, level: WorldLevelBasedOnTier(tier) + 1, defeatCondition, hardMode: true) { }
}
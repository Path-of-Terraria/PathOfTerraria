using PathOfTerraria.Content.Items.Consumables.Maps.BossMaps;
using Terraria.GameContent.ItemDropRules;

namespace PathOfTerraria.Common.NPCs;

internal class GlobalLoot : GlobalNPC
{
	public override void ModifyGlobalLoot(Terraria.ModLoader.GlobalLoot globalLoot)
	{
		globalLoot.Add(ItemDropRule.ByCondition(new BossDownedCondition(BossDownedCondition.Bosses.KingSlime), ModContent.ItemType<KingSlimeMap>(), 1000));
		globalLoot.Add(ItemDropRule.ByCondition(new BossDownedCondition(BossDownedCondition.Bosses.EyeOfCthulhu), ModContent.ItemType<EoCMap>(), 1000));
		globalLoot.Add(ItemDropRule.ByCondition(new BossDownedCondition(BossDownedCondition.Bosses.EaterofWorlds), ModContent.ItemType<EoCMap>(), 1000));
		globalLoot.Add(ItemDropRule.ByCondition(new BossDownedCondition(BossDownedCondition.Bosses.QueenBee), ModContent.ItemType<BeeMap>(), 1000));
		globalLoot.Add(ItemDropRule.ByCondition(new BossDownedCondition(BossDownedCondition.Bosses.Deerclops), ModContent.ItemType<DeerclopsMap>(), 1000));
		globalLoot.Add(ItemDropRule.ByCondition(new BossDownedCondition(BossDownedCondition.Bosses.Skeletron), ModContent.ItemType<SkeletronMap>(), 1000));
		globalLoot.Add(ItemDropRule.ByCondition(new BossDownedCondition(BossDownedCondition.Bosses.WallOfFlesh), ModContent.ItemType<WoFMap>(), 1000));
	}
}

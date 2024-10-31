using Terraria.GameContent.ItemDropRules;

namespace PathOfTerraria.Common.NPCs;

internal class GlobalLoot : GlobalNPC
{
	public override void ModifyGlobalLoot(Terraria.ModLoader.GlobalLoot globalLoot)
	{
		//globalLoot.Add(ItemDropRule.ByCondition(new Conditions.King));
	}
}

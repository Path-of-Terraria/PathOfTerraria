using Terraria.GameContent.ItemDropRules;

namespace PathOfTerraria.Core.Systems.VanillaModifications.BossItemRemovals;

internal class NPCLootDisabler : GlobalNPC
{
	public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
	{
		npcLoot.RemoveWhere(x => x is CommonDrop common && BossGlobalItemDisabler.BossSpawners.Contains(common.itemId));
	}
}

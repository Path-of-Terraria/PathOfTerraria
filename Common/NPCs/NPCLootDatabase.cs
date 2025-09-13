using System.Collections.Generic;
using System.Linq;
using Terraria.GameContent.ItemDropRules;

namespace PathOfTerraria.Common.NPCs;

/// <summary>
/// Stores all non-specific NPCLoot modifications for ease of use and organization.
/// </summary>
internal class NPCLootDatabase : GlobalNPC
{
	public readonly struct ConditionalLoot(Func<NPC, NPCLoot, bool> condition, IItemDropRule loot)
	{
		public readonly Func<NPC, NPCLoot, bool> Condition = condition;
		public readonly IItemDropRule Loot = loot;
	}

	private static readonly List<ConditionalLoot> LootToAdd = [];

	public static Func<NPC, NPCLoot, bool> MatchId(params int[] types)
	{
		return (npc, loot) => types.Contains(npc.type);
	}

	public static Func<NPC, NPCLoot, bool> MatchId(int type)
	{
		return (npc, loot) => type == npc.type;
	}

	/// <summary>
	/// Adds the given loot to the database. This should only be run in <see cref="ModType.SetStaticDefaults"/>.
	/// </summary>
	/// <param name="loot">Loot to add.</param>
	public static void AddLoot(ConditionalLoot loot)
	{
		LootToAdd.Add(loot);
	}

	public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
	{
		foreach (ConditionalLoot loot in LootToAdd)
		{
			if (loot.Condition(npc, npcLoot))
			{
				npcLoot.Add(loot.Loot);
			}
		}
	}
}

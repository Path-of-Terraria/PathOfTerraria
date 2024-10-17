using PathOfTerraria.Content.Items.Quest;
using System.Collections.Generic;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.Localization;

namespace PathOfTerraria.Common.NPCs;

internal class SyncedCustomDrops : GlobalNPC
{
	private static readonly Dictionary<int, int> PlayerCountByItemIds = [];
	
	public static void AddId(int id)
	{
		if (PlayerCountByItemIds.TryGetValue(id, out int count))
		{
			PlayerCountByItemIds[id] = ++count;
		}
		else
		{
			PlayerCountByItemIds.Add(id, 1);
		}
	}

	public static void RemoveId(int id)
	{
		if (PlayerCountByItemIds.TryGetValue(id, out int value))
		{
			PlayerCountByItemIds[id] = --value;

			if (value <= 0)
			{
				PlayerCountByItemIds.Remove(id);
			}
		}
	}

	public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
	{
		if (npc.type is NPCID.GoblinArcher or NPCID.GoblinPeon or NPCID.GoblinScout or NPCID.GoblinSorcerer or NPCID.GoblinThief or NPCID.GoblinWarrior)
		{
			AddCountCondition(npcLoot, LocalizedText.Empty, ModContent.ItemType<TomeOfTheElders>(), 10);
		}
	}

	private static void AddCountCondition(NPCLoot npcLoot, LocalizedText text, int id, int denominator)
	{
		npcLoot.Add(ItemDropRule.ByCondition(new PlayerCountCondition(text, id), id, denominator));
	}

	public class PlayerCountCondition(LocalizedText text, int id) : IItemDropRuleCondition
	{
		private readonly LocalizedText _text = text;
		private readonly int _id = id;

		public bool CanDrop(DropAttemptInfo info)
		{
			return PlayerCountByItemIds.TryGetValue(_id, out int count) && count > 0;
		}

		public bool CanShowItemDropInUI()
		{
			return true;
		}

		public string GetConditionDescription()
		{
			return _text.Value;
		}
	}
}

using Terraria.GameContent.Drawing;
using Terraria.Localization;

namespace PathOfTerraria.Common.Systems.Questing.QuestStepTypes;

/// <summary>
/// Checks if a player has the required <paramref name="stacks"/> in their inventory and is talking to the given <paramref name="npcId"/>.<br/>
/// Optionally, you can also choose to <paramref name="takeItems"/> from the player upon success (and show the visual transfer of said items)
/// and <paramref name="npcDialogue"/> can be used to make the NPC say something on success.
/// </summary>
/// <param name="stacks">The item ID and associated stack count required by this step.</param>
/// <param name="npcId">The NPC to talk to.</param>
/// <param name="takeItems">If true, the NPC will "take" all of the items from the player.</param>
/// <param name="npcDialogue">If not null, the NPC's dialogue will change to this on success.</param>
internal class BringCount((int id, int count)[] stacks, int npcId, bool takeItems = false, LocalizedText npcDialogue = null) : QuestStep
{
	private static LocalizedText CollectText = null;
	private static LocalizedText GiveText = null;

	private readonly (int id, int count)[] Stacks = stacks;
	private readonly int NpcId = npcId;
	private readonly bool TakeItems = takeItems;
	private readonly LocalizedText NpcDialogue = npcDialogue;

	public override string DisplayString()
	{
		CollectText ??= Language.GetText($"Mods.{PoTMod.ModName}.Quests.Bring.Collect");
		GiveText ??= Language.GetText($"Mods.{PoTMod.ModName}.Quests.Bring.GiveItTo");

		string result = CollectText.Value + "\n";

		for (int i = 0; i < Stacks.Length; ++i)
		{
			(int id, int count) = Stacks[i];
			result += $"    {count} {Lang.GetItemNameValue(id)}\n";
		}

		result += $"    {GiveText.Format(Lang.GetNPCNameValue(NpcId))}";
		return result;
	}

	public override bool Track(Player player)
	{
		if (player.TalkNPC is not null && player.TalkNPC.type == NpcId)
		{
			bool hasAllStacks = true;

			foreach ((int id, int count) in Stacks)
			{
				if (player.CountItem(id, count) < count)
				{
					hasAllStacks = false;
					break;
				}
			}

			if (hasAllStacks)
			{
				if (TakeItems)
				{
					TakeAllItems(player);
				}

				if (NpcDialogue is not null)
				{
					Main.npcChatText = NpcDialogue.Value;
				}

				return true;
			}
		}

		return false;
	}

	private void TakeAllItems(Player player)
	{
		foreach ((int id, int count) in Stacks)
		{
			int realCount = count;

			for (int i = 0; i < player.inventory.Length; ++i)
			{
				Item item = player.inventory[i];

				if (!item.IsAir && item.type == id)
				{
					int reduction = Math.Min(realCount, item.stack);
					item.stack -= reduction;
					realCount -= reduction;

					if (item.stack <= 0)
					{
						item.TurnToAir();
					}
				}

				if (realCount <= 0)
				{
					break;
				}
			}

			ParticleOrchestrator.BroadcastOrRequestParticleSpawn(ParticleOrchestraType.ItemTransfer, new ParticleOrchestraSettings
			{
				PositionInWorld = player.Center,
				MovementVector = player.TalkNPC.Center - player.Center,
				UniqueInfoPiece = id
			});
		}
	}
}

// this is basically collect + interact with npc...
// idk if its needed or if it should just be the two from above
// but as one

// well, we might want it to actually take the items
// and collect dosent do/enable that
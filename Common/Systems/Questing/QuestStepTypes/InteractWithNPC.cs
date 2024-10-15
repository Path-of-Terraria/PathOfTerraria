using Terraria.Localization;

namespace PathOfTerraria.Common.Systems.Questing.QuestStepTypes;

/// <summary>
/// A step that simply has you talk to an NPC or give them items. <paramref name="dialogue"/> can be set if you want the NPC to reply with something.
/// </summary>
/// <param name="npcId">NPC ID to talk to.</param>
/// <param name="dialogue">NPC's dialogue. If null, dialog will not be replaced.</param>
internal class InteractWithNPC(int npcId, LocalizedText dialogue = null, (int, int)[] reqItems = null, bool removeItems = false) : QuestStep
{
	private static LocalizedText TalkToText = null;

	private readonly int NpcId = npcId;
	private readonly LocalizedText NpcDialogue = dialogue;
	private readonly (int type, int stack)[] RequiredItems = reqItems;
	private readonly bool RemoveItems = removeItems;

	public override int LineCount => RequiredItems is not null ? 1 + RequiredItems.Length : 1;

	public override string DisplayString()
	{
		TalkToText ??= Language.GetText($"Mods.{PoTMod.ModName}.Quests.TalkTo");
		string baseText = TalkToText.Format(Lang.GetNPCNameValue(NpcId));

		if (RequiredItems is not null)
		{
			baseText += " and give them:";

			foreach ((int type, int stack) in RequiredItems)
			{
				baseText += $"\n    {stack}x {Lang.GetItemNameValue(type)}";
			}
		}
		
		return baseText;
	}

	public override bool Track(Player player)
	{
		bool talkingToNpc = player.TalkNPC is not null && player.TalkNPC.type == NpcId;
		bool hasAllItems = true;
		bool goodToGo = false;

		if (talkingToNpc)
		{
			if (RequiredItems is not null)
			{
				foreach ((int item, int stack) in RequiredItems)
				{
					if (player.CountItem(item) < stack)
					{
						hasAllItems = false;
					}
				}

				if (hasAllItems)
				{
					goodToGo = true;

					if (RemoveItems)
					{
						foreach ((int item, int stack) in RequiredItems)
						{
							for (int i = 0; i < stack; ++i)
							{
								player.ConsumeItem(item);
							}
						}
					}
				}
			}
			else
			{
				goodToGo = true;
			}
		}

		if (talkingToNpc && goodToGo && NpcDialogue is not null)
		{ 
			Main.npcChatText = NpcDialogue.Value;
		}

		return talkingToNpc && goodToGo;
	}
}

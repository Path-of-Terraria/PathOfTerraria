using Terraria.Localization;

namespace PathOfTerraria.Common.Systems.Questing.QuestStepTypes;

public readonly struct GiveItem(int stack, params int[] ids)
{
	public string Names
	{
		get
		{
			string names = "";
			string or = Language.GetTextValue($"Mods.{PoTMod.ModName}.Quests.Or");

			for (int i = 0; i < Ids.Length; i++)
			{
				int id = Ids[i];
				names += Lang.GetItemNameValue(id) + (i == Ids.Length - 1 ? "" : (i == Ids.Length - 2 ? or : ", "));
			}

			return names;
		}
	}

	public readonly int[] Ids = ids;
	public readonly int Stack = stack;
}

/// <summary>
/// A step that has you talk to an NPC and optionally show or give them items.<br/>
/// <paramref name="dialogue"/> can be set if you want the NPC to reply with something.<br/>
/// <paramref name="reqItems"/> can be set if you want the NPC to require the player to have items; and <paramref name="removeItems"/> will take those items if true.
/// </summary>
/// <param name="npcId">NPC ID to talk to.</param>
/// <param name="dialogue">NPC's dialogue. If null, dialog will not be replaced.</param>
/// <param name="reqItems">If not null, the items required to be held by the player when talking to the NPC.</param>
/// <param name="removeItems">If true, and <paramref name="reqItems"/> is not null, all <paramref name="reqItems"/> will be taken up to the required stack.</param>
/// <param name="onSuccessfulInteraction">An action that is run when the interaction is successful (the step is completed).</param>
internal class InteractWithNPC(int npcId, LocalizedText dialogue = null, GiveItem[] reqItems = null, 
	bool removeItems = false, Action<NPC> onSuccess = null) : QuestStep
{
	private static LocalizedText TalkToText = null;

	private readonly int NpcId = npcId;
	private readonly LocalizedText NpcDialogue = dialogue;
	private readonly GiveItem[] RequiredItems = reqItems;
	private readonly bool RemoveItems = removeItems;
	private readonly Action<NPC> OnSuccess = onSuccess;

	public override int LineCount => RequiredItems is not null ? 1 + RequiredItems.Length : 1;

	public override string DisplayString()
	{
		TalkToText ??= Language.GetText($"Mods.{PoTMod.ModName}.Quests.TalkTo");
		string baseText = TalkToText.Format(Lang.GetNPCNameValue(NpcId));

		if (RequiredItems is not null)
		{
			baseText += Language.GetText($"Mods.{PoTMod.ModName}.Quests.GiveThem");
			int id = 0;

			foreach (GiveItem item in RequiredItems)
			{
				baseText += $"\n  {++id}. {item.Stack}x {item.Names}";
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
				foreach (GiveItem item in RequiredItems)
				{
					int count = 0;

					for (int i = 0; i < item.Ids.Length; ++i)
					{
						count += player.CountItem(item.Ids[i]);
					}

					if (count < item.Stack)
					{
						hasAllItems = false;
						break;
					}
				}

				if (hasAllItems)
				{
					goodToGo = true;

					if (RemoveItems)
					{
						foreach (GiveItem item in RequiredItems)
						{
							int totalCount = item.Stack;

							for (int i = 0; i < item.Ids.Length; ++i)
							{
								int count = Math.Min(totalCount, player.CountItem(item.Ids[i]));
								totalCount -= count;
							
								for (int j = 0; j < count; ++j)
								{
									player.ConsumeItem(item.Ids[i]);
								}
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

		bool finished = talkingToNpc && goodToGo;

		if (finished)
		{
			OnSuccess?.Invoke(player.TalkNPC);
		}

		return finished;
	}
}

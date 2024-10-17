using Terraria.Localization;

namespace PathOfTerraria.Common.Systems.Questing.QuestStepTypes;

/// <summary>
/// A step that simply has you talk to an NPC. <paramref name="npcDialogue"/> can be set if you want the NPC to reply with something.
/// </summary>
/// <param name="npcId">NPC ID to talk to.</param>
/// <param name="npcDialogue">NPC's dialogue. If null, dialog will not be replaced.</param>
internal class InteractWithNPC(int npcId, LocalizedText npcDialogue = null) : QuestStep
{
	private static LocalizedText TalkToText = null;

	private readonly int NpcId = npcId;
	private readonly LocalizedText NpcDialogue = npcDialogue;

	public override string DisplayString()
	{
		TalkToText ??= Language.GetText($"Mods.{PoTMod.ModName}.Quests.TalkTo");

		return TalkToText.Format(Lang.GetNPCNameValue(NpcId));
	}

	public override bool Track(Player player)
	{
		bool talkingToNpc = player.TalkNPC is not null && player.TalkNPC.type == NpcId;

		if (talkingToNpc && NpcDialogue is not null)
		{
			Main.npcChatText = NpcDialogue.Value;
		}

		return talkingToNpc;
	}
}

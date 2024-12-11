using Mono.Cecil.Cil;
using MonoMod.Cil;
using PathOfTerraria.Common.Systems.Questing;
using PathOfTerraria.Common.Systems.Questing.Quests.MainPath;
using System.Collections.Generic;
using System.Reflection;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;

namespace PathOfTerraria.Common.NPCs.QuestMarkers.Vanilla;

/// <summary>
/// Used to give the Old Man a quest marker.
/// </summary>
internal class OldManMarkerNPC : IQuestMarkerNPC
{
	public bool HasQuestMarker(out Quest quest)
	{
		quest = Quest.GetLocalPlayerInstance<SkeletronQuest>();
		return NPC.downedBoss1 && quest.CanBeStarted;
	}
}

internal class OldManModifiers : GlobalNPC
{
	public override void Load()
	{
		int slot = Mod.AddNPCHeadTexture(NPCID.OldMan, $"{PoTMod.ModName}/Assets/NPCs/Town/OldMan_Head");

		// Properly add a NPC head to the Old Man
		FieldInfo internalDictInfo = typeof(TownNPCProfiles).GetField("_townNPCProfiles", BindingFlags.Instance | BindingFlags.NonPublic);
		var dict = internalDictInfo.GetValue(TownNPCProfiles.Instance) as Dictionary<int, ITownNPCProfile>;
		dict[NPCID.OldMan] = TownNPCProfiles.LegacyWithSimpleShimmer("OldMan", slot, -1, uniquePartyTexture: false, uniquePartyTextureShimmered: false);

		IL_Main.GUIChatDrawInner += AddOldManButtonHook;
	}

	private void AddOldManButtonHook(ILContext il)
	{
		var c = new ILCursor(il);

		if (!c.TryGotoNext(MoveType.After, x => x.MatchCall(typeof(NPCLoader).FullName, nameof(NPCLoader.SetChatButtons))))
		{
			return;
		}

		c.Emit(OpCodes.Ldloca_S, (byte)11);
		c.Emit(OpCodes.Ldloca_S, (byte)12);
		c.EmitDelegate(ModifyChatButtons);
	}

	public static void ModifyChatButtons(ref string buttonOne, ref string buttonTwo)
	{
		NPC talkNPC = Main.LocalPlayer.TalkNPC;

		if (talkNPC is not null && talkNPC.type == NPCID.OldMan)
		{
			buttonOne = "";
			buttonTwo = !Quest.GetLocalPlayerInstance<SkeletronQuest>().CanBeStarted ? "" : Language.GetTextValue("Mods.PathOfTerraria.NPCs.Quest");
		}
	}

	public override void GetChat(NPC npc, ref string chat)
	{
		if (npc.type != NPCID.OldMan)
		{
			return;
		}

		// Override vanilla dialogue
		if (Main.dayTime)
		{
			chat = Language.GetTextValue("Mods.PathOfTerraria.NPCs.OldMan.Dialogue.Day." + Main.rand.Next(3));
			return;
		}

		chat = Language.GetTextValue($"Mods.PathOfTerraria.NPCs.OldMan.Dialogue.{(!NPC.downedBoss1 ? "Weak" : "Strong")}.{Main.rand.Next(3)}");
	}

	public override void OnChatButtonClicked(NPC npc, bool firstButton)
	{
		if (!firstButton && npc.type == NPCID.OldMan)
		{
			Main.npcChatText = Language.GetTextValue("Mods.PathOfTerraria.NPCs.OldMan.Dialogue.QuestStart");
			Main.LocalPlayer.GetModPlayer<QuestModPlayer>().StartQuest($"{PoTMod.ModName}/{nameof(SkeletronQuest)}");
		}
	}
}

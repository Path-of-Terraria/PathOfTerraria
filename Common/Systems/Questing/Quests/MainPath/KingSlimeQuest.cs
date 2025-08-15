using System.Collections.Generic;
using System.Linq;
using PathOfTerraria.Common.Subworlds.BossDomains.Prehardmode;
using PathOfTerraria.Common.Systems.BossTrackingSystems;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.Questing.QuestStepTypes;
using PathOfTerraria.Common.Systems.Questing.RewardTypes;
using PathOfTerraria.Content.Items.Consumables.Maps.BossMaps;
using PathOfTerraria.Content.NPCs.Town;
using SubworldLibrary;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;

namespace PathOfTerraria.Common.Systems.Questing.Quests.MainPath;

internal class KingSlimeQuest : Quest
{
	public override QuestTypes QuestType => QuestTypes.MainStoryQuestAct1;
	public override int NPCQuestGiver => ModContent.NPCType<GarrickNPC>();

	public override List<QuestReward> QuestRewards =>
	[
		new ActionRewards((p, v) => p.GetModPlayer<ExpModPlayer>().Exp += 500,
			"500 experience (POC giving experience)\nSome gear with an affix\nA unique item\nAgain, just for POC reasons"),
	];

	public override List<QuestStep> SetSteps()
	{
		return 
		[
			new InteractWithNPC(ModContent.NPCType<GarrickNPC>(), 
				Language.GetText("Mods.PathOfTerraria.NPCs.GarrickNPC.Dialogue.QuestResponse"),
				Language.GetText("Mods.PathOfTerraria.NPCs.GarrickNPC.Dialogue.QuestResponse"),
				[
					new GiveItem(5, ItemID.IronBar, ItemID.LeadBar),
					new GiveItem(5, ItemID.GoldBar, ItemID.PlatinumBar),
					new GiveItem(1, ItemID.Ruby),
				], true),
			//Give the map device to the player once req mats are given
			new ActionStep((_, _) => 
			{
				int npc = NPC.FindFirstNPC(ModContent.NPCType<GarrickNPC>());
				int item = Item.NewItem(new EntitySource_Gift(Main.npc[npc]), Main.npc[npc].Center, ModContent.ItemType<Content.Items.Placeable.MapDevice>());

				if (Main.netMode == NetmodeID.MultiplayerClient)
				{
					NetMessage.SendData(MessageID.SyncItem, -1, -1, null, item);
				}

				return true;
			}),
			//Give the map as well
			new ActionStep((_, _) => 
			{
				int npc = NPC.FindFirstNPC(ModContent.NPCType<GarrickNPC>());
				int item = Item.NewItem(new EntitySource_Gift(Main.npc[npc]), Main.npc[npc].Center, ModContent.ItemType<KingSlimeMap>());

				if (Main.netMode == NetmodeID.MultiplayerClient)
				{
					NetMessage.SendData(MessageID.SyncItem, -1, -1, null, item);
				}

				return true;
			}),

			new ConditionCheck(_ => SubworldSystem.Current is KingSlimeDomain, 1, this.GetLocalization("EnterDomain")),
			new ConditionCheck(_ => BossTracker.DownedInDomain<KingSlimeDomain>(NPCID.KingSlime), 1, this.GetLocalization("Kill.KingSlime")),
			new InteractWithNPC(ModContent.NPCType<GarrickNPC>(), LocalizedText.Empty, this.GetLocalization("ThanksDialogue")) { CountsAsCompletedOnMarker = true }
		];
	}

	public override bool Available()
	{
		Quest[] checks = 
		[
			GetLocalPlayerInstance<BlacksmithStartQuest>(),
			GetLocalPlayerInstance<WizardStartQuest>(),
			GetLocalPlayerInstance<WitchStartQuest>(),
			GetLocalPlayerInstance<HunterStartQuest>()
		];

		return checks.Any(x => x.Completed);
	}

	public override string MarkerLocation()
	{
		return "Ravencrest";
	}
}
using NPCUtils;
using PathOfTerraria.Common.NPCs;
using PathOfTerraria.Common.NPCs.Components;
using PathOfTerraria.Common.NPCs.Dialogue;
using PathOfTerraria.Common.NPCs.Effects;
using PathOfTerraria.Common.NPCs.QuestMarkers;
using PathOfTerraria.Common.Quests;
using PathOfTerraria.Common.Subworlds.RavencrestContent;
using PathOfTerraria.Common.Systems.Questing;
using PathOfTerraria.Common.Systems.Questing.Quests.MainPath;
using PathOfTerraria.Common.Systems.Questing.Quests.MainPath.HardmodeQuesting;
using PathOfTerraria.Common.Utilities.Extensions;
using PathOfTerraria.Content.Items.Consumables.Maps.BossMaps;
using PathOfTerraria.Content.Items.Gear.Weapons.Grimoire;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.Localization;

namespace PathOfTerraria.Content.NPCs.Town;

[AutoloadHead]
[LegacyName("WitchNPC")]
public class MorganaNPC : ModNPC, IQuestMarkerNPC, ISpawnInRavencrestNPC
{
	Point16 ISpawnInRavencrestNPC.TileSpawn => (RavencrestSystem.Structures["Burrow"].Position + new Point(18, 20)).ToPoint16();

	public override void SetStaticDefaults()
	{
		Main.npcFrameCount[Type] = 23;
		NPCID.Sets.NoTownNPCHappiness[Type] = true;
		NPCID.Sets.DangerDetectRange[Type] = 400;
		NPCID.Sets.AttackFrameCount[Type] = 2;
		NPCID.Sets.AttackType[Type] = 2;
		NPCID.Sets.AttackTime[Type] = 60;
		NPCID.Sets.AttackAverageChance[Type] = 10;

		NPCID.Sets.NPCBestiaryDrawModifiers modifiers = new() { Velocity = 1f };
		NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, modifiers);
	}

	public override void SetDefaults()
	{
		NPC.townNPC = true;
		NPC.friendly = true;
		NPC.lifeMax = 250;
		NPC.width = 30;
		NPC.height = 50;
		NPC.knockBackResist = 0.4f;
		NPC.HitSound = SoundID.NPCHit1;
		NPC.DeathSound = SoundID.NPCDeath1;
		NPC.aiStyle = NPCAIStyleID.Passive;

		AnimationType = NPCID.Dryad;

		NPC.TryEnableComponent<NPCHitEffects>(
			c =>
			{
				c.AddGore(new NPCHitEffects.GoreSpawnParameters($"{PoTMod.ModName}/{Name}_0", 1, NPCHitEffects.OnDeath));
				c.AddGore(new NPCHitEffects.GoreSpawnParameters($"{PoTMod.ModName}/{Name}_1", 1, NPCHitEffects.OnDeath));
				c.AddGore(new NPCHitEffects.GoreSpawnParameters($"{PoTMod.ModName}/{Name}_2", 2, NPCHitEffects.OnDeath));
				c.AddGore(new NPCHitEffects.GoreSpawnParameters($"{PoTMod.ModName}/{Name}_3", 1, NPCHitEffects.OnDeath));
				
				c.AddDust(new NPCHitEffects.DustSpawnParameters(DustID.Blood, 15));
			}
		);

		NPC.TryEnableComponent<NPCTownDialogue>(
			c =>
			{
				c.AddDialogue(new NPCTownDialogue.DialogueEntry($"Mods.{PoTMod.ModName}.NPCs.{Name}.Dialogue.Common0"));
				c.AddDialogue(new NPCTownDialogue.DialogueEntry($"Mods.{PoTMod.ModName}.NPCs.{Name}.Dialogue.Common1"));
				c.AddDialogue(new NPCTownDialogue.DialogueEntry($"Mods.{PoTMod.ModName}.NPCs.{Name}.Dialogue.Common2"));
				c.AddDialogue(new NPCTownDialogue.DialogueEntry($"Mods.{PoTMod.ModName}.NPCs.{Name}.Dialogue.Common3"));
			}
		);
	}

	public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
	{
		bestiaryEntry.AddInfo(this, "Surface");
	}

	public override void SetChatButtons(ref string button, ref string button2)
	{
		Quest startQuest = Quest.GetLocalPlayerInstance<WitchStartQuest>();
		if (startQuest.Active || startQuest.Completed) //Don't display the shop until the player has received the first quest
		{
			button = Language.GetTextValue("LegacyInterface.28"); //Shop
		}

		bool hasAvailableQuest = QuestUnlockManager.CanStartQuest<WitchStartQuest>() ||
		                         QuestUnlockManager.CanStartQuest<PlanteraQuest>() ||
		                         QuestUnlockManager.CanStartQuest<QueenBeeQuest>();

		button2 = hasAvailableQuest ? Language.GetTextValue("Mods.PathOfTerraria.NPCs.Quest") : "";

		if (QuestUtils.ShouldRegrantQuestItem<PlanteraQuest>(Main.LocalPlayer, ModContent.ItemType<PlanteraMap>(), 2, 3))
		{
			button2 = this.GetLocalization("AnotherMap").Value;
			Main.npcChatCornerItem = ModContent.ItemType<PlanteraMap>();
		}
	}

	//TODO: This should probably be a base NPC functionality as many NPC's will have multiple quests. 
	private static Quest DetermineNewestQuest()
	{
		if (QuestUnlockManager.CanStartQuest<WitchStartQuest>())
		{
			return Quest.GetLocalPlayerInstance<WitchStartQuest>();
		}
		if (QuestUnlockManager.CanStartQuest<QueenBeeQuest>())
		{
			return Quest.GetLocalPlayerInstance<QueenBeeQuest>();
		}
		if (QuestUnlockManager.CanStartQuest<PlanteraQuest>())
		{
			return Quest.GetLocalPlayerInstance<PlanteraQuest>();
		}
		return Quest.GetLocalPlayerInstance<WitchStartQuest>(); //Shouldn't be possible, but just in case
	}

	public override void OnChatButtonClicked(bool firstButton, ref string shopName)
	{
		if (firstButton)
		{
			shopName = "Shop";
			return;
		}

		if (QuestUtils.ShouldRegrantQuestItem<PlanteraQuest>(Main.LocalPlayer, ModContent.ItemType<PlanteraMap>(), 2, 3))
		{
			QuestUtils.GiftQuestItem(NPC, ModContent.ItemType<PlanteraMap>());
			Main.npcChatText = this.GetLocalization("Dialogue.GetPlanteraMapAgain").Value;
			return;
		}

		Quest quest = DetermineNewestQuest();

		switch (quest)
		{
			case WitchStartQuest:
				Main.LocalPlayer.QuickSpawnItem(new EntitySource_Gift(NPC), ModContent.ItemType<GrimoireItem>());
				Main.npcChatText = Language.GetTextValue("Mods.PathOfTerraria.NPCs.MorganaNPC.Dialogue.Quest");
				Main.LocalPlayer.GetModPlayer<QuestModPlayer>().StartQuest<WitchStartQuest>();
				break;
			case QueenBeeQuest:
				Main.npcChatText = Language.GetTextValue("Mods.PathOfTerraria.NPCs.MorganaNPC.Dialogue.QueenBeeQuest");
				Main.LocalPlayer.GetModPlayer<QuestModPlayer>().StartQuest<QueenBeeQuest>();
				break;
			case PlanteraQuest:
				Main.npcChatText = Language.GetTextValue("Mods.PathOfTerraria.NPCs.MorganaNPC.Dialogue.Plantera.0");
				Main.LocalPlayer.GetModPlayer<QuestModPlayer>().StartQuest<PlanteraQuest>();
				break;
		}
	}

	public override void AddShops()
	{
		new NPCShop(Type)
			.Add<GrimoireItem>()
			.Register();
	}

	public override ITownNPCProfile TownNPCProfile()
	{
		return this.GetDefaultProfile();
	}

	public override void TownNPCAttackStrength(ref int damage, ref float knockback)
	{
		damage = 15;
		knockback = 1f;
	}

	public override void TownNPCAttackCooldown(ref int cooldown, ref int randExtraCooldown)
	{
		cooldown = 30;
		randExtraCooldown = 1;
	}

	public override void TownNPCAttackProj(ref int projType, ref int attackDelay)
	{
		projType = ProjectileID.WoodenArrowFriendly;
		attackDelay = 1;
	}

	public override void TownNPCAttackProjSpeed(ref float multiplier, ref float gravityCorrection, ref float randomOffset)
	{
		multiplier = 10f;
		randomOffset = 0.5f;
	}

	public bool HasQuestMarker(out Quest quest)
	{
		quest = DetermineNewestQuest();
		return !quest.Completed;
	}
}
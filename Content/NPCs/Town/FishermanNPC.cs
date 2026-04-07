using NPCUtils;
using PathOfTerraria.Common.NPCs;
using PathOfTerraria.Common.NPCs.Components;
using PathOfTerraria.Common.NPCs.Dialogue;
using PathOfTerraria.Common.NPCs.OverheadDialogue;
using PathOfTerraria.Common.NPCs.QuestMarkers;
using PathOfTerraria.Common.Subworlds.RavencrestContent;
using PathOfTerraria.Common.Systems.Questing;
using PathOfTerraria.Common.Systems.Questing.Quests.MainPath.HardmodeQuesting;
using PathOfTerraria.Common.Utilities.Extensions;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.Localization;

namespace PathOfTerraria.Content.NPCs.Town;

[AutoloadHead]
public class FishermanNPC : ModNPC, IQuestMarkerNPC, IOverheadDialogueNPC, ISpawnInRavencrestNPC
{
	Point16 ISpawnInRavencrestNPC.TileSpawn => RavencrestSystem.StaticStructureLocations["Port"];

	OverheadDialogueInstance IOverheadDialogueNPC.CurrentDialogue { get; set; }

	public override void SetStaticDefaults()
	{
		Main.npcFrameCount[Type] = 25;

		NPCID.Sets.ExtraFramesCount[Type] = 9;
		NPCID.Sets.AttackFrameCount[Type] = 4;
		NPCID.Sets.DangerDetectRange[Type] = 500;
		NPCID.Sets.AttackType[Type] = 0;
		NPCID.Sets.AttackTime[Type] = 16;
		NPCID.Sets.AttackAverageChance[Type] = 30;
		NPCID.Sets.NoTownNPCHappiness[Type] = true;

		var drawModifiers = new NPCID.Sets.NPCBestiaryDrawModifiers() { Velocity = 1f };
		NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);
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

		AnimationType = NPCID.Guide;

		/*
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
		*/

		NPC.TryEnableComponent<NPCTownDialogue>(c =>
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
		button = Language.GetTextValue("LegacyInterface.28"); // Shop

		bool hasAvailableQuest = QuestUnlockManager.CanStartQuest<FishronQuest>();
		//                         QuestUnlockManager.CanStartQuest<TwinsQuest>() || 

		button2 = hasAvailableQuest ? Language.GetTextValue("Mods.PathOfTerraria.NPCs.Quest") : "";
	}

	public override void OnChatButtonClicked(bool firstButton, ref string shopName)
	{
		if (firstButton)
		{
			shopName = "Shop";
			return;
		}

		if (QuestUnlockManager.CanStartQuest<FishronQuest>())
		{
			Main.npcChatText =
				Language.GetTextValue("Mods.PathOfTerraria.NPCs.FishermanNPC.Dialogue.FishermanFishronDialogue1");
			Main.LocalPlayer.GetModPlayer<QuestModPlayer>().StartQuest<FishronQuest>();
		}
	}


	public override void AddShops()
	{
		var shop = new NPCShop(Type);

		shop.Add(ItemID.Worm)
			.Add(ItemID.JourneymanBait)
			.Add(ItemID.MasterBait)
			.Add(ItemID.FishingPotion)
			.Add(ItemID.CratePotion)
			.Add(ItemID.SonarPotion);
		
		shop.Register();
	}

	public override ITownNPCProfile TownNPCProfile()
	{
		return this.GetDefaultProfile();
	}

	public override void TownNPCAttackStrength(ref int damage, ref float knockback)
	{
		damage = 20;
		knockback = 2f;
	}

	public override void TownNPCAttackCooldown(ref int cooldown, ref int randExtraCooldown)
	{
		cooldown = 25;
		randExtraCooldown = 5;
	}

	public override void TownNPCAttackProj(ref int projType, ref int attackDelay)
	{
		projType = ProjectileID.CombatWrench;
		attackDelay = 1;
	}

	public override void TownNPCAttackProjSpeed(ref float multiplier, ref float gravityCorrection,
		ref float randomOffset)
	{
		multiplier = 8f;
		gravityCorrection = 0.1f;
		randomOffset = 0.3f;
	}

	public bool CanSpawn(NPCSpawnTimeframe timeframe, bool alreadyExists)
	{
		return Main.hardMode && (timeframe is NPCSpawnTimeframe.WorldGen or NPCSpawnTimeframe.NaturalSpawn && !alreadyExists);
	}

	public bool HasQuestMarker(out Quest quest)
	{
		Quest fishronQuest = Quest.GetLocalPlayerInstance<FishronQuest>();

		if (fishronQuest.QuestNotStarted || (fishronQuest.Active && !fishronQuest.Completed))
		{
			quest = fishronQuest;
			return true;
		}

		quest = null;
		return false;
	}
}
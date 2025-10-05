using NPCUtils;
using PathOfTerraria.Common.NPCs;
using PathOfTerraria.Common.NPCs.Components;
using PathOfTerraria.Common.NPCs.Dialogue;
using PathOfTerraria.Common.NPCs.Effects;
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
public class TinkerNPC : ModNPC, IQuestMarkerNPC, IOverheadDialogueNPC, ISpawnInRavencrestNPC
{
	Point16 ISpawnInRavencrestNPC.TileSpawn =>
		(RavencrestSystem.Structures["Burrow"].Position + new Point(20, 20)).ToPoint16();

	OverheadDialogueInstance IOverheadDialogueNPC.CurrentDialogue { get; set; }

	public override void SetStaticDefaults()
	{
		Main.npcFrameCount[Type] = 23;

		NPCID.Sets.ExtraFramesCount[Type] = 9;
		NPCID.Sets.AttackFrameCount[Type] = 2;
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

		AnimationType = NPCID.Mechanic;

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

		bool hasAvailableQuest = QuestUnlockManager.CanStartQuest<TinkerIntroQuest>() || 
		                         QuestUnlockManager.CanStartQuest<TwinsQuest>() || 
		                         QuestUnlockManager.CanStartQuest<DestroyerQuest>() || 
		                         QuestUnlockManager.CanStartQuest<SkelePrimeQuest>();

		button2 = hasAvailableQuest ? Language.GetTextValue("Mods.PathOfTerraria.NPCs.Quest") : "";
	}

	public override void OnChatButtonClicked(bool firstButton, ref string shopName)
	{
		if (firstButton)
		{
			shopName = "Shop";
			return;
		}

		if (QuestUnlockManager.CanStartQuest<TinkerIntroQuest>())
		{
			Main.npcChatText =
				Language.GetTextValue("Mods.PathOfTerraria.NPCs.TinkerNPC.Dialogue.TinkerIntroDialogue1");
			Main.LocalPlayer.GetModPlayer<QuestModPlayer>().StartQuest<TinkerIntroQuest>();
		}
		else if (QuestUnlockManager.CanStartQuest<TwinsQuest>())
		{
			Main.npcChatText =
				Language.GetTextValue("Mods.PathOfTerraria.NPCs.TinkerNPC.Dialogue.TinkerTwinsDialogue1");
			Main.LocalPlayer.GetModPlayer<QuestModPlayer>().StartQuest<TwinsQuest>();
		}
		else if (QuestUnlockManager.CanStartQuest<DestroyerQuest>())
		{
			Main.npcChatText =
				Language.GetTextValue("Mods.PathOfTerraria.NPCs.TinkerNPC.Dialogue.TinkerDestroyerDialogue1");
			Main.LocalPlayer.GetModPlayer<QuestModPlayer>().StartQuest<DestroyerQuest>();
		}
		else if (QuestUnlockManager.CanStartQuest<SkelePrimeQuest>())
		{
			Main.npcChatText =
				Language.GetTextValue("Mods.PathOfTerraria.NPCs.TinkerNPC.Dialogue.TinkerSkeletronPrimeDialogue1");
			Main.LocalPlayer.GetModPlayer<QuestModPlayer>().StartQuest<SkelePrimeQuest>();
		}

	}


	public override void AddShops()
	{
		new NPCShop(Type)
			.Register();
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
		Quest tinkerIntroQuest = Quest.GetLocalPlayerInstance<TinkerIntroQuest>();
		Quest twinsQuest = Quest.GetLocalPlayerInstance<TwinsQuest>();
		Quest destroyerQuest = Quest.GetLocalPlayerInstance<DestroyerQuest>();
		Quest skelePrimeQuest = Quest.GetLocalPlayerInstance<SkelePrimeQuest>();

		if (tinkerIntroQuest.CanBeStarted || (tinkerIntroQuest.Active && !tinkerIntroQuest.Completed))
		{
			quest = tinkerIntroQuest;
			return true;
		}
		if (twinsQuest.CanBeStarted || (twinsQuest.Active && !twinsQuest.Completed))
		{
			quest = twinsQuest;
			return true;
		}
		if (destroyerQuest.CanBeStarted || (destroyerQuest.Active && !destroyerQuest.Completed))
		{
			quest = destroyerQuest;
			return true;
		}
		if (skelePrimeQuest.CanBeStarted || (skelePrimeQuest.Active && !skelePrimeQuest.Completed))
		{
			quest = skelePrimeQuest;
			return true;
		}
	
		quest = null;
		return false;
	}
}
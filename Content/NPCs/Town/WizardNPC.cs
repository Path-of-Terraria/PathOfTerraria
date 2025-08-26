using NPCUtils;
using PathOfTerraria.Common.NPCs;
using PathOfTerraria.Common.NPCs.Components;
using PathOfTerraria.Common.NPCs.Dialogue;
using PathOfTerraria.Common.NPCs.Effects;
using PathOfTerraria.Common.NPCs.OverheadDialogue;
using PathOfTerraria.Common.NPCs.QuestMarkers;
using PathOfTerraria.Common.Subworlds.RavencrestContent;
using PathOfTerraria.Common.Systems.Questing;
using PathOfTerraria.Common.Systems.Questing.Quests.MainPath;
using PathOfTerraria.Common.Systems.Questing.Quests.MainPath.HardmodeQuesting;
using PathOfTerraria.Common.Utilities.Extensions;
using PathOfTerraria.Content.Items.Currency;
using PathOfTerraria.Content.Items.Quest;
using PathOfTerraria.Content.Tiles.BossDomain;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.Localization;

namespace PathOfTerraria.Content.NPCs.Town;

[AutoloadHead]
public class WizardNPC : ModNPC, IQuestMarkerNPC, ISpawnInRavencrestNPC, IOverheadDialogueNPC
{
	Point16 ISpawnInRavencrestNPC.TileSpawn => (RavencrestSystem.Structures["Library"].Position + new Point(50, 40)).ToPoint16();
	OverheadDialogueInstance IOverheadDialogueNPC.CurrentDialogue { get; set; }

	public override void SetStaticDefaults()
	{
		Main.npcFrameCount[NPC.type] = 23;

		NPCID.Sets.ExtraFramesCount[NPC.type] = 9;
		NPCID.Sets.AttackFrameCount[NPC.type] = 2;
		NPCID.Sets.DangerDetectRange[NPC.type] = 500;
		NPCID.Sets.AttackType[NPC.type] = 0;
		NPCID.Sets.AttackTime[NPC.type] = 16;
		NPCID.Sets.AttackAverageChance[NPC.type] = 30;
		NPCID.Sets.NoTownNPCHappiness[Type] = true;

		var drawModifiers = new NPCID.Sets.NPCBestiaryDrawModifiers()
		{
			Velocity = 1f
		};
		NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);
	}

	public override void SetDefaults()
	{
		NPC.CloneDefaults(NPCID.Wizard);
		NPC.townNPC = true;
		NPC.friendly = true;
		NPC.aiStyle = 7;
		NPC.damage = 30;
		NPC.defense = 30;
		NPC.lifeMax = 250;
		NPC.HitSound = SoundID.NPCHit1;
		NPC.DeathSound = SoundID.NPCDeath1;
		NPC.knockBackResist = 0.4f;
		AnimationType = NPCID.Wizard;
		
		NPC.TryEnableComponent<NPCHitEffects>(
			c =>
			{
				c.AddGore(new NPCHitEffects.GoreSpawnParameters($"{PoTMod.ModName}/{Name}_0", 1, NPCHitEffects.OnDeath));
				c.AddGore(new NPCHitEffects.GoreSpawnParameters($"{PoTMod.ModName}/{Name}_1", 1, NPCHitEffects.OnDeath));
				c.AddGore(new NPCHitEffects.GoreSpawnParameters($"{PoTMod.ModName}/{Name}_2", 2, NPCHitEffects.OnDeath));
				
				c.AddDust(new NPCHitEffects.DustSpawnParameters(DustID.Blood, 20));
			}
		);
		
		NPC.TryEnableComponent<NPCTownDialogue>(
			c =>
			{
				c.AddDialogue(new NPCTownDialogue.DialogueEntry($"Mods.{PoTMod.ModName}.NPCs.{Name}.Dialogue.Common0"));
				c.AddDialogue(new NPCTownDialogue.DialogueEntry($"Mods.{PoTMod.ModName}.NPCs.{Name}.Dialogue.Common1"));
				c.AddDialogue(new NPCTownDialogue.DialogueEntry($"Mods.{PoTMod.ModName}.NPCs.{Name}.Dialogue.Common2"));
				c.AddDialogue(new NPCTownDialogue.DialogueEntry($"Mods.{PoTMod.ModName}.NPCs.{Name}.Dialogue.Common3"));
				c.AddDialogue(new NPCTownDialogue.DialogueEntry($"Mods.{PoTMod.ModName}.NPCs.{Name}.Dialogue.Common4"));
			}
		);
	}

	public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
	{
		bestiaryEntry.AddInfo(this, "Surface");
	}

	public override void AddShops()
	{
		var shop = new NPCShop(Type);
		shop.Add(new NPCShop.Entry(new Item(ModContent.ItemType<UnfoldingShard>()) { shopCustomPrice = Item.buyPrice(0, 10, 0, 0) }));
		shop.Add(new NPCShop.Entry(new Item(ModContent.ItemType<GlimmeringShard>()) { shopCustomPrice = Item.buyPrice(0, 25, 0, 0) }, 
			new Condition(LocalizedText.Empty, () => Quest.GetLocalPlayerInstance<WizardStartQuest>().Completed)));

		Condition conditions = new("Mods.PathOfTerraria.Misc.VoidPearlCondition", () => QuestReady());
		shop.Add(new NPCShop.Entry(new Item(ModContent.ItemType<VoidPearl>()) { shopCustomPrice = Item.buyPrice(0, 50, 0, 0) }, conditions));

		shop.Add<WeakMalaiseItem>(Condition.DownedEowOrBoc);
		shop.Add<PusBlockItem>(Condition.DownedEowOrBoc);
		shop.Register();
	}

	public static bool QuestReady()
	{
		var quest = Quest.GetLocalPlayerInstance<WoFQuest>();
		return quest.Active && quest.CurrentStep > 2;
	}

	public override void TownNPCAttackStrength(ref int damage, ref float knockback)
	{
		damage = 30;
		knockback = 8;
	}

	public override void TownNPCAttackCooldown(ref int cooldown, ref int randExtraCooldown)
	{
		cooldown = 5;
		randExtraCooldown = 5;
	}

	public override void TownNPCAttackProj(ref int projType, ref int attackDelay)
	{
		projType = Main.rand.NextBool() ? ProjectileID.SpectreWrath : ProjectileID.InfernoFriendlyBlast;
		attackDelay = 1;
	}

	public override void TownNPCAttackProjSpeed(ref float multiplier, ref float gravityCorrection, ref float randomOffset)
	{
		multiplier = 4f;
		randomOffset = 2f;
	}

	public override ITownNPCProfile TownNPCProfile()
	{
		return this.GetDefaultProfile();
	}

	public override void SetChatButtons(ref string button, ref string button2)
	{
		button = Language.GetTextValue("LegacyInterface.28");
		button2 = !QuestUnlockManager.CanStartQuest<WizardStartQuest>() ? "" : Language.GetTextValue("Mods.PathOfTerraria.NPCs.Quest");

		if (button2 == "" && Main.hardMode && Quest.GetLocalPlayerInstance<QueenSlimeQuest>().CanBeStarted)
		{
			button2 = Language.GetTextValue("Mods.PathOfTerraria.NPCs.Quest");
		}
	}

	public override void OnChatButtonClicked(bool firstButton, ref string shopName)
	{
		if (firstButton)
		{
			shopName = "Shop";
		}
		else
		{
			if (QuestUnlockManager.CanStartQuest<WizardStartQuest>())
			{
				Main.npcChatCornerItem = ModContent.ItemType<TomeOfTheElders>();
				Main.npcChatText = Language.GetTextValue("Mods.PathOfTerraria.NPCs.WizardNPC.Dialogue.Quest");
				Main.LocalPlayer.GetModPlayer<QuestModPlayer>().StartQuest<WizardStartQuest>();
			}
			else
			{
				Main.npcChatText = this.GetLocalizedValue("Dialogue.GiveQSQuest");
				Main.LocalPlayer.GetModPlayer<QuestModPlayer>().StartQuest<QueenSlimeQuest>();
			}
		}
	}

	public bool HasQuestMarker(out Quest quest)
	{
		quest = Quest.GetLocalPlayerInstance<WizardStartQuest>();

		if (quest.Completed && Main.hardMode && Quest.GetLocalPlayerInstance<QueenSlimeQuest>().CanBeStarted)
		{
			quest = Quest.GetLocalPlayerInstance<QueenSlimeQuest>();
		}

		return !quest.Completed;
	}
}

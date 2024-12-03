using PathOfTerraria.Common.NPCs.Components;
using PathOfTerraria.Common.NPCs.Dialogue;
using PathOfTerraria.Common.NPCs.Effects;
using PathOfTerraria.Common.Systems.Questing;
using PathOfTerraria.Common.Systems.Questing.Quests.MainPath;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using PathOfTerraria.Common.Utilities.Extensions;
using PathOfTerraria.Common.NPCs;
using Terraria.DataStructures;
using PathOfTerraria.Common.NPCs.OverheadDialogue;
using Terraria.GameContent.Bestiary;
using NPCUtils;
using PathOfTerraria.Content.Items.Quest;
using PathOfTerraria.Content.Items.Consumables.Maps.BossMaps;
using PathOfTerraria.Common.NPCs.QuestMarkers;

namespace PathOfTerraria.Content.NPCs.Town;

[AutoloadHead]
public class RhineNPC : ModNPC, IQuestMarkerNPC, ISpawnInRavencrestNPC, IOverheadDialogueNPC
{
	Point16 ISpawnInRavencrestNPC.TileSpawn => new(255, 165);
	OverheadDialogueInstance IOverheadDialogueNPC.CurrentDialogue { get; set; }

	public override void SetStaticDefaults()
	{
		Main.npcFrameCount[NPC.type] = 25;

		NPCID.Sets.ExtraFramesCount[NPC.type] = 9;
		NPCID.Sets.AttackFrameCount[NPC.type] = 4;
		NPCID.Sets.DangerDetectRange[NPC.type] = 500;
		NPCID.Sets.AttackType[NPC.type] = 0;
		NPCID.Sets.AttackTime[NPC.type] = 16;
		NPCID.Sets.AttackAverageChance[NPC.type] = 30;
		NPCID.Sets.NoTownNPCHappiness[Type] = true;
	}

	public override void SetDefaults()
	{
		NPC.CloneDefaults(NPCID.Guide);
		NPC.townNPC = true;
		NPC.friendly = true;
		NPC.aiStyle = 7;
		NPC.damage = 30;
		NPC.defense = 30;
		NPC.lifeMax = 250;
		NPC.HitSound = SoundID.NPCHit1;
		NPC.DeathSound = SoundID.NPCDeath1;
		NPC.knockBackResist = 0.4f;
		AnimationType = NPCID.Guide;
		
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
			}
		);
	}

	public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
	{
		bestiaryEntry.AddInfo(this, "Surface");
	}

	public override void TownNPCAttackStrength(ref int damage, ref float knockback)
	{
		damage = 13;
		knockback = 3f;
	}

	public override void TownNPCAttackCooldown(ref int cooldown, ref int randExtraCooldown)
	{
		cooldown = 5;
		randExtraCooldown = 5;
	}

	public override void TownNPCAttackProj(ref int projType, ref int attackDelay)
	{
		projType = 507;
		attackDelay = 1;
	}

	public override void TownNPCAttackProjSpeed(ref float multiplier, ref float gravityCorrection, ref float randomOffset)
	{
		multiplier = 11f;
		randomOffset = 2f;
	}

	public override ITownNPCProfile TownNPCProfile()
	{
		return this.GetDefaultProfile();
	}

	public override void SetChatButtons(ref string button, ref string button2)
	{
		button = Main.LocalPlayer.HasItem(ModContent.ItemType<SimpleCompass>()) ? "" : Language.GetTextValue("Mods.PathOfTerraria.NPCs.RhineNPC.NewCompass.Button");
		button2 = !ModContent.GetInstance<DeerclopsQuest>().CanBeStarted ? "" : Language.GetTextValue("Mods.PathOfTerraria.NPCs.Quest");
	}

	public override void OnChatButtonClicked(bool firstButton, ref string shopName)
	{
		if (firstButton)
		{
			if (ModContent.GetInstance<DeerclopsQuest>().Active)
			{
				if (Main.LocalPlayer.CountItem(ItemID.Wood, 16) >= 15)
				{
					Main.npcChatText = Language.GetTextValue("Mods.PathOfTerraria.NPCs.RhineNPC.NewCompass.OnCraft");
					Item.NewItem(new EntitySource_Gift(NPC), NPC.Bottom, ModContent.ItemType<SimpleCompass>());

					for (int i = 0; i < 15; ++i)
					{
						Main.LocalPlayer.ConsumeItem(ItemID.Wood);
					}
				}
				else
				{
					Main.npcChatText = Language.GetTextValue("Mods.PathOfTerraria.NPCs.RhineNPC.NewCompass.CantCraft");
				}
			}
		}
		else
		{
			Main.npcChatText = Language.GetTextValue("Mods.PathOfTerraria.NPCs.RhineNPC.Dialogue.Quest");
			Main.LocalPlayer.GetModPlayer<QuestModPlayer>().StartQuest($"{PoTMod.ModName}/{nameof(DeerclopsQuest)}");

			Item.NewItem(new EntitySource_Gift(NPC), NPC.Hitbox, ModContent.ItemType<SimpleCompass>());
		}
	}

	private float animCounter;

	public override void FindFrame(int frameHeight)
	{
		if (!NPC.IsABestiaryIconDummy)
		{
			return;
		}

		animCounter += 0.25f;

		if (animCounter >= 16)
		{
			animCounter = 2;
		}
		else if (animCounter < 2)
		{
			animCounter = 2;
		}

		int frame = (int)animCounter;
		NPC.frame.Y = frame * frameHeight;
	}

	public bool HasQuestMarker(out Quest quest)
	{
		quest = ModContent.GetInstance<DeerclopsQuest>();
		return !quest.Completed;
	}
}

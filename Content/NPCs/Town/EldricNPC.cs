using PathOfTerraria.Common.NPCs.Components;
using PathOfTerraria.Common.NPCs.Effects;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using PathOfTerraria.Common.Utilities.Extensions;
using PathOfTerraria.Common.Systems.Questing;
using PathOfTerraria.Common.Systems.Questing.Quests.MainPath;
using PathOfTerraria.Common.NPCs;
using PathOfTerraria.Common.NPCs.OverheadDialogue;
using PathOfTerraria.Common.NPCs.Dialogue;

namespace PathOfTerraria.Content.NPCs.Town;

[AutoloadHead]
public sealed class EldricNPC : ModNPC, IQuestMarkerNPC, IOverheadDialogueNPC
{
	OverheadDialogueInstance IOverheadDialogueNPC.CurrentDialogue { get; set; }

	private float animCounter;

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
		NPC.CloneDefaults(NPCID.ArmsDealer);
		NPC.townNPC = true;
		NPC.friendly = true;
		NPC.aiStyle = 7;
		NPC.defense = 30;
		NPC.lifeMax = 250;
		NPC.HitSound = SoundID.NPCHit1;
		NPC.DeathSound = SoundID.NPCDeath1;
		NPC.knockBackResist = 0.5f;
		AnimationType = NPCID.Guide;
		
		NPC.TryEnableComponent<NPCHitEffects>(
			c =>
			{
				c.AddGore(new($"{PoTMod.ModName}/{Name}_0", 1));
				c.AddGore(new($"{PoTMod.ModName}/{Name}_1", 2));
				c.AddGore(new($"{PoTMod.ModName}/{Name}_2", 2));
				
				c.AddDust(new(DustID.Blood, 20));
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
		button = Language.GetTextValue("LegacyInterface.28");
		button2 = /*!ModContent.GetInstance<EoCQuest>().CanBeStarted ? "" :*/ Language.GetTextValue("Mods.PathOfTerraria.NPCs.Quest");
	}

	public override void OnChatButtonClicked(bool firstButton, ref string shopName)
	{
		if (firstButton)
		{
			shopName = "Shop";
		}
		else
		{
			Main.npcChatText = Language.GetTextValue("Mods.PathOfTerraria.NPCs.EldricNPC.Dialogue.Quest");
			Main.LocalPlayer.GetModPlayer<QuestModPlayer>().StartQuest($"{PoTMod.ModName}/{nameof(EoCQuest)}");
		}
	}

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
		quest = ModContent.GetInstance<EoCQuest>();
		return !quest.Completed;
	}
}

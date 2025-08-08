using NPCUtils;
using PathOfTerraria.Common.NPCs;
using PathOfTerraria.Common.NPCs.Components;
using PathOfTerraria.Common.NPCs.Dialogue;
using PathOfTerraria.Common.NPCs.Effects;
using PathOfTerraria.Common.NPCs.OverheadDialogue;
using PathOfTerraria.Common.NPCs.QuestMarkers;
using PathOfTerraria.Common.Subworlds.RavencrestContent;
using PathOfTerraria.Common.Systems.BossTrackingSystems;
using PathOfTerraria.Common.Systems.Questing;
using PathOfTerraria.Common.Systems.Questing.Quests.MainPath;
using PathOfTerraria.Common.Utilities.Extensions;
using PathOfTerraria.Content.Items.Quest;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.Localization;

namespace PathOfTerraria.Content.NPCs.Town;

[AutoloadHead]
public sealed class EldricNPC : ModNPC, IQuestMarkerNPC, IOverheadDialogueNPC, ISpawnInRavencrestNPC, ITavernNPC
{
	Point16 ISpawnInRavencrestNPC.TileSpawn => RavencrestSystem.Structures["Observatory"].Position.ToPoint16();
	OverheadDialogueInstance IOverheadDialogueNPC.CurrentDialogue { get; set; }

	bool ISpawnInRavencrestNPC.CanSpawn(bool worldGen, bool alreadyExists)
	{
		return (BossTracker.TotalBossesDowned.Contains(NPCID.EyeofCthulhu) || NPC.downedBoss1) && !worldGen && !alreadyExists;
	}

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

		var drawModifiers = new NPCID.Sets.NPCBestiaryDrawModifiers()
		{
			Velocity = 1f
		};
		NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);
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
		//button = Language.GetTextValue("LegacyInterface.28");

		Quest quest = Quest.GetLocalPlayerInstance<EoCQuest>();
		button2 = !quest.CanBeStarted ? "" : Language.GetTextValue("Mods.PathOfTerraria.NPCs.Quest");

		if (quest.Active && quest.CurrentStep >= 1 && !Main.LocalPlayer.HasItem(ModContent.ItemType<LunarObject>()))
		{
			button2 = this.GetLocalization("CreateConcoction").Value;

			Main.npcChatCornerItem = ModContent.ItemType<LunarObject>();
		}
		else if (quest.Completed && (Main.LocalPlayer.HasItem(ModContent.ItemType<LunarShard>()) || Main.LocalPlayer.HasItem(ModContent.ItemType<LunarLiquid>())))
		{
			button2 = this.GetLocalization("DonateLunar").Value;
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
			Quest quest = Quest.GetLocalPlayerInstance<EoCQuest>();

			if (quest.Active && quest.CurrentStep >= 1 && !Main.LocalPlayer.HasItem(ModContent.ItemType<LunarObject>()))
			{
				if (Main.LocalPlayer.CountItem(ModContent.ItemType<LunarShard>(), 5) >= 5 && Main.LocalPlayer.HasItem(ModContent.ItemType<LunarLiquid>()))
				{
					Main.npcChatText = this.GetLocalization("Dialogue.TradeLunarObject").Value;
					int item = Item.NewItem(new EntitySource_Gift(NPC), NPC.Hitbox, ModContent.ItemType<LunarObject>(), noGrabDelay: true);

					if (Main.netMode == NetmodeID.MultiplayerClient)
					{
						NetMessage.SendData(MessageID.SyncItem, -1, -1, null, item);
					}

					Main.LocalPlayer.ConsumeItem(ModContent.ItemType<LunarLiquid>());

					for (int i = 0; i < 5; ++i)
					{
						Main.LocalPlayer.ConsumeItem(ModContent.ItemType<LunarShard>());
					}
				}
				else
				{
					Main.npcChatText = this.GetLocalization("Dialogue.CantTradeObject").Value;
				}

				return;
			}
			else if (quest.Completed)
			{
				Main.npcChatText = this.GetLocalization("Dialogue.Donation").Value;
				int shardCount = Main.LocalPlayer.CountItem(ModContent.ItemType<LunarShard>());
				int count = shardCount + Main.LocalPlayer.CountItem(ModContent.ItemType<LunarLiquid>());
				Main.LocalPlayer.QuickSpawnItem(new EntitySource_Gift(NPC), ItemID.SilverCoin, count);

				for (int i = 0; i < count - shardCount; ++i)
				{
					Main.LocalPlayer.ConsumeItem(ModContent.ItemType<LunarLiquid>());
				}

				for (int i = 0; i < shardCount; ++i)
				{
					Main.LocalPlayer.ConsumeItem(ModContent.ItemType<LunarShard>());
				}

				return;
			}

			Main.npcChatText = Language.GetTextValue("Mods.PathOfTerraria.NPCs.EldricNPC.Dialogue.Quest");
			Main.LocalPlayer.GetModPlayer<QuestModPlayer>().StartQuest<EoCQuest>();
		}
	}

	public bool HasQuestMarker(out Quest quest)
	{
		quest = Quest.GetLocalPlayerInstance<EoCQuest>();
		return !quest.Completed;
	}

	public bool ForceSpawnInTavern()
	{
		HashSet<int> downs = BossTracker.TotalBossesDowned;
		return downs.Contains(NPCID.KingSlime) && !downs.Contains(NPCID.EyeofCthulhu) || Quest.GetLocalPlayerInstance<EoCQuest>().Active;
	}

	public float SpawnChanceInTavern()
	{
		return 0f;
	}
}

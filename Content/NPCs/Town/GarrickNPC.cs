using PathOfTerraria.Common.NPCs.Components;
using PathOfTerraria.Common.NPCs.Effects;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using PathOfTerraria.Common.Utilities;
using PathOfTerraria.Common.Utilities.Extensions;
using PathOfTerraria.Common.Systems.Questing;
using PathOfTerraria.Common.Systems.Questing.Quests.MainPath;
using PathOfTerraria.Common.NPCs.OverheadDialogue;
using Terraria.GameContent.Bestiary;
using NPCUtils;
using PathOfTerraria.Common.NPCs.QuestMarkers;
using PathOfTerraria.Content.Items.Quest;
using Terraria.DataStructures;
using PathOfTerraria.Content.Items.Consumables.Maps.BossMaps;
using PathOfTerraria.Common.NPCs;

namespace PathOfTerraria.Content.NPCs.Town;

[AutoloadHead]
public sealed class GarrickNPC : ModNPC, IQuestMarkerNPC, IOverheadDialogueNPC, ITavernNPC
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
				c.AddGore(new($"{PoTMod.ModName}/{Name}_0", 1));
				c.AddGore(new($"{PoTMod.ModName}/{Name}_1", 2));
				c.AddGore(new($"{PoTMod.ModName}/{Name}_2", 2));
				
				c.AddDust(new(DustID.Blood, 20));
			}
		);
	}

	public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
	{
		bestiaryEntry.AddInfo(this, "Surface");
	}

	public override string GetChat()
	{
		return Language.GetTextValue("Mods.PathOfTerraria.NPCs.GarrickNPC.Dialogue." + Main.rand.Next(4));
	}

	public override void AddShops()
	{
		if (!ShopUtils.TryCloneNpcShop("Terraria/DD2Bartender/Shop", Type))
		{
			Mod.Logger.Error($"Failed to clone shop 'Terraria/DD2Bartender/Shop' to NPC '{Name}'!");
		}
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
		button2 = !Quest.GetLocalPlayerInstance<KingSlimeQuest>().CanBeStarted ? "" : Language.GetTextValue("Mods.PathOfTerraria.NPCs.Quest");

		EoCQuest quest = Quest.GetLocalPlayerInstance<EoCQuest>();

		if (quest.Active && quest.CurrentStep >= 1 && !Main.LocalPlayer.HasItem(ModContent.ItemType<LunarLiquid>()))
		{
			button2 = this.GetLocalization("LunarLiquidButton").Value;
			Main.npcChatCornerItem = ModContent.ItemType<LunarLiquid>();
		}

		KingSlimeQuest kingQuest = Quest.GetLocalPlayerInstance<KingSlimeQuest>();

		if (kingQuest.Active && kingQuest.CurrentStep >= 1 && !Main.LocalPlayer.HasItem(ModContent.ItemType<KingSlimeMap>()))
		{
			button2 = this.GetLocalization("AnotherMap").Value;
			Main.npcChatCornerItem = ModContent.ItemType<KingSlimeMap>();
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
			EoCQuest quest = Quest.GetLocalPlayerInstance<EoCQuest>();
			KingSlimeQuest kingQuest = Quest.GetLocalPlayerInstance<KingSlimeQuest>();

			if (kingQuest.Active && kingQuest.CurrentStep >= 1 && !Main.LocalPlayer.HasItem(ModContent.ItemType<KingSlimeMap>()))
			{
				Item.NewItem(new EntitySource_Gift(NPC), NPC.Hitbox, ModContent.ItemType<LunarLiquid>());
				Main.npcChatText = this.GetLocalization("Dialogue.GetKingMapAgain").Value;
				return;
			}

			if (quest.Active && quest.CurrentStep >= 1 && !Main.LocalPlayer.HasItem(ModContent.ItemType<LunarLiquid>()))
			{
				if (Main.LocalPlayer.CountItem(ModContent.ItemType<LunarShard>(), 5) >= 5)
				{
					Main.npcChatText = this.GetLocalization("Dialogue.TradeLunarLiquid").Value;
					Item.NewItem(new EntitySource_Gift(NPC), NPC.Hitbox, ModContent.ItemType<LunarLiquid>());

					for (int i = 0; i < 5; ++i)
					{
						Main.LocalPlayer.ConsumeItem(ModContent.ItemType<LunarShard>());
					}
				}
				else
				{
					Main.npcChatText = this.GetLocalization("Dialogue.CantTradeLiquid").Value;
				}

				return; // EoC quest is after King Slime quest, shouldn't be possible to do this before needing KS quest
			}

			Main.npcChatText = Language.GetTextValue("Mods.PathOfTerraria.NPCs.GarrickNPC.Dialogue.Quest");
			Main.LocalPlayer.GetModPlayer<QuestModPlayer>().StartQuest($"{PoTMod.ModName}/{nameof(KingSlimeQuest)}");
			Item.NewItem(new EntitySource_Gift(NPC), NPC.Hitbox, ModContent.ItemType<KingSlimeMap>());
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
		quest = Quest.GetLocalPlayerInstance<KingSlimeQuest>();
		return !quest.Completed;
	}

	public bool ForceSpawnInTavern()
	{
		return QuestSystem.ForceGarrickSpawn();
	}

	public float SpawnChanceInTavern()
	{
		return NPC.downedSlimeKing ? 0.2f : 0f;
	}
}

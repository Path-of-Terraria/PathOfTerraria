using PathOfTerraria.Common.NPCs;
using PathOfTerraria.Common.NPCs.OverheadDialogue;
using PathOfTerraria.Common.NPCs.Components;
using PathOfTerraria.Common.NPCs.Dialogue;
using PathOfTerraria.Common.NPCs.Effects;
using PathOfTerraria.Common.Systems.Questing;
using PathOfTerraria.Common.Systems.Questing.Quests.MainPath;
using PathOfTerraria.Common.Utilities;
using PathOfTerraria.Common.Utilities.Extensions;
using PathOfTerraria.Content.Items.Gear.Weapons.Bow;
using ReLogic.Content;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.GameContent.Bestiary;
using NPCUtils;
using PathOfTerraria.Content.Items.Consumables.Maps.BossMaps;
using Terraria;
using PathOfTerraria.Common.NPCs.QuestMarkers;

namespace PathOfTerraria.Content.NPCs.Town;

[AutoloadHead]
public class HunterNPC : ModNPC, IQuestMarkerNPC, ISpawnInRavencrestNPC, IOverheadDialogueNPC
{
	Point16 ISpawnInRavencrestNPC.TileSpawn => new(319, 163);
	OverheadDialogueInstance IOverheadDialogueNPC.CurrentDialogue { get; set; }

	public override void SetStaticDefaults()
	{
		Main.npcFrameCount[Type] = 23;
		NPCID.Sets.NoTownNPCHappiness[Type] = true;
		NPCID.Sets.DangerDetectRange[Type] = 400;
		NPCID.Sets.AttackFrameCount[Type] = 4;
		NPCID.Sets.AttackType[Type] = 1;
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
		AnimationType = NPCID.BestiaryGirl;

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

	public override void SetChatButtons(ref string button, ref string button2)
	{
		button = Language.GetTextValue("LegacyInterface.28");

		if (Quest.GetLocalPlayerInstance<DeerclopsQuest>().Active)
		{
			button2 = this.GetLocalizedValue("MapButton");
			Main.npcChatCornerItem = ModContent.ItemType<DeerclopsMap>();
		}
		else
		{
			button2 = !Quest.GetLocalPlayerInstance<HunterStartQuest>().CanBeStarted ? "" : Language.GetOrRegister($"Mods.{PoTMod.ModName}.NPCs.Quest").Value;
		}
	}

	public override void OnChatButtonClicked(bool firstButton, ref string shopName)
	{
		if (firstButton)
		{
			shopName = "Shop";
			return;
		}

		if (Quest.GetLocalPlayerInstance<DeerclopsQuest>().Active)
		{
			if (Main.LocalPlayer.BuyItem(Item.buyPrice(0, 20, 0, 0)))
			{
				Main.npcChatText = Language.GetTextValue("Mods.PathOfTerraria.NPCs.HunterNPC.Dialogue.BuyMap");
				Item.NewItem(new EntitySource_Gift(NPC), NPC.Bottom, ModContent.ItemType<DeerclopsMap>());
			}
			else
			{
				Main.npcChatText = Language.GetTextValue("Mods.PathOfTerraria.NPCs.HunterNPC.Dialogue.BuyMapFail");
			}
		}
		else
		{
			Main.npcChatText = Language.GetTextValue("Mods.PathOfTerraria.NPCs.HunterNPC.Dialogue.Quest");
			Main.LocalPlayer.GetModPlayer<QuestModPlayer>().StartQuest($"{PoTMod.ModName}/{nameof(HunterStartQuest)}");
		}
	}
	
	public override void AddShops()
	{
		new NPCShop(Type)
			.Add<WoodenBow>()
			.Add<WoodenShortBow>()
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

	public override void DrawTownAttackGun(ref Texture2D item, ref Rectangle itemFrame, ref float scale, ref int horizontalHoldoutOffset)
	{
		int type = ModContent.ItemType<WoodenBow>();
		
		Asset<Texture2D> asset = TextureUtils.LoadAndGetItem(type);
		
		item = asset.Value;
		itemFrame = asset.Frame();
	}

	public bool HasQuestMarker(out Quest quest)
	{
		quest = Quest.GetLocalPlayerInstance<HunterStartQuest>();

		if (Quest.GetLocalPlayerInstance<DeerclopsQuest>().Active)
		{
			return false;
		}

		return !quest.Completed;
	}
}
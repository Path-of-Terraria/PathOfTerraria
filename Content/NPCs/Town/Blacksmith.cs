using System.Collections.Generic;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.GameContent.Bestiary;
using PathOfTerraria.Content.Items.Gear.Weapons.Battleaxe;
using PathOfTerraria.Content.Items.Gear.Weapons.Sword;
using PathOfTerraria.Helpers.Extensions;
using PathOfTerraria.Core.Systems.Questing;
using PathOfTerraria.Core.Systems.Questing.Quests.TestQuest;

namespace PathOfTerraria.Content.NPCs.Town;

[AutoloadHead]
public class Blacksmith : ModNPC
{
	public override string Texture => "PathOfTerraria/Assets/NPCs/Town/Blacksmith";

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
	}

	public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
	{
		//bestiaryEntry.AddInfo(this, "Surface");
	}

	public override void HitEffect(NPC.HitInfo hit)
	{
		if (NPC.life <= 0 && Main.netMode != NetmodeID.Server)
		{
			Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("Blacksmith_0").Type);

			for (int i = 0; i < 2; ++i)
			{
				Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("Blacksmith_1").Type);
				Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("Blacksmith_2").Type);
			}
		}
	}

	public override List<string> SetNPCNameList()
	{
		return [Language.GetOrRegister("Mods.PathOfTerraria.NPCs.Blacksmith.DisplayName").Value];
	}

	public override string GetChat()
	{
		return Language.GetTextValue("Mods.PathOfTerraria.NPCs.Blacksmith.Dialogue." + Main.rand.Next(4));
	}

	public override void AddShops()
	{
		var shop = new NPCShop(Type);
		shop.Add<RustedBattleaxe>();
		shop.Add<StoneSword>();
		shop.Register();
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
		return this.DefaultProfile();
	}

	public override void SetChatButtons(ref string button, ref string button2)
	{
		button = Language.GetTextValue("LegacyInterface.28");
		button2 = Language.GetTextValue("Mods.PathOfTerraria.NPCs.Quest");
	}

	public override void OnChatButtonClicked(bool firstButton, ref string shopName)
	{
		if (firstButton)
		{
			shopName = "Shop";
		}
		else
		{
			Main.npcChatText = Language.GetTextValue("Mods.PathOfTerraria.NPCs.Blacksmith.Dialogue.Quest");
			Main.LocalPlayer.GetModPlayer<QuestModPlayer>().StartQuest<BlacksmithStartQuest>();
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
}

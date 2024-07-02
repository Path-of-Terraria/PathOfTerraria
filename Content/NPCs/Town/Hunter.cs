using System.Collections.Generic;
using PathOfTerraria.Content.Items.Gear.Weapons.Boomerangs;
using PathOfTerraria.Content.Items.Gear.Weapons.Bow;
using PathOfTerraria.Core.Systems.Questing;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;

namespace PathOfTerraria.Content.NPCs.Town;

[AutoloadHead]
public class Hunter : ModNPC
{
	private int dialogue;

	/// <summary>
	///		The current dialogue sentence being used.
	/// </summary>
	/// <remarks>
	///		This is utilized to refrain from random dialogue selection.
	/// </remarks>
	public int Dialogue
	{
		get => dialogue;
		set => dialogue = value > 2 ? 0 : value;
	}
	
	public override string HeadTexture { get; } = "Terraria/Images/NPC_Head_" + NPCHeadID.BestiaryGirl;
	
	public override string Texture { get; } = "Terraria/Images/NPC_" + NPCID.BestiaryGirl;

	public override void SetStaticDefaults()
	{
		Main.npcFrameCount[Type] = Main.npcFrameCount[NPCID.BestiaryGirl];

		NPCID.Sets.AttackFrameCount[Type] = NPCID.Sets.AttackFrameCount[NPCID.BestiaryGirl];
		NPCID.Sets.DangerDetectRange[Type] = 400;
		NPCID.Sets.AttackType[Type] = 1;
		NPCID.Sets.AttackTime[Type] = 10;
		NPCID.Sets.AttackAverageChance[Type] = 5;
		
		NPCID.Sets.NoTownNPCHappiness[Type] = true;
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
	}
	
	public override ITownNPCProfile TownNPCProfile()
	{
		return new Profiles.DefaultNPCProfile(Texture, ModContent.GetModHeadSlot(HeadTexture));
	}
	
	public override List<string> SetNPCNameList()
	{
		return [DisplayName.Value];
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
		var asset = TextureAssets.Item[ModContent.ItemType<WoodenBow>()];
		
		item = asset.Value;
		itemFrame = asset.Frame();
	}

	public override void SetChatButtons(ref string button, ref string button2)
	{
		button = Language.GetTextValue("LegacyInterface.28");
		button2 = Language.GetOrRegister($"Mods.{nameof(PathOfTerraria)}.GUI.TownNPCQuestsTab").Value;
	}
	
	public override void OnChatButtonClicked(bool firstButton, ref string shopName)
	{
		if (firstButton)
		{
			shopName = "Shop";
			return;
		}

		Main.npcChatText = this.GetLocalizedValue("Dialogue.Quest");

		if (!Main.LocalPlayer.TryGetModPlayer(out QuestModPlayer modPlayer))
		{
			return;
		}
		
		// TODO: Implement quest.
		modPlayer.RestartQuestTest();
	}

	public override string GetChat()
	{
		var dialogue = this.GetLocalizedValue("Dialogue." + Dialogue);

		Dialogue++;
		
		return dialogue;
	}

	public override void AddShops()
	{
		new NPCShop(Type)
			.Add<WoodenBow>()
			.Add<WoodenShortBow>()
			.Register();
	}

	// TODO: Implement gore.
	public override void HitEffect(NPC.HitInfo hit)
	{
		var amount = NPC.life > 0 ? 3 : 10;
		
		for (var i = 0; i < amount; i++)
		{
			Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Blood);
		}
	}
}
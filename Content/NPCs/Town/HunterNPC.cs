using PathOfTerraria.Common.NPCs.Components;
using PathOfTerraria.Common.NPCs.Effects;
using PathOfTerraria.Common.Systems.Questing;
using PathOfTerraria.Common.Utilities;
using PathOfTerraria.Common.Utilities.Extensions;
using PathOfTerraria.Content.Items.Gear.Weapons.Bow;
using ReLogic.Content;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;

namespace PathOfTerraria.Content.NPCs.Town;

[AutoloadHead]
public class HunterNPC : ModNPC
{
	/// <summary>
	///     The index of the current dialogue sentence.
	/// </summary>
	public int Dialogue
	{
		get => dialogue;
		set => dialogue = value > 2 ? 0 : value;
	}

	private int dialogue;

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

		NPC.TryEnableComponent<NPCDeathEffects>(
			c =>
			{
				c.AddGore($"{PoTMod.ModName}/{Name}_0", 1);
				c.AddGore($"{PoTMod.ModName}/{Name}_1", 2);
				c.AddGore($"{PoTMod.ModName}/{Name}_2", 2);
				
				c.AddDust(DustID.Blood, 20);
			}
		);
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

	public override void SetChatButtons(ref string button, ref string button2)
	{
		button = Language.GetTextValue("LegacyInterface.28");
		button2 = Language.GetOrRegister($"Mods.{PoTMod.ModName}.NPCs.Quest").Value;
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

		modPlayer.RestartQuestTest();
	}

	public override string GetChat()
	{
		string dialogue = this.GetLocalizedValue("Dialogue." + Dialogue);

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
}
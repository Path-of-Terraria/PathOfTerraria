using NPCUtils;
using PathOfTerraria.Common.NPCs;
using PathOfTerraria.Common.NPCs.BestiaryInfoProviders;
using PathOfTerraria.Common.NPCs.Components;
using PathOfTerraria.Common.NPCs.Effects;
using Terraria.GameContent.Bestiary;
using Terraria.ID;

namespace PathOfTerraria.Content.NPCs.Mapping.Desert;

[AutoloadBanner]
internal class SwarmScarab : ModNPC
{
	public NPC Controller => Main.npc[Owner];

	private int Owner => (int)NPC.ai[0];

	public override void SetStaticDefaults()
	{
		Main.npcFrameCount[Type] = 2;

		var drawModifiers = new NPCID.Sets.NPCBestiaryDrawModifiers()
		{
			Velocity = 1.5f
		};
		NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);
	}

	public override void SetDefaults()
	{
		NPC.Size = new Vector2(34, 30);
		NPC.aiStyle = -1;
		NPC.lifeMax = 60;
		NPC.defense = 0;
		NPC.damage = 50;
		NPC.HitSound = SoundID.NPCHit23;
		NPC.DeathSound = SoundID.NPCDeath16;
		NPC.noGravity = true;
		NPC.noTileCollide = true;
		NPC.color = Color.White;
		NPC.value = 0;
		NPC.npcSlots = 0.5f;

		NPC.TryEnableComponent<NPCHitEffects>(
			c =>
			{
				c.AddGore(new NPCHitEffects.GoreSpawnParameters($"{PoTMod.ModName}/Scarab_0", 2, NPCHitEffects.OnDeath));
				c.AddGore(new NPCHitEffects.GoreSpawnParameters($"{PoTMod.ModName}/Scarab_1", 1, NPCHitEffects.OnDeath));
				c.AddGore(new NPCHitEffects.GoreSpawnParameters($"{PoTMod.ModName}/Scarab_2", 1, NPCHitEffects.OnDeath));

				c.AddDust(new NPCHitEffects.DustSpawnParameters(DustID.Enchanted_Gold, 5));
				c.AddDust(new NPCHitEffects.DustSpawnParameters(DustID.Enchanted_Gold, 20, NPCHitEffects.OnDeath));
			}
		);
	}

	public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
	{
		bestiaryEntry.AddInfo(this, "Desert");
		bestiaryEntry.UIInfoProvider = new CustomInfoProvider(ContentSamples.NpcBestiaryCreditIdsByNpcNetIds[Type], false, 150);
	}

	public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
	{
		NPC.lifeMax = ModeUtils.ByMode(70, 115, 160);
		NPC.damage = ModeUtils.ByMode(70, 80, 110);
		NPC.defense = ModeUtils.ByMode(0, 5, 10, 20);
	}

	public override void AI()
	{
		const float MaxSpeed = 14;

		Vector2 vel = NPC.DirectionTo(Controller.Center) * 0.8f;

		if (vel.HasNaNs())
		{
			vel = Vector2.Zero;
		}

		NPC.velocity += vel;

		if (NPC.velocity.LengthSquared() > MaxSpeed * MaxSpeed)
		{
			NPC.velocity = Vector2.Normalize(NPC.velocity) * MaxSpeed;
		}

		NPC.spriteDirection = Math.Sign(NPC.velocity.X);

		if (Controller.ModNPC is not ScarabSwarmController controller)
		{
			NPC.active = false;
			return;
		}

		controller.UnderlingCount++;
	}

	public override void FindFrame(int frameHeight)
	{
		NPC.frameCounter += NPC.velocity.Length() * 0.2f;
		NPC.frame.Y = (int)(NPC.frameCounter % 2) * frameHeight;
	}

	public override void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone)
	{
		OwnerCopyInteractions();
	}

	public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone)
	{
		OwnerCopyInteractions();
	}

	private void OwnerCopyInteractions()
	{
		Controller.CopyInteractions(NPC);
	}
}

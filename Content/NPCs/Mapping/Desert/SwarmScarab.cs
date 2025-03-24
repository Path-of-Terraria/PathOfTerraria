using NPCUtils;
using PathOfTerraria.Common.NPCs;
using PathOfTerraria.Common.NPCs.Components;
using PathOfTerraria.Common.NPCs.Effects;
using Terraria.GameContent.Bestiary;
using Terraria.ID;

namespace PathOfTerraria.Content.NPCs.Mapping.Desert;

internal class SwarmScarab : ModNPC
{
	public NPC Controller => Main.npc[Owner];

	private int Owner => (int)NPC.ai[0];

	public override void SetStaticDefaults()
	{
		Main.npcFrameCount[Type] = 2;

		var drawModifiers = new NPCID.Sets.NPCBestiaryDrawModifiers()
		{
			Hide = true
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
		NPC.HitSound = SoundID.NPCHit30;
		NPC.noGravity = true;
		NPC.noTileCollide = true;
		NPC.color = Color.White;
		NPC.value = 0;

		NPC.TryEnableComponent<NPCHitEffects>(
			c =>
			{
				c.AddGore(new NPCHitEffects.GoreSpawnParameters($"{PoTMod.ModName}/MummyPaper_0", 2, NPCHitEffects.OnDeath));
				c.AddGore(new NPCHitEffects.GoreSpawnParameters($"{PoTMod.ModName}/MummyPaper_1", 2, NPCHitEffects.OnDeath));
				c.AddGore(new NPCHitEffects.GoreSpawnParameters($"{PoTMod.ModName}/MummyPaper_2", 2, NPCHitEffects.OnDeath));

				c.AddDust(new NPCHitEffects.DustSpawnParameters(ModContent.DustType<GhostDust>(), 5));
				c.AddDust(new NPCHitEffects.DustSpawnParameters(ModContent.DustType<GhostDust>(), 20, NPCHitEffects.OnDeath));
			}
		);
	}

	public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
	{
		NPC.lifeMax = ModeUtils.ByMode(80, 110, 160);
		NPC.damage = ModeUtils.ByMode(50, 80, 120);
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
}

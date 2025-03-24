using NPCUtils;
using PathOfTerraria.Common.NPCs.Components;
using PathOfTerraria.Common.NPCs.Effects;
using Terraria.GameContent.Bestiary;
using Terraria.ID;

namespace PathOfTerraria.Content.NPCs.Mapping.Desert;

internal class ScarabSwarmController : ModNPC
{
	public enum AIState
	{
		Init,
		Chase,
	}

	public Player Target => Main.player[NPC.target];

	internal ref float UnderlingCount => ref NPC.ai[0];

	private AIState State
	{
		get => (AIState)NPC.ai[1];
		set => NPC.ai[1] = (float)value;
	}

	public override void SetStaticDefaults()
	{
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
		NPC.lifeMax = 300;
		NPC.defense = 0;
		NPC.damage = 0;
		NPC.HitSound = SoundID.NPCHit30;
		NPC.noGravity = true;
		NPC.noTileCollide = true;
		NPC.hide = true;
		NPC.color = Color.White;
		NPC.knockBackResist = 1.8f;
		NPC.dontTakeDamage = true;
		NPC.value = Item.buyPrice(0, 0, 20);

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

	public override bool CanHitPlayer(Player target, ref int cooldownSlot)
	{
		return false;
	}

	public override void AI()
	{
		if (State == AIState.Init)
		{
			State = AIState.Chase;
			int count = Main.rand.Next(5, 9);

			for (int i = 0; i < count; ++i)
			{
				int npc = NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y, ModContent.NPCType<SwarmScarab>(), 0, NPC.whoAmI);
				Main.npc[npc].velocity = Main.rand.NextVector2Circular(4, 4);
			}
		}
		else if (State == AIState.Chase)
		{
			const float MaxSpeed = 8;

			if (UnderlingCount <= 0)
			{
				NPC.life = -1;
				NPC.checkDead();
				NPC.netUpdate = true;
				return;
			}

			NPC.TargetClosest();
			NPC.velocity += NPC.DirectionTo(Target.Center) * 0.6f;

			if (NPC.velocity.LengthSquared() > MaxSpeed * MaxSpeed)
			{
				NPC.velocity = Vector2.Normalize(NPC.velocity) * MaxSpeed;
			}

			UnderlingCount = 0;
		}
	}
}

using NPCUtils;
using PathOfTerraria.Common.NPCs.Components;
using PathOfTerraria.Common.NPCs.Effects;
using System.Collections.Specialized;
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

			if (Main.netMode != NetmodeID.MultiplayerClient)
			{
				int count = Main.rand.Next(4, 8);

				if (Main.masterMode)
				{
					count += 2;
				}

				if (Main.getGoodWorld)
				{
					count += 2;
				}

				for (int i = 0; i < count; ++i)
				{
					int npc = NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y, ModContent.NPCType<SwarmScarab>(), 0, NPC.whoAmI);
					Main.npc[npc].velocity = Main.rand.NextVector2Circular(4, 4);
					Main.npc[npc].netUpdate = true;
				}
			}
		}
		else if (State == AIState.Chase)
		{
			const float MaxSpeed = 5;

			if (UnderlingCount <= 0 && Main.netMode != NetmodeID.MultiplayerClient)
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

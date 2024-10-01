using PathOfTerraria.Common.NPCs.Components;
using PathOfTerraria.Common.NPCs.Effects;
using PathOfTerraria.Common.Systems.WorldNavigation;
using Terraria.ID;

namespace PathOfTerraria.Content.NPCs.Town;

public sealed class RavenNPC : ModNPC
{
	private ref float LastDropX => ref NPC.ai[3];

	public override void SetStaticDefaults()
	{
		Main.npcFrameCount[Type] = 5;
	}

	public override void SetDefaults()
	{
		NPC.CloneDefaults(NPCID.Bird);
		NPC.noTileCollide = true;

		AnimationType = NPCID.Bird;
		AIType = NPCID.Bird;

		NPC.TryEnableComponent<NPCHitEffects>(c =>
		{
			c.AddDust(new(DustID.Blood, 2));
			c.AddDust(new(DustID.Blood, 6, NPCHitEffects.OnDeath));
		});
	}

	public override bool PreAI()
	{
		Vector2 entrancePosition = ModContent.GetInstance<RavencrestSystem>().EntrancePosition.ToVector2() * 16;

		if (NPC.DistanceSQ(entrancePosition) < 600 * 600)
		{
			if (!Collision.SolidCollision(NPC.position, NPC.width, NPC.height))
			{
				NPC.noTileCollide = false;
				NPC.ai[0] = 0;
			}

		}
		
		if (NPC.direction != Math.Sign(entrancePosition.X - NPC.Center.X))
		{
			NPC.direction = Math.Sign(entrancePosition.X - NPC.Center.X);
			NPC.velocity.X *= 0.98f;
		}

		if (Math.Abs(LastDropX - NPC.Center.X) > 120)
		{
			Item.NewItem(NPC.GetSource_FromAI(), NPC.Center, ItemID.SilverCoin);
			LastDropX = NPC.Center.X;
		}

		return true;
	}

	public override void PostAI()
	{
		Vector2 entrancePosition = ModContent.GetInstance<RavencrestSystem>().EntrancePosition.ToVector2() * 16;

		if (NPC.DistanceSQ(entrancePosition) < 600 * 600 && NPC.velocity.Y < 0)
		{
			NPC.velocity.Y = 0;
		}
	}

	public override bool CheckActive()
	{
		return false;
	}
}
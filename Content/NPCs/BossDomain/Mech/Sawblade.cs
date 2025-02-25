using Terraria.ID;

namespace PathOfTerraria.Content.NPCs.BossDomain.Mech;

internal class Sawblade : ModNPC
{
	public override void SetDefaults()
	{
		NPC.CloneDefaults(NPCID.BlazingWheel);
		NPC.Opacity = 1;
		NPC.Size = new Vector2(40);
	}

	public override bool PreAI()
	{
		if (NPC.ai[0] == 0f)
		{
			NPC.TargetClosest();
			NPC.directionY = 1;
			NPC.ai[0] = 1f;
		}

		const int Speed = 7;

		if (NPC.ai[1] == 0f)
		{
			NPC.rotation += NPC.direction * NPC.directionY * 0.18f;

			if (NPC.collideY)
			{
				NPC.ai[0] = 2f;
			}

			if (!NPC.collideY && NPC.ai[0] == 2f)
			{
				NPC.direction = -NPC.direction;
				NPC.ai[1] = 1f;
				NPC.ai[0] = 1f;
			}

			if (NPC.collideX)
			{
				NPC.directionY = -NPC.directionY;
				NPC.ai[1] = 1f;
			}
		}
		else
		{
			NPC.rotation -= NPC.direction * NPC.directionY * 0.18f;

			if (NPC.collideX)
			{
				NPC.ai[0] = 2f;
			}

			if (!NPC.collideX && NPC.ai[0] == 2f)
			{
				NPC.directionY = -NPC.directionY;
				NPC.ai[1] = 0f;
				NPC.ai[0] = 1f;
			}

			if (NPC.collideY)
			{
				NPC.direction = -NPC.direction;
				NPC.ai[1] = 0f;
			}
		}

		NPC.velocity.X = Speed * NPC.direction;
		NPC.velocity.Y = Speed * NPC.directionY;

		Lighting.AddLight(NPC.Center, TorchID.Red);

		return false;
	}
}

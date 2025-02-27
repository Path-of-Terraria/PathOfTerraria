using NPCUtils;
using PathOfTerraria.Content.Scenes;
using Terraria.GameContent.Bestiary;
using Terraria.ID;

namespace PathOfTerraria.Content.NPCs.BossDomain.Mech;

internal class Sawblade : ModNPC
{
	public override void Load()
	{
		On_NPC.UpdateCollision += ChangeNPCTypeCollision;
	}

	private void ChangeNPCTypeCollision(On_NPC.orig_UpdateCollision orig, NPC self)
	{
		int oldType = self.type;

		if (self.type == ModContent.NPCType<Sawblade>())
		{
			self.type = NPCID.BlazingWheel;
		}

		orig(self);

		self.type = oldType;
	}

	public override void SetDefaults()
	{
		NPC.CloneDefaults(NPCID.BlazingWheel);
		NPC.Opacity = 1;
		NPC.Size = new Vector2(40);

		AIType = NPCID.BlazingWheel;
		SpawnModBiomes = [ModContent.GetInstance<MechBiome>().Type];
	}

	public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
	{
		bestiaryEntry.AddInfo(this, "");
	}

	public override bool PreAI()
	{
		if (NPC.ai[0] == 0f)
		{
			NPC.TargetClosest();
			NPC.directionY = 1;
			NPC.ai[0] = 1f;
		}

		const float Speed = 6.5f;

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

		Lighting.AddLight(NPC.Center, TorchID.Blue);
		Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.MinecartSpark, -NPC.velocity.X, -NPC.velocity.Y, Scale: 2);

		return false;
	}
}

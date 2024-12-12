using NPCUtils;
using PathOfTerraria.Common.NPCs.Components;
using PathOfTerraria.Common.NPCs.Effects;
using PathOfTerraria.Common.Subworlds.RavencrestContent;
using Terraria.GameContent.Bestiary;
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
		NPC.width = 30;
		NPC.width = 26;
		NPC.dontTakeDamage = true;
		NPC.dontTakeDamageFromHostiles = true;
		NPC.netAlways = true;
		NPC.netUpdate2 = true;
		NPC.Opacity = 0f;

		AnimationType = NPCID.Bird;
		AIType = NPCID.Bird;

		NPC.TryEnableComponent<NPCHitEffects>(c =>
		{
			c.AddDust(new(DustID.Ghost, 2));
			c.AddDust(new(DustID.Ghost, 6, NPCHitEffects.OnDeath));
		});
	}

	public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
	{
		bestiaryEntry.AddInfo(this, "Surface");
	}

	public override bool PreAI()
	{
		Lighting.AddLight(NPC.Center, new Vector3(0.15f));
		Vector2 entrancePosition = ModContent.GetInstance<RavencrestSystem>().EntrancePosition.ToVector2() * 16;

		if (NPC.DistanceSQ(entrancePosition) < 400 * 400 || Main.player[Player.FindClosest(NPC.Center, 1, 1)].DistanceSQ(NPC.Center) > 700 * 700)
		{
			if (!Collision.SolidCollision(NPC.position, NPC.width, NPC.height))
			{
				NPC.noTileCollide = false;
				NPC.ai[0] = 0;
			}

			NPC.velocity.X *= 0.9f;
		}
		else
		{
			NPC.noTileCollide = true;
		}

		if (Math.Abs(NPC.Center.X - entrancePosition.X) < 8)
		{
			NPC.Opacity -= 0.05f;
			NPC.velocity.X *= 0.5f;

			if (NPC.Opacity < 0.05f)
			{
				NPC.active = false;

				for (int i = 0; i < 15; ++i)
				{
					Dust.NewDust(NPC.position, NPC.width, NPC.height, Main.rand.NextBool() ? DustID.GemDiamond : DustID.Phantasmal);
				}
			}
		}
		else
		{
			NPC.Opacity = MathHelper.Lerp(NPC.Opacity, 0.8f, 0.05f);
		}

		if (NPC.direction != Math.Sign(entrancePosition.X - NPC.Center.X))
		{
			NPC.direction = Math.Sign(entrancePosition.X - NPC.Center.X);
			NPC.velocity.X *= 0.98f;
		}

		if (Math.Abs(LastDropX - NPC.Center.X) > 200 && Main.netMode != NetmodeID.MultiplayerClient)
		{
			Item.NewItem(NPC.GetSource_FromAI(), NPC.Center, ItemID.SilverCoin, 2);
			LastDropX = NPC.Center.X;
		}

		if (NPC.ai[0] == 1 && Main.rand.NextBool(15))
		{
			Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.GemDiamond, newColor: Color.Black);
		}

		NPC.velocity.X *= 1.05f;
		NPC.velocity.X = MathHelper.Clamp(NPC.velocity.X, -7, 7);

		float lerpVal = Collision.SolidCollision(NPC.position, NPC.width, NPC.height) ? -12 : NPC.velocity.Y;
		NPC.velocity.Y = MathHelper.Lerp(NPC.velocity.Y, lerpVal, 0.2f);

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

	public override bool NeedSaving()
	{
		return true;
	}
}
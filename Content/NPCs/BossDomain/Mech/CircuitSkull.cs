using NPCUtils;
using PathOfTerraria.Common.Subworlds.BossDomains;
using PathOfTerraria.Content.Scenes;
using ReLogic.Content;
using System.Collections.Generic;
using Terraria.GameContent.Bestiary;
using Terraria.ID;

namespace PathOfTerraria.Content.NPCs.BossDomain.Mech;

[AutoloadBanner]
internal class CircuitSkull : ModNPC
{
	private static Asset<Texture2D> Glow = null;

	private ref float Timer => ref NPC.ai[0];
	private ref float State => ref NPC.ai[1];

	public override void SetStaticDefaults()
	{
		Glow = ModContent.Request<Texture2D>(Texture + "_Glow");
	}

	public override void SetDefaults()
	{
		NPC.CloneDefaults(NPCID.Slimer);
		NPC.Opacity = 1;
		NPC.Size = new Vector2(36);
		NPC.aiStyle = -1;
		NPC.lifeMax = 150;
		NPC.defense = 50;
		NPC.damage = 20;
		NPC.HitSound = SoundID.NPCHit4;
		NPC.noGravity = true;
		NPC.knockBackResist = 0;
		NPC.scale = 1;

		SpawnModBiomes = [ModContent.GetInstance<MechBiome>().Type];
	}

	public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
	{
		bestiaryEntry.AddInfo(this, "");
	}

	public override bool? CanFallThroughPlatforms()
	{
		return true;
	}

	public override void AI()
	{
		Timer++;

		float lifeRatio = NPC.life / (float)NPC.lifeMax;

		if (State == 0)
		{
			if (Timer > 5 + 55 * lifeRatio)
			{
				State = 1;
				Timer = 0;

				NPC.TargetClosest();
				Player player = Main.player[NPC.target];
				float speed = 9 + 2 * (1 - lifeRatio);

				if (Collision.CanHit(NPC, player))
				{
					NPC.velocity = NPC.DirectionTo(player.Center).RotatedByRandom(0.2f) * speed;
				}
				else
				{
					NPC.velocity = DetermineRandomDirection().RotatedByRandom(0.2f) * speed;
				}
			}
		}
		else if (State == 1)
		{
			NPC.rotation += 0.14f * Math.Sign(NPC.velocity.X);

			if (NPC.collideX || NPC.collideY)
			{
				State = 0;
				Timer = 0;

				NPC.velocity = Vector2.Zero;
			}
		}
	}

	private Vector2 DetermineRandomDirection()
	{
		List<OpenFlags> flags = new(4);

		if (!Collision.SolidCollision(NPC.TopLeft - new Vector2(12, 6), NPC.width + 24, 6))
		{
			flags.Add(OpenFlags.Above);
		}

		if (!Collision.SolidCollision(NPC.BottomLeft - new Vector2(12, 0), NPC.width + 24, 6))
		{
			flags.Add(OpenFlags.Below);
		}

		if (!Collision.SolidCollision(NPC.TopLeft - new Vector2(6, 12), 6, NPC.height + 24))
		{
			flags.Add(OpenFlags.Left);
		}

		if (!Collision.SolidCollision(NPC.TopRight - new Vector2(0, 12), 6, NPC.height + 24))
		{
			flags.Add(OpenFlags.Right);
		}

		if (flags.Count == 0)
		{
			return Vector2.Zero;
		}

		Vector2 dir = Main.rand.Next([.. flags]) switch
		{
			OpenFlags.Above => new Vector2(0, -1),
			OpenFlags.Left => new Vector2(-1, 0),
			OpenFlags.Right => new Vector2(1, 0),
			_ => new Vector2(0, 1)
		};
		
		return dir;
	}

	public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
	{
		Vector2 position = NPC.Center - screenPos + new Vector2(0, 6);
		spriteBatch.Draw(Glow.Value, position, NPC.frame, Color.White, NPC.rotation, NPC.frame.Size() / 2f, 1f, SpriteEffects.None, 0);
	}
}

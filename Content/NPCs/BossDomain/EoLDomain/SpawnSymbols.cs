using PathOfTerraria.Common.Subworlds;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;

namespace PathOfTerraria.Content.NPCs.BossDomain.EoLDomain;

internal class SpawnSymbols : ModProjectile
{
	private ref float NPCTypeToSpawn => ref Projectile.ai[0];
	private ref float WaitTimer => ref Projectile.ai[1];

	public override void SetStaticDefaults()
	{
		Main.projFrames[Type] = 4;
	}

	public override void SetDefaults()
	{
		Projectile.frame = Main.rand.Next(4);
		Projectile.Size = new Vector2(30);
		Projectile.friendly = false;
		Projectile.hostile = false;
		Projectile.timeLeft = 240;
		Projectile.Opacity = 0f;
		Projectile.scale = 1f;
	}

	public override void AI()
	{
		if (WaitTimer > 0)
		{
			WaitTimer--;
			Projectile.timeLeft++;
			return;
		}

		if (Projectile.timeLeft == 60)
		{
			if (Main.netMode != NetmodeID.MultiplayerClient && NPCTypeToSpawn != 0)
			{
				if (NPCTypeToSpawn == NPCID.HallowBoss)
				{
					NPC.SpawnBoss((int)Projectile.Center.X, (int)Projectile.Center.Y, (int)NPCTypeToSpawn, Player.FindClosest(Projectile.position, 16, 16));
				}
				else
				{
					int npc = NPC.NewNPC(Projectile.GetSource_FromAI(), (int)Projectile.Center.X, (int)Projectile.Center.Y, (int)NPCTypeToSpawn);
					Main.npc[npc].GetGlobalNPC<ArenaEnemyNPC>().Arena = true;
				}
			}

			for (int i = 0; i < 20; ++i)
			{
				Vector2 vel = Main.rand.NextVector2Circular(8, 8);
				Vector2 pos = Projectile.position + new Vector2(Main.rand.Next(Projectile.width), Main.rand.Next(Projectile.height));
				Dust.NewDustPerfect(pos, DustID.AncientLight, vel, newColor: Color.Yellow);
			}

			SoundEngine.PlaySound(SoundID.Item4 with { Volume = 0.8f, PitchRange = (-0.4f, 0.8f) }, Projectile.Center);
		}

		if (Projectile.timeLeft > 60)
		{
			int timer = 180 - (Projectile.timeLeft - 60);
			Projectile.Opacity = timer / 180f;
			Projectile.scale = timer / 180f;
		}
		else
		{
			Projectile.Opacity = Projectile.timeLeft / 60f;
		}
	}

	public override Color? GetAlpha(Color lightColor)
	{
		return Color.White * Projectile.Opacity;
	}

	public override void PostDraw(Color lightColor)
	{
		Texture2D tex = TextureAssets.Projectile[Type].Value;
		int frameHeight = tex.Height / Main.projFrames[Type];
		Rectangle src = new(0, frameHeight * Projectile.frame, tex.Width, frameHeight);
		Vector2 basePos = Projectile.Center - Main.screenPosition;

		for (int i = 0; i < 4; ++i)
		{
			Vector2 position = basePos + Main.rand.NextVector2Circular(3, 3);
			Main.spriteBatch.Draw(tex, position, src, Color.White * 0.3f * Projectile.Opacity, 0, src.Size() / 2f, Projectile.scale * (1 + i * 0.1f), SpriteEffects.None, 0);
		}

		if (Projectile.timeLeft < 60 && Projectile.timeLeft >= 40)
		{
			Vector2 position = basePos + Main.rand.NextVector2Circular(6, 6);
			float addedScale = (1 - Projectile.timeLeft / 60f) * 16;
			float modOpacity = (Projectile.timeLeft - 40) / 20f;
			Main.spriteBatch.Draw(tex, position, src, Color.White * 0.3f * Projectile.Opacity * modOpacity, 0, src.Size() / 2f, Projectile.scale + addedScale, SpriteEffects.None, 0);
		}
	}
}

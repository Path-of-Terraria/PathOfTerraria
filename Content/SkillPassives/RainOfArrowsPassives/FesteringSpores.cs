using PathOfTerraria.Common;
using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Projectiles;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.Skills;
using PathOfTerraria.Content.Skills.Ranged;
using PathOfTerraria.Content.SkillTrees;
using ReLogic.Content;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;

namespace PathOfTerraria.Content.SkillPassives.RainOfArrowsPassives;

internal class FesteringSpores(SkillTree tree) : SkillPassive(tree)
{
	internal class FesteringSporesProj : SkillProjectile<RainOfArrows>
	{
		public const int AreaOfEffect = 80;

		public static Asset<Texture2D> Glow = null;

		private bool IsExploding
		{
			get => Projectile.ai[0] == 1;
			set => Projectile.ai[0] = value ? 1 : 0;
		}

		private ref float GlowTimer => ref Projectile.ai[1];
		private ref float SpeedTimer => ref Projectile.ai[2];

		private bool Init
		{
			get => Projectile.localAI[0] == 1;
			set => Projectile.localAI[0] = value ? 1 : 0;
		}

		public override void SetStaticDefaults()
		{
			Main.projFrames[Type] = 3;

			Glow = ModContent.Request<Texture2D>(Texture + "Glow");
		}

		public override void SetDefaults()
		{
			Projectile.friendly = true;
			Projectile.Size = new Vector2(16);
			Projectile.timeLeft = 2;
			Projectile.penetrate = -1;
			Projectile.aiStyle = -1;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.timeLeft = Main.rand.Next(120, 180);
			Projectile.Opacity = 0f;
		}

		public override bool? CanHitNPC(NPC target)
		{
			return IsExploding ? null : false;
		}

		public override void AI()
		{
			if (!Init)
			{
				Projectile.frame = Main.rand.Next(3);
				Init = true;
				SpeedTimer = Main.rand.NextFloat(-0.003f, 0);
			}

			Projectile.scale = Projectile.Opacity = MathHelper.Lerp(Projectile.Opacity, 1f, 0.05f);
			GlowTimer += SpeedTimer;
			SpeedTimer += 0.003f;

			if (Projectile.timeLeft == 1)
			{
				IsExploding = true;

				Player player = Main.player[Projectile.owner];
				float aoE = AreaOfEffect * (1 + player.GetPassiveStrength<RainOfArrowsTree, FungalSpread>() * 0.1f);
				Projectile.Resize((int)aoE, (int)aoE);
				Projectile.Damage();
				Projectile.Kill();

				if (!Main.dedServ)
				{
					SoundEngine.PlaySound(SoundID.Item10, Projectile.Center);

					for (int i = 0; i < 4; ++i)
					{
						Vector2 vel = Main.rand.NextVector2CircularEdge(4, 4) * Main.rand.NextFloat(0.5f, 1f);
						Gore.NewGore(Projectile.GetSource_Death(), Projectile.Center, vel, GoreID.Smoke1 + Main.rand.Next(3));
					}

					for (int i = 0; i < 20; ++i)
					{
						Vector2 v = Main.rand.NextVector2CircularEdge(6, 6) * Main.rand.NextFloat(0.5f, 1f);
						Dust.NewDust(Projectile.Center, 1, 1, DustID.Grass, v.X, v.Y, Scale: Main.rand.NextFloat(0.5f, 1.2f));
					}
				}
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D tex = TextureAssets.Projectile[Type].Value;
			var src = new Rectangle(0, Projectile.frame * 24, tex.Width, 22);
			Vector2 origin = new(src.Width / 2, src.Height / 2);
			Point16 tilePos = Projectile.Center.ToTileCoordinates16();
			float push = Main.instance.TilesRenderer.GetWindGridPush(tilePos.X, tilePos.Y, 30, 4f);
			Main.spriteBatch.Draw(tex, Projectile.Bottom - Main.screenPosition, src, lightColor * Projectile.Opacity, push, origin, Projectile.scale, SpriteEffects.None, 0);
			Color color = Color.White * (MathF.Sin(GlowTimer) * 0.25f + 0.25f) * Projectile.Opacity;
			Main.spriteBatch.Draw(Glow.Value, Projectile.Bottom - Main.screenPosition, src, color, push, origin, Projectile.scale, SpriteEffects.None, 0);
			return false;
		}
	}
}
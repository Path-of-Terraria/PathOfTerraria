using NPCUtils;
using PathOfTerraria.Common.NPCs;
using PathOfTerraria.Common.NPCs.Components;
using PathOfTerraria.Common.NPCs.Effects;
using ReLogic.Content;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.Drawing;
using Terraria.ID;

namespace PathOfTerraria.Content.NPCs.BossDomain.EoLDomain;

[AutoloadBanner]
internal class Prismatism : ModNPC
{
	private static Asset<Texture2D> CapeTex = null;
	private static Asset<Texture2D> ShineTex = null;

	private Player Target => Main.player[NPC.target];

	private ref float Timer => ref NPC.ai[0];
	private ref float RotationSpeed => ref NPC.ai[1];

	private Vector2 TeleportTarget
	{
		get => new(NPC.ai[2], NPC.ai[3]);
		set => (NPC.ai[2], NPC.ai[3]) = (value.X, value.Y);
	}

	private ref float Repeats => ref NPC.localAI[0];

	public override void SetStaticDefaults()
	{
		CapeTex = ModContent.Request<Texture2D>(Texture + "_Cape");
		ShineTex = ModContent.Request<Texture2D>(Texture + "_Shine");
	}

	public override void SetDefaults()
	{
		NPC.Size = new Vector2(42);
		NPC.damage = 0;
		NPC.aiStyle = -1;
		NPC.defense = 50;
		NPC.lifeMax = 1500;
		NPC.noGravity = true;
		NPC.knockBackResist = 0f;
		NPC.noTileCollide = true;

		NPC.TryEnableComponent<NPCHitEffects>(c =>
		{
			c.AddDust(new(DustID.BubbleBurst_White, 4));
			c.AddDust(new(DustID.BubbleBurst_White, 16, NPCHitEffects.OnDeath));

			for (int i = 0; i < 3; ++i)
			{
				c.AddGore(new NPCHitEffects.GoreSpawnParameters($"{PoTMod.ModName}/{Name}_" + i, 1, NPCHitEffects.OnDeath));
			}
		});
	}

	public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
	{
		bestiaryEntry.AddInfo(this, "TheHallow");
	}

	public override void AI()
	{
		const int TimeToWait = 240;

		NPC.TargetClosest(false);

		Timer++;

		RotationSpeed = MathHelper.Lerp(RotationSpeed, Timer > TimeToWait ? 0 : 0.3f, Timer > TimeToWait ? 0.02f : 0.02f);
		NPC.rotation += RotationSpeed;

		if (Timer >= TimeToWait + 90 && Timer <= TimeToWait + 100)
		{
			TeleportTarget = Repeats == 2 ? Target.Center + Target.velocity * 30 : Target.Center;

			if (Timer == TimeToWait + 92)
			{
				var settings = new ParticleOrchestraSettings
				{
					PositionInWorld = NPC.Center,
					MovementVector = NPC.velocity + new Vector2(0, -18).RotatedByRandom(0.2f) * Main.rand.NextFloat(0.6f, 1.2f),
				};

				ParticleOrchestrator.RequestParticleSpawn(clientOnly: false, ParticleOrchestraType.RainbowRodHit, settings);
			}
		}
		else if (Timer >= TimeToWait + 130)
		{
			SpawnVFX();

			int repeats = (int)(NPC.Distance(TeleportTarget) / 120f);

			for (int i = 0; i < repeats; ++i)
			{
				var position = Vector2.Lerp(NPC.Center, TeleportTarget, (i + 1f) / (repeats + 1f));
				int damage = ModeUtils.ProjectileDamage(80);
				Projectile.NewProjectile(NPC.GetSource_FromAI(), position, Vector2.Zero, ModContent.ProjectileType<PrismatismAfterimage>(), damage, 0, Main.myPlayer);
			}

			NPC.Center = TeleportTarget;
			Repeats++;

			if (Repeats >= 3)
			{
				Timer = 0;
				Repeats = 0;
			}
			else
			{
				Timer = TimeToWait + 89;
			}

			SpawnVFX();
		}

		void SpawnVFX()
		{
			for (int i = 0; i < 6; ++i)
			{
				SpawnDust(NPC);
			}

			for (int i = 0; i < 2; ++i)
			{
				SpawnSparkles();
			}
		}
	}

	internal static void SpawnDust(Entity entity, float speedMul = 1f)
	{
		int dust = Dust.NewDust(entity.position, entity.width, entity.height, DustID.RainbowTorch, 0f, 0f, 100, Main.DiscoColor, 2.5f);
		Main.dust[dust].velocity = Main.rand.NextVector2CircularEdge(6, 6) * Main.rand.NextFloat(0.6f, 1.2f) * speedMul;
		Main.dust[dust].noGravity = true;
	}

	private void SpawnSparkles()
	{
		var settings = new ParticleOrchestraSettings
		{
			PositionInWorld = NPC.Center,
			MovementVector = NPC.velocity + Main.rand.NextVector2CircularEdge(18, 18) * Main.rand.NextFloat(0.6f, 1.2f),
		};

		ParticleOrchestrator.RequestParticleSpawn(clientOnly: false, ParticleOrchestraType.RainbowRodHit, settings);
	}

	public override void FindFrame(int frameHeight)
	{
		NPC.frameCounter++;
	}

	public override Color? GetAlpha(Color drawColor)
	{
		return NPC.color == Color.Transparent ? Color.White : NPC.color;
	}

	public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
	{
		if (NPC.IsABestiaryIconDummy)
		{
			NPC.rotation += 0.05f;
		}

		float rot = NPC.velocity.X * 0.2f;
		Vector2 basePosition = NPC.Center - screenPos;
		Main.EntitySpriteDraw(ShineTex.Value, basePosition + new Vector2(0, 6), null, NPC.GetAlpha(Color.White), rot, ShineTex.Size() / 2f, 1f, SpriteEffects.None, 0);

		Vector2 position = basePosition + new Vector2(0, 14);
		Main.EntitySpriteDraw(CapeTex.Value, position, null, NPC.GetAlpha(Color.White), rot, CapeTex.Size() / 2f, 1f, SpriteEffects.None, 0);
	}
}

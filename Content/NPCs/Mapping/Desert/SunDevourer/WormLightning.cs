using NPCUtils;
using PathOfTerraria.Common.NPCs;
using PathOfTerraria.Common.NPCs.Worms;
using PathOfTerraria.Common.Subworlds.MappingAreas;
using PathOfTerraria.Content.NPCs.Mapping.Desert.SunDevourer.Projectiles;
using ReLogic.Content;
using SubworldLibrary;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ID;

namespace PathOfTerraria.Content.NPCs.Mapping.Desert.SunDevourer;

[AutoloadBanner]
internal class WormLightning : ModNPC
{
	internal class WormLightning_Body : WormSegment
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();

			NPCID.Sets.TrailCacheLength[Type] = 2;
			NPCID.Sets.TrailingMode[Type] = 1;
		}

		public override void Defaults()
		{
			NPC.Size = new Vector2(22);
			NPC.damage = 60;
		}

		public override Color? GetAlpha(Color drawColor)
		{
			return Color.White;
		}

		public override void AI()
		{
			base.AI();

			if (!Parent.active)
			{
				NPC.active = false;

				if (Main.netMode != NetmodeID.MultiplayerClient)
				{
					Vector2 vel = NPC.oldPos[0] - NPC.oldPos[1];
					int type = ModContent.ProjectileType<WormLightningDeathFX>();
					Projectile.NewProjectile(NPC.GetSource_Death(), NPC.Center, vel, type, 0, 0, Main.myPlayer, NPC.type, 0, NPC.rotation);
				}
			}

			if (!Main.rand.NextBool(55))
			{
				return;
			}

			var dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Electric);
			dust.velocity *= 2f;
			dust.noGravity = true;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			DrawSelf(NPC, spriteBatch, screenPos);
			return false;
		}

		public override void HitEffect(NPC.HitInfo hit)
		{
			int reps = NPC.life < 0 ? 8 : 2;

			for (int i = 0; i < reps; ++i)
			{
				Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Electric, NPC.velocity.X, NPC.velocity.X);
			}
		}

		public override bool? CanBeHitByProjectile(Projectile projectile)
		{
			return projectile.type != ProjectileID.SandBallFalling ? null : false;
		}
	}

	internal class WormLightning_Tail : WormLightning_Body
	{
	}

	private ref float State => ref NPC.ai[0];
	private ref float MaxJerkTimer => ref NPC.ai[1];
	private ref float JerkTimer => ref NPC.ai[2];

	private bool _spawnedSegments = false;

	public override void SetDefaults()
	{
		NPC.Size = new Vector2(22);
		NPC.lifeMax = 100;
		NPC.damage = 60;
		NPC.aiStyle = -1;
		NPC.noGravity = true;
		NPC.knockBackResist = 0;
		NPC.noTileCollide = true;
	}

	public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
	{
		NPC.lifeMax = ModeUtils.ByMode(120, 200, 300, 600);
	}

	public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
	{
		bestiaryEntry.AddInfo(this, "Desert");
	}

	public override bool CheckDead()
	{
		SpawnDeathFX();
		return true;
	}

	public override void AI()
	{
		const float MaxSpeed = 13;

		if (Main.rand.NextBool(55))
		{
			var dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Electric);
			dust.velocity *= 2f;
			dust.noGravity = true;
		}

		if (!_spawnedSegments && Main.netMode != NetmodeID.MultiplayerClient)
		{
			WormSegment.SpawnWhole<WormLightning_Body, WormLightning_Tail>(NPC.GetSource_FromAI(), NPC, 24, 12);
			_spawnedSegments = true;
		}

		if (SubworldSystem.Current is not DesertArea || !NPC.AnyNPCs(ModContent.NPCType<SunDevourerNPC>()))
		{
			NPC.active = false;
			SpawnDeathFX();
			return;
		}

		NPC.rotation = NPC.velocity.ToRotation();

		if (State == 0)
		{
			NPC.dontTakeDamage = false;
			NPC.TargetClosest();
			Player target = Main.player[NPC.target];

			NPC.velocity += NPC.DirectionTo(target.Center) * 0.4f;

			if (NPC.velocity.LengthSquared() > MaxSpeed * MaxSpeed)
			{
				NPC.velocity = Vector2.Normalize(NPC.velocity) * MaxSpeed;
			}

			if (JerkTimer++ > MaxJerkTimer)
			{
				JerkTimer = 0;
				MaxJerkTimer = Main.rand.NextFloat(25, 90);
				NPC.velocity = NPC.velocity.RotatedBy(Main.rand.NextFloat(0.2f, 0.4f) * (Main.rand.NextBool() ? 1 : -1));
				NPC.netUpdate = true;
			}
		}
	}

	private void SpawnDeathFX()
	{
		if (Main.netMode != NetmodeID.MultiplayerClient)
		{
			int type = ModContent.ProjectileType<WormLightningDeathFX>();
			Projectile.NewProjectile(NPC.GetSource_Death(), NPC.Center, NPC.velocity, type, 0, 0, Main.myPlayer, NPC.type, 0, NPC.rotation);
		}
	}

	public override void HitEffect(NPC.HitInfo hit)
	{
		int reps = NPC.life < 0 ? 12 : 3;

		for (int i = 0; i < reps; ++i)
		{
			Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Electric, NPC.velocity.X, NPC.velocity.X);
		}
	}

	public override bool? CanBeHitByProjectile(Projectile projectile)
	{
		return projectile.type != ProjectileID.SandBallFalling ? null : false;
	}

	public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
	{
		DrawSelf(NPC, spriteBatch, screenPos);
		return false;
	}

	public static void DrawSelf(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos)
	{
		Texture2D value = TextureAssets.Npc[npc.type].Value;

		for (int i = 0; i < 3; ++i)
		{
			float scale = 0.6f + (i + 1) * 0.3f;
			float lerpFactor = MathF.Pow(MathF.Sin((float)Main.timeForVisualEffects * 0.1f + i + npc.whoAmI * 2.5f), 2);
			Color color = GetElectricColor(lerpFactor) * (1 - i * 0.4f) * npc.Opacity;
			int frame = (int)((Main.timeForVisualEffects * 0.25f + i) % 3);
			Rectangle src = new(0, 26 * frame, 22, 24);
			Vector2 position = npc.Center - screenPos + Main.rand.NextVector2Circular(i * 3, i * 3);
			spriteBatch.Draw(value, position, src, color, npc.rotation, src.Size() * new Vector2(0.5f, 0.5f), scale, SpriteEffects.None, 0);
		}
	}

	private static Color GetElectricColor(float lerpFactor)
	{
		return Color.Lerp(Color.Lerp(new Color(160, 255, 255), Color.White, lerpFactor), Color.Lerp(Color.White, Color.Yellow with { B = 190 }, lerpFactor), lerpFactor);
	}
}
using NPCUtils;
using PathOfTerraria.Common.NPCs;
using PathOfTerraria.Common.NPCs.Components;
using PathOfTerraria.Common.NPCs.Effects;
using ReLogic.Content;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ID;

namespace PathOfTerraria.Content.NPCs.Mapping.Forest;

internal class Ent : ModNPC
{
	public const int SwingWaitTimer = 60;

	private static Asset<Texture2D> Glow = null;

	public enum AIState
	{
		Chase,
		SwingProj,
		Slam
	}

	public Player Target => Main.player[NPC.target];

	private bool LastOnGround
	{
		get => NPC.ai[0] == 1f;
		set => NPC.ai[0] = value ? 1f : 0f;
	}

	private AIState State 
	{
		get => (AIState)NPC.ai[1];
		set => NPC.ai[1] = (float)value;
	}

	private ref float Timer => ref NPC.ai[2];
	private ref float ProjThrowTimer => ref NPC.ai[3];
	private ref float MiragePower => ref NPC.localAI[0];

	public override void SetStaticDefaults()
	{
		Glow = ModContent.Request<Texture2D>(Texture + "_Glow");
		Main.npcFrameCount[Type] = 19;

		var drawModifiers = new NPCID.Sets.NPCBestiaryDrawModifiers()
		{
			Velocity = 2f
		};
		NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);
	}

	public override void SetDefaults()
	{
		NPC.Size = new Vector2(50, 124);
		NPC.aiStyle = -1;
		NPC.lifeMax = 450;
		NPC.defense = 35;
		NPC.damage = 65;
		NPC.knockBackResist = 0.3f;
	}

	public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
	{
		NPC.lifeMax = ModeUtils.ByMode(450, 600, 900);
		NPC.damage = ModeUtils.ByMode(65, 130, 195);
		NPC.knockBackResist = ModeUtils.ByMode(0.3f, 0.27f, 0.23f);
	}

	public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
	{
		bestiaryEntry.AddInfo(this, "Surface");
	}

	public override void AI()
	{
		if (!LastOnGround && NPC.velocity.Y == 0 && NPC.oldVelocity.Y > 3)
		{
			for (int i = 0; i < 6; ++i)
			{
				Dust.NewDust(NPC.BottomLeft, NPC.width, 4, DustID.CorruptionThorns, Main.rand.NextFloat(-3, 3), Main.rand.NextFloat(3, 6));
			}

			Collision.HitTiles(NPC.BottomLeft - new Vector2(32, 0), NPC.velocity, NPC.width + 64, 6);
			SoundEngine.PlaySound(SoundID.DD2_BetsysWrathImpact, NPC.Center);
		}

		LastOnGround = NPC.velocity.Y == 0;
		MiragePower = MathHelper.Lerp(MiragePower, State == AIState.SwingProj ? 1 : 0, 0.1f);

		if (Main.rand.NextBool(40))
		{
			SpawnDust(true);
		}

		if (State == AIState.Chase)
		{
			MovementBehaviour();
		}
		else if (State == AIState.SwingProj)
		{
			SwingBehaviour();
		}
	}

	private void SwingBehaviour()
	{
		NPC.TargetClosest(true);
		NPC.spriteDirection = -NPC.direction;

		Timer++;
		NPC.velocity.X *= 0.9f;

		if (Timer < SwingWaitTimer && Main.rand.NextBool(4))
		{
			SpawnDust(false);
		}

		if (Timer >= 10 + SwingWaitTimer && Timer <= 12 + SwingWaitTimer)
		{
			Vector2 pos = NPC.Center - new Vector2(60 * NPC.spriteDirection, 0);
			Vector2 vel = pos.DirectionTo(Target.Center).RotatedByRandom(0.3f) * 12;
			Projectile.NewProjectile(NPC.GetSource_FromAI(), pos, vel, ModContent.ProjectileType<EntThorn>(), 30, 2, Main.myPlayer);
		}
		else if (Timer >= 35 + SwingWaitTimer)
		{
			Timer = 0;
			State = AIState.Chase;
		}
	}

	private void SpawnDust(bool noLight)
	{
		Vector2 vel = new Vector2(0, -Main.rand.NextFloat(4, 7)).RotatedByRandom(0.1f);
		Vector2 position = NPC.Center - new Vector2((6 + Main.rand.Next(-2, 4)) * NPC.spriteDirection, Main.rand.Next(-4, 4));
		var dust = Dust.NewDustPerfect(position, ModContent.DustType<EntDust>(), vel, 0, default, Main.rand.NextFloat(1, 1.3f));
		dust.noLight = noLight;
	}

	private void MovementBehaviour()
	{
		NPC.TargetClosest(true);

		if (Math.Abs(NPC.velocity.X) > 0.4f)
		{
			NPC.spriteDirection = -NPC.direction;
		}

		Timer++;

		if (Timer > ProjThrowTimer)
		{
			if (Collision.CanHit(NPC, Target))
			{
				State = AIState.SwingProj;
				Timer = 0;
				ProjThrowTimer = Main.rand.Next(480, 600);

				NPC.netUpdate = true;
			}
			else
			{
				ProjThrowTimer += 120;
			}
		}

		if (Math.Abs(Target.Center.X - NPC.Center.X) > 50)
		{
			float targetXVel = Target.Center.X > NPC.Center.X ? 3 : -3;

			if (NPC.HasBuff(BuffID.Confused))
			{
				targetXVel *= -0.8f;
			}

			NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, targetXVel, 0.03f);
		}
		else
		{
			NPC.velocity.X *= 0.9f;
		}

		if (ShouldJump())
		{
			NPC.velocity.Y = -9;

			for (int i = 0; i < 6; ++i)
			{
				Dust.NewDust(NPC.BottomLeft, NPC.width, 4, DustID.CorruptionThorns, Main.rand.NextFloat(-3, 3), Main.rand.NextFloat(-6, -3));
			}
		}

		Collision.StepUp(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.stepSpeed, ref NPC.gfxOffY);
	}

	private bool ShouldJump()
	{
		int d = NPC.Center.X < Target.Center.X ? 1 : -1;
		bool wall = d == -1 ? Collision.SolidCollision(NPC.TopLeft - new Vector2(6, 0), 6, NPC.height - 18) : Collision.SolidCollision(NPC.TopRight, 6, NPC.height - 18);
		bool gap = d == -1 ? !Collision.SolidCollision(NPC.BottomLeft - new Vector2(16, 0), 14, 36) : !Collision.SolidCollision(NPC.BottomRight + new Vector2(2, 0), 14, 36);
		bool underPlayer = Math.Abs(NPC.Center.X - Target.Center.X) < 80 && NPC.Center.Y > Target.Center.Y;

		if (underPlayer)
		{
			ProjThrowTimer -= 2f;
		}

		return (wall || gap) && NPC.velocity.Y == 0;
	}

	public override void HitEffect(NPC.HitInfo hit)
	{
		int dustCount = NPC.life < 0 ? 20 : 5;

		for (int i = 0; i < dustCount; ++i)
		{
			int type = Main.rand.NextBool(10) ? ModContent.DustType<EntDust>() : DustID.CorruptionThorns;
			Dust.NewDust(NPC.position, NPC.width, NPC.height, type, NPC.velocity.X, NPC.velocity.Y);
		}
	}

	public override void FindFrame(int frameHeight)
	{
		if (State == AIState.Chase || NPC.IsABestiaryIconDummy)
		{
			if (NPC.velocity.Y == 0 || NPC.IsABestiaryIconDummy)
			{
				if (Math.Abs(NPC.velocity.X) <= 0.4f)
				{
					NPC.frame.Y = 0;
				}
				else
				{
					NPC.frameCounter += Math.Abs(NPC.velocity.X) / 4f;
					NPC.frame.Y = frameHeight * (int)(NPC.frameCounter / 4f % 7) + frameHeight;
				}
			}
			else
			{
				NPC.frameCounter = 0;
				NPC.frame.Y = frameHeight;
			}
		}
		else if (State == AIState.SwingProj)
		{
			float adjTimer = MathF.Max(Timer - SwingWaitTimer, 0);

			if (adjTimer != 0)
			{
				NPC.frame.Y = frameHeight * (int)(8 + adjTimer / 5f);
			}
			else
			{
				NPC.frame.Y = 0;
			}
		}
	}

	public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
	{
		Texture2D tex = TextureAssets.Npc[Type].Value;
		SpriteEffects effect = NPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
		Vector2 position = NPC.Center - screenPos - new Vector2(16 * NPC.spriteDirection, -NPC.gfxOffY);
		
		Main.EntitySpriteDraw(tex, position, NPC.frame, drawColor, NPC.rotation, NPC.frame.Size() / 2f, 1f, effect);
		Main.EntitySpriteDraw(Glow.Value, position, NPC.frame, Color.White, NPC.rotation, NPC.frame.Size() / 2f, 1f, effect);

		if (MiragePower > 0)
		{
			for (int i = 0; i < 10; ++i)
			{
				Vector2 pos = position + Main.rand.NextVector2Circular(MiragePower * 6, MiragePower * 6);
				Main.EntitySpriteDraw(Glow.Value, pos, NPC.frame, Color.White * 0.3f, NPC.rotation, NPC.frame.Size() / 2f, 1f, effect);
			}
		}

		return false;
	}

	public class EntThorn : ModProjectile
	{
		private static Asset<Texture2D> ProjGlow = null;

		public override void SetStaticDefaults()
		{
			ProjGlow = ModContent.Request<Texture2D>(Texture + "_Glow");
		}

		public override void SetDefaults()
		{
			Projectile.aiStyle = -1;
			Projectile.timeLeft = 10 * 60;
			Projectile.hostile = true;
			Projectile.friendly = false;
			Projectile.Size = new(18);
		}

		public override void AI()
		{
			Projectile.rotation = Projectile.velocity.ToRotation();
			Projectile.velocity *= 0.999f;

			if (Main.rand.NextBool(6))
			{
				Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.WoodFurniture, Projectile.velocity.X, Projectile.velocity.Y);
			}
		}

		public override void OnKill(int timeLeft)
		{
			for (int i = 0; i < 12; ++i)
			{
				Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.WoodFurniture, Projectile.velocity.X, Projectile.velocity.Y);
			}
		}

		public override void PostDraw(Color lightColor)
		{
			Vector2 position = Projectile.Center - Main.screenPosition;
			Main.EntitySpriteDraw(ProjGlow.Value, position, null, Color.White, Projectile.rotation, ProjGlow.Size() / 2f, 1f, SpriteEffects.None, 0);
		}
	}

	public class EntDust : ModDust
	{
		public override void OnSpawn(Dust dust)
		{
			dust.noGravity = true;
			dust.frame = new Rectangle(0, 8 * Main.rand.Next(3), 6, 6);
		}

		public override bool Update(Dust dust)
		{
			dust.velocity *= 0.99f;
			dust.alpha += 2;
			dust.rotation += 0.2f;

			if (dust.alpha >= 254)
			{
				dust.active = false;
			}

			dust.position += dust.velocity;

			if (!dust.noLight)
			{
				Lighting.AddLight(dust.position, new Vector3(251, 242, 54) / 400f * (1 - dust.alpha / 255f));
			}

			return false;
		}
	}
}

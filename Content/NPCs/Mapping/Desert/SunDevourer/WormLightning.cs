using NPCUtils;
using PathOfTerraria.Common.NPCs;
using PathOfTerraria.Common.NPCs.Worms;
using ReLogic.Content;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ID;

namespace PathOfTerraria.Content.NPCs.Mapping.Desert.SunDevourer;

[AutoloadBanner]
internal class WormLightning : ModNPC
{
	internal class WormLightning_Body : WormSegment
	{
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

		public override bool? CanBeHitByProjectile(Projectile projectile)
		{
			return projectile.type != ProjectileID.SandBallFalling ? null : false;
		}
	}

	internal class WormLightning_Tail : WormLightning_Body
	{
	}

	internal static Asset<Texture2D>[] Textures;

	private ref float State => ref NPC.ai[0];

	private Vector2 TargetGlass
	{
		get => new(NPC.ai[1], NPC.ai[2]);
		set => (NPC.ai[1], NPC.ai[2]) = (value.X, value.Y);
	}

	private ref float GlassBroken => ref NPC.ai[3];

	private bool _spawnedSegments = false;

	public override void SetStaticDefaults()
	{
		Textures = 
		[
			ModContent.Request<Texture2D>(Texture),
			ModContent.Request<Texture2D>(Texture + "_Body"),
			ModContent.Request<Texture2D>(Texture + "_Tail")
		];
	}

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
		NPC.lifeMax = ModeUtils.ByMode(60, 100, 150, 250);
	}

	public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
	{
		bestiaryEntry.AddInfo(this, "Desert");
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

			// Aliasing locally as both of these are already used for TargetGlass, but State == 0 doesn't use TargetGlass
			ref float maxJerkTimer = ref NPC.ai[1];
			ref float jerkTime = ref GlassBroken;

			if (jerkTime++ > maxJerkTimer)
			{
				jerkTime = 0;
				maxJerkTimer = Main.rand.NextFloat(25, 90);
				NPC.velocity = NPC.velocity.RotatedBy(Main.rand.NextFloat(0.2f, 0.4f) * (Main.rand.NextBool() ? 1 : -1));
				NPC.netUpdate = true;
			}
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

	private static void DrawSelf(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos)
	{
		Texture2D value = TextureAssets.Npc[npc.type].Value;

		for (int i = 0; i < 3; ++i)
		{
			float scale = 0.6f + (i + 1) * 0.3f;
			float lerpFactor = MathF.Pow(MathF.Sin((float)Main.timeForVisualEffects * 0.1f + i + npc.whoAmI * 2.5f), 2);
			Color color = GetElectricColor(lerpFactor) * (1 - i * 0.4f);
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
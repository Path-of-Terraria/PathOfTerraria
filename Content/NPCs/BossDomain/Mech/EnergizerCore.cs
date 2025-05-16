using NPCUtils;
using PathOfTerraria.Common.NPCs.Components;
using PathOfTerraria.Common.NPCs.Effects;
using PathOfTerraria.Content.Scenes;
using ReLogic.Content;
using Terraria.GameContent.Bestiary;
using Terraria.ID;

namespace PathOfTerraria.Content.NPCs.BossDomain.Mech;

[AutoloadBanner]
internal class EnergizerCore : ModNPC
{
	private static Asset<Texture2D> Glow = null;

	private ref float Timer => ref NPC.ai[0];

	public override void SetStaticDefaults()
	{
		Glow = ModContent.Request<Texture2D>(Texture + "_Glow");

		Main.npcFrameCount[Type] = 7;
	}

	public override void SetDefaults()
	{
		NPC.CloneDefaults(NPCID.Slimer);
		NPC.Opacity = 1;
		NPC.Size = new Vector2(52);
		NPC.aiStyle = -1;
		NPC.lifeMax = 650;
		NPC.defense = 40;
		NPC.HitSound = SoundID.NPCHit4;
		NPC.noGravity = true;
		NPC.knockBackResist = 0;
		NPC.scale = 1;
		NPC.value = Item.buyPrice(0, 0, 5);
		NPC.damage = 0;

		SpawnModBiomes = [ModContent.GetInstance<MechBiome>().Type];

		NPC.TryEnableComponent<NPCHitEffects>(c =>
		{
			c.AddDust(new(DustID.MinecartSpark, 3, null, static x => x.scale = 10));
			c.AddDust(new(DustID.MinecartSpark, 20, NPCHitEffects.OnDeath, static x => x.scale = 10));

			c.AddDust(new(DustID.RedTorch, 4, null, static x =>
			{
				x.noGravity = true;
				x.velocity = Main.rand.NextVector2CircularEdge(8, 8) * Main.rand.NextFloat(0.8f, 1f);
				x.scale = Main.rand.NextFloat(1.5f, 2f);
			}));

			c.AddDust(new(DustID.RedTorch, 20, NPCHitEffects.OnDeath, static x =>
			{
				x.noGravity = true;
				x.velocity = Main.rand.NextVector2CircularEdge(8, 8) * Main.rand.NextFloat(0.8f, 1f);
				x.scale = Main.rand.NextFloat(1.5f, 2f);
			}));

			c.AddGore(new NPCHitEffects.GoreSpawnParameters($"{PoTMod.ModName}/{Name}_0", 1, NPCHitEffects.OnDeath));
		});
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

		if (Main.netMode != NetmodeID.MultiplayerClient && Timer % 30 == 0)
		{
			foreach (NPC npc in Main.ActiveNPCs)
			{
				if (npc.type != Type && npc.life < npc.lifeMax && npc.DistanceSQ(NPC.Center) < 400 * 400)
				{
					int amount = (int)Math.Min(npc.lifeMax - npc.life, npc.lifeMax * 0.05f);

					npc.HealEffect(amount);
					npc.life += amount;
					npc.netUpdate = true;
				}
			}
		}

		if (Main.netMode != NetmodeID.Server)
		{
			foreach (NPC npc in Main.ActiveNPCs)
			{
				if (npc.type != Type && npc.life < npc.lifeMax && npc.DistanceSQ(NPC.Center) < 400 * 400)
				{
					int max = 14;

					for (int i = 0; i < max; ++i)
					{
						Dust.NewDustPerfect(LerpBezier(NPC, npc, i / (max - 1f)), DustID.RedTorch, Vector2.Zero).noGravity = true;
					}
				}
			}
		}

		return;

		static Vector2 LerpBezier(NPC me, NPC other, float factor)
		{
			Vector2 mid = (me.Center + other.Center) / 2 + new Vector2(0, 80);
			return Vector2.Lerp(Vector2.Lerp(me.Center, mid, factor), Vector2.Lerp(mid, other.Center, factor), factor);
		}
	}

	public override void FindFrame(int frameHeight)
	{
		NPC.frame.Y = (int)(NPC.frameCounter++ / 5f % Main.npcFrameCount[Type]) * frameHeight;
	}

	public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
	{
		Vector2 position = NPC.Center - screenPos + new Vector2(0, 3);
		spriteBatch.Draw(Glow.Value, position, NPC.frame, Color.White, NPC.rotation, NPC.frame.Size() / 2f, 1f, SpriteEffects.None, 0);
	}
}

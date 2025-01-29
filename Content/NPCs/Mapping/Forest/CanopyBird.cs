using NPCUtils;
using PathOfTerraria.Common.NPCs;
using PathOfTerraria.Common.NPCs.Components;
using PathOfTerraria.Common.NPCs.Effects;
using PathOfTerraria.Common.World.Generation;
using ReLogic.Content;
using Steamworks;
using System.Collections.Generic;
using System.IO;
using Terraria.GameContent.Bestiary;
using Terraria.ID;

namespace PathOfTerraria.Content.NPCs.Mapping.Forest;

[AutoloadBanner]
internal class CanopyBird : ModNPC
{
	public const int SplineCount = 30;

	private static Asset<Texture2D> Glow = null;

	public enum AIState
	{
		Chase,
		SwingProj,
		Slam
	}

	public Player Target => Main.player[NPC.target];

	private ref float Timer => ref NPC.ai[0];
	private ref float SplineFactor => ref NPC.ai[1];

	private Vector2[] spline = null;
	private Vector2 spawn = Vector2.Zero;

	public override void SetStaticDefaults()
	{
		Glow = ModContent.Request<Texture2D>(Texture + "_Glow");
		Main.npcFrameCount[Type] = 5;

		var drawModifiers = new NPCID.Sets.NPCBestiaryDrawModifiers()
		{
			Velocity = 2f
		};
		NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);
	}

	public override void SetDefaults()
	{
		NPC.Size = new Vector2(40, 36);
		NPC.aiStyle = -1;
		NPC.lifeMax = 300;
		NPC.defense = 0;
		NPC.damage = 50;
		NPC.knockBackResist = 0;
		NPC.HitSound = SoundID.NPCHit30;
		NPC.noGravity = true;
		NPC.noTileCollide = true;

		NPC.TryEnableComponent<NPCHitEffects>(
			c =>
			{
				c.AddGore(new NPCHitEffects.GoreSpawnParameters($"{PoTMod.ModName}/{Name}_0", 1, NPCHitEffects.OnDeath));
				c.AddGore(new NPCHitEffects.GoreSpawnParameters($"{PoTMod.ModName}/{Name}_1", 2, NPCHitEffects.OnDeath));
				c.AddGore(new NPCHitEffects.GoreSpawnParameters($"{PoTMod.ModName}/{Name}_2", 1, NPCHitEffects.OnDeath));

				c.AddDust(new NPCHitEffects.DustSpawnParameters(DustID.CorruptionThorns, 5));
				c.AddDust(new NPCHitEffects.DustSpawnParameters(ModContent.DustType<EntDust>(), 1));
				c.AddDust(new NPCHitEffects.DustSpawnParameters(DustID.CorruptionThorns, 20, NPCHitEffects.OnDeath));
				c.AddDust(new NPCHitEffects.DustSpawnParameters(ModContent.DustType<EntDust>(), 10, NPCHitEffects.OnDeath));
			}
		);
	}

	public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
	{
		NPC.lifeMax = ModeUtils.ByMode(250, 300, 400);
		NPC.damage = ModeUtils.ByMode(50, 80, 120);
		NPC.defense = ModeUtils.ByMode(0, 0, 10, 20);
	}

	public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
	{
		bestiaryEntry.AddInfo(this, "Surface");
	}

	public override void AI()
	{
		if (spawn == Vector2.Zero)
		{
			spawn = NPC.Center;
		}

		NPC.spriteDirection = Math.Sign(NPC.velocity.X);
		
		if (NPC.spriteDirection == 0)
		{
			NPC.spriteDirection = 1;
		}

		if (Timer > 60)
		{
			List<Vector2> splineLocations = [];
			Vector2 lastPos = NPC.Center;

			HashSet<int> ids = [TileID.LeafBlock];
			HashSet<int> players = [];

			for (int i = 0; i < 5; ++i)
			{
				Vector2 position = NPC.Center;

				if (i == 4)
				{
					position = spawn;
				}
				else if (i != 0)
				{
					position = lastPos + Main.rand.NextVector2CircularEdge(300, 300) * Main.rand.NextFloat(0.8f, 1f);

					foreach (Player player in Main.ActivePlayers)
					{
						if (!player.dead && !players.Contains(player.whoAmI) && player.DistanceSQ(lastPos) < 500 * 500)
						{
							position = player.Center;
							players.Add(player.whoAmI);
						}
					}
				}

				splineLocations.Add(position);
				lastPos = position;
			}

			spline = Spline.InterpolateXY([.. splineLocations], SplineCount);
			Timer = 0;

			NPC.netUpdate = true;
		}

		if (spline is not null && spline.Length > 0)
		{
			SplineFactor += 0.20f;

			if (SplineFactor > spline.Length)
			{
				NPC.velocity = Vector2.Lerp(NPC.velocity, NPC.DirectionTo(spline[^1]) * 6f, 0.2f);

				if (NPC.DistanceSQ(spline[^1]) < 10 * 10)
				{
					spline = null;
					SplineFactor = 0;
				}

				return;
			}

			int index = (int)SplineFactor;
			Vector2 curr = spline[index];
			Vector2 next = index < spline.Length - 2 ? spline[index + 1] : curr;
			float factor = SplineFactor % 1;

			NPC.velocity = Vector2.Lerp(NPC.velocity, NPC.DirectionTo(Vector2.Lerp(curr, next, factor)) * 8f, 0.2f);
		}
		else
		{
			Timer++;
			NPC.velocity *= 0.8f;
		}
	}

	public override void FindFrame(int frameHeight)
	{
		if (spline is null)
		{
			NPC.frame.Y = 0;
		}
		else
		{
			NPC.frameCounter += 0.4f;
			NPC.frame.Y = frameHeight * (int)(NPC.frameCounter % 4 + 1);
		}
	}

	public override void SendExtraAI(BinaryWriter writer)
	{
		writer.Write(spline is not null);

		if (spline is not null)
		{
			for (int i = 0; i < spline.Length; i++)
			{
				writer.WriteVector2(spline[i]);
			}
		}
	}

	public override void ReceiveExtraAI(BinaryReader reader)
	{
		bool hasSpline = reader.ReadBoolean();

		if (hasSpline)
		{
			spline = new Vector2[SplineCount];

			for (int i = 0; i < spline.Length; i++)
			{
				spline[i] = reader.ReadVector2();
			}
		}
		else
		{
			spline = null;
			SplineFactor = 0;
		}
	}

	public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
	{
		SpriteEffects effect = NPC.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
		spriteBatch.Draw(Glow.Value, NPC.Center - screenPos + Vector2.UnitY * 2, NPC.frame, Color.White, 0f, NPC.frame.Size() / 2f, 1f, effect, 0);
	}
}

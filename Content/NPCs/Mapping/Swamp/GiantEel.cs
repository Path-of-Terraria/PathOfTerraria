using NPCUtils;
using PathOfTerraria.Common.AI;
using PathOfTerraria.Common.NPCs.Components;
using PathOfTerraria.Common.NPCs.Effects;
using PathOfTerraria.Common.NPCs.Worms;
using PathOfTerraria.Common.Subworlds.MappingAreas;
using PathOfTerraria.Common.World.Generation;
using PathOfTerraria.Content.Dusts;
using PathOfTerraria.Content.Gores;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ID;

namespace PathOfTerraria.Content.NPCs.Mapping.Swamp;

#nullable enable

[AutoloadBossHead]
internal class GiantEel : ModNPC
{
	internal class GiantEelBody : WormSegment
	{
		public override void Defaults()
		{
			NPC.Size = new Vector2(50);
			NPC.damage = 60;
			NPC.HitSound = SoundID.NPCHit34;
			NPC.DeathSound = SoundID.NPCHit53;
		}

		public override bool CheckActive()
		{
			return false;
		}

		public override void AI()
		{
			base.AI();

			if (!Parent.active)
			{
				NPC.active = false;
			}

			SpamDust(NPC, NPC.position - NPC.oldPosition);
		}

		public override void HitEffect(NPC.HitInfo hit)
		{
			if (Main.dedServ)
			{
				return;
			}

			int reps = NPC.life < 0 ? 8 : 2;
			Vector2 velocity = NPC.position - NPC.oldPosition;

			for (int i = 0; i < reps; ++i)
			{
				Dust.NewDust(NPC.position, NPC.width, NPC.height, ModContent.DustType<DarkMossDust>(), velocity.X, velocity.X);
			}
		}
	}

	internal class GiantEelTail : GiantEelBody
	{
	}

	private enum States
	{
		Roaming = 0,
		Chase,
		Fury,
		Flee,
	}

	private States State
	{
		get => (States)NPC.ai[0];
		set => NPC.ai[0] = (float)value;
	}

	private ref float Timer => ref NPC.ai[1];
	private ref float TargetPlayer => ref NPC.ai[2];

	private ref float SplineIndex => ref NPC.localAI[0];
	private ref float AvoidWaterTimer => ref NPC.localAI[1];
	private ref float MiscTimer => ref NPC.localAI[2];

	private bool _spawnedSegments = false;
	private Vector2[] _spline = [];

	//private readonly List<Segment> _segments = [];

	public override void Load()
	{
		//GoreLoader.AddGoreFromTexture<SimpleModGore>(Mod, $"{Texture}_GoreBlade");
		//GoreLoader.AddGoreFromTexture<AdvancedGore>(Mod, $"{Texture}_GoreHead");
		//GoreLoader.AddGoreFromTexture<AdvancedGore>(Mod, $"{Texture}_GoreChest");
		//GoreLoader.AddGoreFromTexture<AdvancedGore>(Mod, $"{Texture}_GoreArm");
		//GoreLoader.AddGoreFromTexture<AdvancedGore>(Mod, $"{Texture}_GoreLeg");
	}

	public override void SetDefaults()
	{
		NPC.aiStyle = -1;
		NPC.lifeMax = 150000;
		NPC.defense = 150;
		NPC.damage = 50;
		NPC.width = 62;
		NPC.height = 62;
		NPC.knockBackResist = 0f;
		NPC.noGravity = true;
		NPC.noTileCollide = true;

		NPC.HitSound = new($"{nameof(PathOfTerraria)}/Assets/Sounds/HitEffects/FleshHit", 3) { MaxInstances = 5, Volume = 0.4f };
		NPC.DeathSound = SoundID.NPCDeath23 with { Pitch = +0.1f, PitchVariance = 0.15f, Identifier = "FallenDeath" };

		NPC.TryEnableComponent<NPCVoice>(e =>
		{
			e.Data.PainSound = (3, SoundID.NPCHit56 with { Pitch = +0.3f, PitchVariance = 0.4f, Identifier = "FallenHit" });
		});
		NPC.TryEnableComponent<NPCHitEffects>(c =>
		{
			c.AddGore(new($"{PoTMod.ModName}/{nameof(BloodSplatMedium)}", 1));

			c.AddGore(new($"{PoTMod.ModName}/{nameof(BloodSplatSmall)}", 2, NPCHitEffects.OnDeath));
			c.AddGore(new($"{PoTMod.ModName}/{nameof(BloodSplatMedium)}", 1, NPCHitEffects.OnDeath));
			c.AddGore(new($"{PoTMod.ModName}/{nameof(BloodSplatLarge)}", 1, NPCHitEffects.OnDeath));
			c.AddGore(new($"{PoTMod.ModName}/{nameof(BloodSplatSmall)}", 2, NPCHitEffects.OnDeath));
			c.AddGore(new($"{PoTMod.ModName}/{nameof(BloodSplatMedium)}", 1, NPCHitEffects.OnDeath));
			c.AddGore(new($"{PoTMod.ModName}/{nameof(BloodSplatLarge)}", 1, NPCHitEffects.OnDeath));
		});
	}

	public override bool CheckActive()
	{
		return false;
	}

	public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
	{
		bestiaryEntry.AddInfo(this, "");
	}

	public override void AI()
	{
		if (!_spawnedSegments && Main.netMode != NetmodeID.MultiplayerClient)
		{
			WormSegment.SpawnWhole<GiantEelBody, GiantEelTail>(NPC.GetSource_FromAI(), NPC, 46, 16);
			_spawnedSegments = true;
		}

		if (Math.Abs(NPC.velocity.X) > 0.01f)
		{
			NPC.direction = -Math.Sign(NPC.velocity.X);
		}

		Timer++;
		NPC.rotation = NPC.velocity.ToRotation();
		Lighting.AddLight(NPC.Center, new Vector3(0.6f, 0.6f, 0.1f));

		Player targetPlayer = Main.player[(int)TargetPlayer];
		SpamDust(NPC, NPC.velocity);

		if (State == States.Roaming)
		{
			float aggro = Utils.Remap(targetPlayer.Center.Y / 16f, 40, SwampArea.FloorY, 4000, 1200, true);
			Vector2 targetDirection = NPC.DirectionTo(new Vector2(targetPlayer.Center.X, MathF.Sin(Timer * 0.008f) * (30 * 16) + 180 * 16)) * new Vector2(2.5f, 1);
			NPC.velocity = Vector2.SmoothStep(NPC.velocity, targetDirection * 8, 0.05f) + new Vector2(MathF.Sin(Timer * 0.04f) * 0.25f, 0);

			if (targetPlayer.DistanceSQ(NPC.Center) < aggro * aggro)
			{
				State = States.Chase;
			}
		}
		else if (State == States.Chase)
		{
			if (MiscTimer == 0)
			{
				Vector2 destination = targetPlayer.Center;
				NPC.velocity = Vector2.SmoothStep(NPC.velocity, NPC.DirectionTo(destination) * 20, 0.2f);

				if (NPC.DistanceSQ(destination) < 120 * 120)
				{
					MiscTimer = 1;

					Vector2 reverseDirection = destination.DirectionFrom(NPC.Center);
					_spline = Spline.InterpolateXY([NPC.Center, destination + reverseDirection * 650, destination - reverseDirection * 300 - new Vector2(0, 800)], 6);
				}
			}
			else
			{
				Vector2 destination = _spline[(int)SplineIndex];
				NPC.velocity = Vector2.SmoothStep(NPC.velocity, NPC.DirectionTo(destination) * 20, 0.2f);

				if (NPC.DistanceSQ(destination) < 40 * 40 && SplineIndex < _spline.Length - 1)
				{
					SplineIndex++;
				}

				if (NPC.DistanceSQ(destination) < 60 * 60 && SplineIndex >= _spline.Length - 1)
				{
					MiscTimer = 0;
					SplineIndex = 0;
				}
			}

			if (Collision.WetCollision(NPC.position + new Vector2(0, 200), NPC.width, NPC.height) || Collision.WetCollision(NPC.position, NPC.width, NPC.height))
			{
				AvoidWaterTimer++;

				if (AvoidWaterTimer > 120)
				{
					State = States.Flee;
					MiscTimer = 0;
					AvoidWaterTimer = 0;
				}
			}
		}
		else if (State == States.Flee)
		{
			NPC.velocity = Vector2.SmoothStep(NPC.velocity, new Vector2(MathF.Sin(MiscTimer * 0.03f) * 2, -20), 0.2f);

			if (NPC.Center.Y / 16f < Main.offLimitBorderTiles * 4)
			{
				State = States.Roaming;
			}
		}
	}

	private static void SpamDust(NPC npc, Vector2 velocity)
	{
		if (Main.rand.NextBool(30))
		{
			Dust dust = Main.dust[Dust.NewDust(npc.position, npc.width, npc.height, DustID.JungleGrass)];
			dust.velocity = velocity * 0.2f + new Vector2(0, Main.rand.NextFloat(1, 2));
			dust.noGravity = true;
			dust.alpha = 0;
			dust.scale = Main.rand.NextFloat(1, 2);
		}
	}

	public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
	{
		Texture2D tex = TextureAssets.Npc[Type].Value;
		SpriteEffects flip = NPC.velocity.X >= 0 ? SpriteEffects.None : SpriteEffects.FlipVertically;

		spriteBatch.Draw(tex, NPC.Center - screenPos, NPC.frame, drawColor, NPC.rotation, NPC.frame.Size() / 2f, 1f, flip, 0);
		return false;
	}
}

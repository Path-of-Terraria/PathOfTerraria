using NPCUtils;
using PathOfTerraria.Common.AI;
using PathOfTerraria.Common.NPCs.Components;
using PathOfTerraria.Common.NPCs.Effects;
using PathOfTerraria.Common.NPCs.Worms;
using PathOfTerraria.Common.Subworlds.MappingAreas;
using PathOfTerraria.Content.Gores;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.ID;

namespace PathOfTerraria.Content.NPCs.Mapping.Swamp;

#nullable enable

internal class GiantEel : ModNPC
{
	private readonly struct Context(NPC npc)
	{
		public NPCTargeting Targeting { get; } = npc.GetGlobalNPC<NPCTargeting>();
	}

	internal class GiantEelBody : WormSegment
	{
		public override void Defaults()
		{
			NPC.Size = new Vector2(50);
			NPC.damage = 60;
			NPC.HitSound = SoundID.NPCHit34;
			NPC.DeathSound = SoundID.NPCHit53;
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
			}
		}

		//public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		//{
		//	DrawSelf(NPC, spriteBatch, screenPos);
		//	return false;
		//}

		public override void HitEffect(NPC.HitInfo hit)
		{
			if (Main.dedServ)
			{
				return;
			}

			int reps = NPC.life < 0 ? 8 : 2;

			for (int i = 0; i < reps; ++i)
			{
				Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Electric, NPC.velocity.X, NPC.velocity.X);
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
	}

	private States State
	{
		get => (States)NPC.ai[0];
		set => NPC.ai[0] = (float)value;
	}

	private ref float Timer => ref NPC.ai[1];
	private ref float SubTimer => ref NPC.ai[2];

	private bool _spawnedSegments = false;

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
		NPC.width = 142;
		NPC.height = 42;
		NPC.knockBackResist = 0f;
		NPC.noGravity = true;
		NPC.noTileCollide = true;

		NPC.HitSound = new($"{nameof(PathOfTerraria)}/Assets/Sounds/HitEffects/FleshHit", 3) { MaxInstances = 5, Volume = 0.4f };
		NPC.DeathSound = SoundID.NPCDeath23 with { Pitch = +0.1f, PitchVariance = 0.15f, Identifier = "FallenDeath" };

		NPC.TryEnableComponent<NPCTargeting>();
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
		Context ctx = new(NPC);
		ctx.Targeting.ManualUpdate(new(NPC));

		if (!_spawnedSegments && Main.netMode != NetmodeID.MultiplayerClient)
		{
			WormSegment.SpawnWhole<GiantEelBody, GiantEelTail>(NPC.GetSource_FromAI(), NPC, 50, 12);
			_spawnedSegments = true;
		}

		if (Math.Abs(NPC.velocity.X) > 0.01f)
		{
			NPC.direction = -Math.Sign(NPC.velocity.X);
		}

		NPCAimedTarget target = NPC.GetTargetData();
		float aggro = Utils.Remap(target.Center.Y / 16f, 40, SwampArea.FloorY, 2400, 800, true);

		Timer++;
		NPC.rotation = NPC.velocity.ToRotation();

		if (State == States.Roaming)
		{
			NPC.velocity = Vector2.SmoothStep(NPC.velocity, NPC.DirectionTo(new Vector2(target.Center.X, MathF.Sin(Timer * 0.02f) * (20 * 16) + 40 * 16)), 0.01f);

			if (ctx.Targeting.GetTargetCenter(NPC).DistanceSQ(NPC.Center) < aggro * aggro)
			{
				State = States.Chase;
			}
		}
		else if (State == States.Chase)
		{
			if (SubTimer == 0)
			{
				NPC.velocity = Vector2.SmoothStep(NPC.velocity, NPC.DirectionTo(ctx.Targeting.GetTargetCenter(NPC)) * 16, 0.2f);

				if (NPC.DistanceSQ(ctx.Targeting.GetTargetCenter(NPC)) < 80 * 80)
				{
					SubTimer = 1;
				}
			}
			else
			{
				if (NPC.DistanceSQ(ctx.Targeting.GetTargetCenter(NPC)) > 1200 * 1200)
				{
					SubTimer = 0;
				}
			}
		}
	}
}

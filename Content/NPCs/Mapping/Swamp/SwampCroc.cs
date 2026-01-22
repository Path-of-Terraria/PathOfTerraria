using NPCUtils;
using PathOfTerraria.Common.AI;
using PathOfTerraria.Common.NPCs.Components;
using PathOfTerraria.Common.NPCs.Effects;
using PathOfTerraria.Content.Gores;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.ID;

namespace PathOfTerraria.Content.NPCs.Mapping.Swamp;

internal class SwampCroc : ModNPC
{
	private readonly struct Context(NPC npc)
	{
		public NPCTargeting Targeting { get; } = npc.GetGlobalNPC<NPCTargeting>();
		public NPCAnimations Animations { get; } = npc.GetGlobalNPC<NPCAnimations>();
	}

	private enum States
	{
		Idle = 0,
		Wake,
		Swim,
	}

	private static readonly SpriteAnimation animIdle = new() { Id = "idle", Frames = [0], Speed = 0 };
	private static readonly SpriteAnimation animWake = new() { Id = "wake", Frames = [1, 2, 3, 4], Speed = 12f };
	private static readonly SpriteAnimation animSwim = new() { Id = "swim", Frames = [5, 6, 7, 8, 9, 10, 11, 12], Speed = 12f, Loop = true, UpdateDirection = true };

	private States State
	{
		get => (States)NPC.ai[0];
		set => NPC.ai[0] = (float)value;
	}

	public override void Load()
	{
		//GoreLoader.AddGoreFromTexture<SimpleModGore>(Mod, $"{Texture}_GoreBlade");
		//GoreLoader.AddGoreFromTexture<AdvancedGore>(Mod, $"{Texture}_GoreHead");
		//GoreLoader.AddGoreFromTexture<AdvancedGore>(Mod, $"{Texture}_GoreChest");
		//GoreLoader.AddGoreFromTexture<AdvancedGore>(Mod, $"{Texture}_GoreArm");
		//GoreLoader.AddGoreFromTexture<AdvancedGore>(Mod, $"{Texture}_GoreLeg");
	}

	public override void SetStaticDefaults()
	{
		NPCID.Sets.TeleportationImmune[Type] = true;
		NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Shimmer] = true;
		Main.npcFrameCount[Type] = 13;
	}

	public override void SetDefaults()
	{
		NPC.aiStyle = -1;
		NPC.lifeMax = 400;
		NPC.defense = 15;
		NPC.damage = 50;
		NPC.width = 142;
		NPC.height = 42;
		NPC.knockBackResist = 0.3f;
		NPC.noGravity = true;

		NPC.HitSound = new($"{nameof(PathOfTerraria)}/Assets/Sounds/HitEffects/FleshHit", 3) { MaxInstances = 5, Volume = 0.4f };
		NPC.DeathSound = SoundID.NPCDeath23 with { Pitch = +0.1f, PitchVariance = 0.15f, Identifier = "FallenDeath" };

		NPC.TryEnableComponent<NPCTargeting>();
		NPC.TryEnableComponent<NPCAnimations>(e =>
		{
			e.BaseFrame = new SpriteFrame(1, 13) with { PaddingX = 0, PaddingY = 0 };
			e.SpriteOffset = new(0f, -1f);
		});
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
			//c.AddGore(new($"{PoTMod.ModName}/{Name}_GoreHead", +1, NPCHitEffects.OnDeath) { Position = (new(0, 0), new(+00, -16), new(3, 3)) });
			//c.AddGore(new($"{PoTMod.ModName}/{Name}_GoreChest", 1, NPCHitEffects.OnDeath) { Position = (new(0, 0), new(+00, +00), new(3, 3)) });
			//c.AddGore(new($"{PoTMod.ModName}/{Name}_GoreBlade", 1, NPCHitEffects.OnDeath) { Position = (new(0, 0), new(+00, +00), new(15, 15)) });
			//c.AddGore(new($"{PoTMod.ModName}/{Name}_GoreArm", +1, NPCHitEffects.OnDeath) { Position = (new(0, 0), new(-16, +00), new(3, 3)) });
			//c.AddGore(new($"{PoTMod.ModName}/{Name}_GoreArm", +1, NPCHitEffects.OnDeath) { Position = (new(0, 0), new(+16, +00), new(3, 3)) });
			//c.AddGore(new($"{PoTMod.ModName}/{Name}_GoreLeg", +1, NPCHitEffects.OnDeath) { Position = (new(0, 0), new(+16, -16), new(3, 3)) });
			//c.AddGore(new($"{PoTMod.ModName}/{Name}_GoreLeg", +1, NPCHitEffects.OnDeath) { Position = (new(0, 0), new(+16, +16), new(3, 3)) });
			c.AddGore(new($"{PoTMod.ModName}/{nameof(BloodSplatSmall)}", 2, NPCHitEffects.OnDeath));
			c.AddGore(new($"{PoTMod.ModName}/{nameof(BloodSplatMedium)}", 1, NPCHitEffects.OnDeath));
			c.AddGore(new($"{PoTMod.ModName}/{nameof(BloodSplatLarge)}", 1, NPCHitEffects.OnDeath));
		});
	}

	public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
	{
		bestiaryEntry.AddInfo(this, "");
	}

	public override void FindFrame(int frameHeight)
	{
		Context ctx = new(NPC);
		ctx.Animations.Advance();
		ctx.Animations.Set(PickAnimation(in ctx));
	}

	public override void AI()
	{
		Context ctx = new(NPC);
		ctx.Targeting.ManualUpdate(new(NPC));

		if (Math.Abs(NPC.velocity.X) > 0.01f)
		{
			NPC.direction = -Math.Sign(NPC.velocity.X);
		}

		if (State == States.Idle)
		{
			NPC.velocity.X *= 0.95f;

			if (Collision.WetCollision(NPC.Left - new Vector2(0, 12), NPC.width, 8))
			{
				if (Collision.WetCollision(NPC.Left - new Vector2(0, 20), NPC.width, 8))
				{
					NPC.velocity.Y -= 0.07f;
				}
				else 
				{ 
					NPC.velocity *= 0.8f;
				}
			}
			else
			{
				NPC.velocity.Y += !NPC.wet ? 0.25f : 0.1f;
			}

			NPCAimedTarget data = NPC.GetTargetData();
			float dist = Collision.WetCollision(data.Position, data.Width, data.Height) ? 280 * 280 : 160 * 160;

			if (ctx.Targeting.GetTargetCenter(NPC).DistanceSQ(NPC.Center) < 160 * 160)
			{
				State = States.Swim;
			}
		}
		else if (State == States.Swim)
		{
			Vector2 target = !NPC.wet ? NPC.velocity + new Vector2(0, 10f) : NPC.DirectionTo(ctx.Targeting.GetTargetCenter(NPC)) * 12;
			NPC.velocity = Vector2.SmoothStep(NPC.velocity, target, 0.1f);

			if (!NPC.wet)
			{
				NPC.velocity.X *= 0.95f;
			}

			NPCAimedTarget data = NPC.GetTargetData();
			float dist = Collision.WetCollision(data.Position, data.Width, data.Height) ? 800 * 800 : 400 * 400;

			if (ctx.Targeting.GetTargetCenter(NPC).DistanceSQ(NPC.Center) > dist || !Collision.CanHit(NPC, NPC.GetTargetData()))
			{
				State = States.Idle;
			}
		}
	}

	private SpriteAnimation? PickAnimation(in Context ctx)
	{
		Vector2 vel = NPC.position - NPC.oldPosition;

		return ctx.Animations.Current switch
		{
			_ when State == States.Swim || NPC.IsABestiaryIconDummy => animSwim,
			_ when State == States.Idle => animIdle,
			_ when State == States.Wake => animWake,
			_ => null,
		};
	}
}

using NPCUtils;
using PathOfTerraria.Common.AI;
using PathOfTerraria.Common.NPCs.Components;
using PathOfTerraria.Common.NPCs.Effects;
using PathOfTerraria.Content.Dusts;
using PathOfTerraria.Content.Gores;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.ID;

namespace PathOfTerraria.Content.Swamp.NPCs;

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
	private static readonly SpriteAnimation animWake = new() { Id = "wake", Frames = [1, 2, 3, 4], Speed = 8f, Loop = false };
	private static readonly SpriteAnimation animSwim = new() { Id = "swim", Frames = [5, 6, 7, 8, 9, 10, 11, 12], Speed = 12f, Loop = true, UpdateDirection = true };

	private Vector2 HeadPosition => NPC.position + new Vector2(NPC.direction == 1 ? 30 : NPC.width - 30, 0);

	private States State
	{
		get => (States)NPC.ai[0];
		set => NPC.ai[0] = (float)value;
	}

	private bool WorldSpawned
	{
		get => NPC.ai[1] == 1;
		set => NPC.ai[1] = value ? 1 : 0;
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
		NPC.lifeMax = 600;
		NPC.defense = 15;
		NPC.damage = 50;
		NPC.width = 142;
		NPC.height = 42;
		NPC.knockBackResist = 0.1f;
		NPC.noGravity = true;
		NPC.value = Item.buyPrice(0, 0, 35, 0);

		NPC.HitSound = new($"{nameof(PathOfTerraria)}/Assets/Sounds/HitEffects/FleshHit", 3) { MaxInstances = 5, Volume = 0.4f };
		NPC.DeathSound = SoundID.NPCDeath23 with { Pitch = +0.1f, PitchVariance = 0.15f, Identifier = $"{Name}Death" };

		NPC.TryEnableComponent<NPCTargeting>();
		NPC.TryEnableComponent<NPCAnimations>(e =>
		{
			e.BaseFrame = new SpriteFrame(1, 13) with { PaddingX = 0, PaddingY = 0 };
			e.SpriteOffset = new(0f, -1f);
		});
		NPC.TryEnableComponent<NPCVoice>(e =>
		{
			e.Data.PainSound = (3, SoundID.NPCHit56 with { Pitch = +0.3f, PitchVariance = 0.4f, Identifier = $"{Name}Hit" });
		});
		NPC.TryEnableComponent<NPCHitEffects>(c =>
		{
			c.AddGore(new($"{PoTMod.ModName}/{nameof(BloodSplatMedium)}", 1));

			c.AddGore(new($"{PoTMod.ModName}/{nameof(BloodSplatSmall)}", 2, NPCHitEffects.OnDeath));
			c.AddGore(new($"{PoTMod.ModName}/{nameof(BloodSplatMedium)}", 1, NPCHitEffects.OnDeath));
			c.AddGore(new($"{PoTMod.ModName}/{nameof(BloodSplatLarge)}", 1, NPCHitEffects.OnDeath));
			c.AddGore(new($"{PoTMod.ModName}/{Name}_0", +1, NPCHitEffects.OnDeath) { Position = (new(0, 0), new Vector2(+12, +20), new(3, 3)), FlipWithDirection = false, NoCentering = true });
			c.AddGore(new($"{PoTMod.ModName}/{Name}_1", +1, NPCHitEffects.OnDeath) { Position = (new(0, 0), new Vector2(+116, +14), new(3, 3)), FlipWithDirection = false, NoCentering = true });
			c.AddGore(new($"{PoTMod.ModName}/{Name}_2", +1, NPCHitEffects.OnDeath) { Position = (new(0, 0), new Vector2(+70, +32), new(3, 3)), FlipWithDirection = false, NoCentering = true });
			c.AddGore(new($"{PoTMod.ModName}/{Name}_3", +1, NPCHitEffects.OnDeath) { Position = (new(0, 0), new Vector2(+68, +18), new(12, 3)), FlipWithDirection = false, NoCentering = true });
			c.AddGore(new($"{PoTMod.ModName}/{Name}_4", +1, NPCHitEffects.OnDeath) { Position = (new(0, 0), new Vector2(+36, +32), new(3, 3)), FlipWithDirection = false, NoCentering = true });
			c.AddGore(new($"{PoTMod.ModName}/{nameof(BloodSplatSmall)}", 2, NPCHitEffects.OnDeath));
			c.AddGore(new($"{PoTMod.ModName}/{nameof(BloodSplatMedium)}", 1, NPCHitEffects.OnDeath));
			c.AddGore(new($"{PoTMod.ModName}/{nameof(BloodSplatLarge)}", 1, NPCHitEffects.OnDeath));
		});
	}

	public override bool CheckActive()
	{
		return !WorldSpawned;
	}

	public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
	{
		return State == States.Idle ? false : null;
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
		else
		{
			NPC.direction = NPC.spriteDirection;
		}

		NPC.ShowNameOnHover = State != States.Idle;
		Lighting.AddLight(HeadPosition, new Vector3(0.05f, 0.1f, 0.08f));

		if (State == States.Idle)
		{
			NPC.velocity.X *= 0.95f;

			if (Collision.WetCollision(NPC.Left - new Vector2(0, 8), NPC.width, 8))
			{
				if (Collision.WetCollision(NPC.Left - new Vector2(0, 16), NPC.width, 8))
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
			float dist = Collision.WetCollision(data.Position - new Vector2(0, 40), data.Width, data.Height) ? 360 * 360 : 160 * 160;

			if (ctx.Targeting.GetTargetCenter(NPC).DistanceSQ(NPC.Center) < dist)
			{
				State = States.Wake;
				NPC.netUpdate = true;

				Dust.NewDustPerfect(HeadPosition, ModContent.DustType<AlertDust>(), new Vector2(0, -6), 0);
			}
		}
		else if (State == States.Wake && ctx.Animations.Completed)
		{
			State = States.Swim;
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
				NPC.netUpdate = true;
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

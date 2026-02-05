using System.Runtime.CompilerServices;
using PathOfTerraria.Common.AI;
using PathOfTerraria.Common.NPCs;
using PathOfTerraria.Common.NPCs.Components;
using PathOfTerraria.Common.NPCs.Effects;
using PathOfTerraria.Common.Utilities;
using PathOfTerraria.Common.World.Utilities;
using PathOfTerraria.Content.Gores;
using PathOfTerraria.Core.Time;
using PathOfTerraria.Utilities;
using PathOfTerraria.Utilities.Terraria;
using ReLogic.Utilities;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;

#nullable enable

namespace PathOfTerraria.Content.Conflux;

internal sealed class InfernalBoss : ModNPC
{
	public enum Flag : byte
	{
	}
	private readonly struct Context(NPC npc)
	{
		public Vector2 Center { get; } = npc.Center;
		public NPCMovement Movement { get; } = npc.GetGlobalNPC<NPCMovement>();
		public NPCNavigation Navigation { get; } = npc.GetGlobalNPC<NPCNavigation>();
		public NPCTargeting Targeting { get; } = npc.GetGlobalNPC<NPCTargeting>();
		public NPCAttacking Attacking { get; } = npc.GetGlobalNPC<NPCAttacking>();
		public NPCAnimations Animations { get; } = npc.GetGlobalNPC<NPCAnimations>();
		// public NPCTeleports Teleports { get; } = npc.GetGlobalNPC<NPCTeleports>();
	}

	private static readonly SpriteAnimation animIdle = new() { Id = "idle", Frames = [0], Speed = 3f };
	private static readonly SpriteAnimation animJump = new() { Id = "jump", Frames = [0] };
	private static readonly SpriteAnimation animFall = new() { Id = "fall", Frames = [0] };
	private static readonly SpriteAnimation animWalk = new() { Id = "walk", Frames = [0] };
	private static readonly SpriteAnimation animAttack = new() { Id = "attack", Frames = [0], Speed = 17f, Loop = false };

	// public ref float FlightCounter => ref NPC.ai[3];
	public ref Flag Flags => ref Unsafe.As<float, Flag>(ref NPC.ai[3]);

	private float flightVolume;
	private SlotId flightSound;

	public override void Load()
	{
		
	}

	public override void SetStaticDefaults()
	{
		NPCID.Sets.UsesNewTargetting[Type] = true;
		NPCID.Sets.TeleportationImmune[Type] = true;
		NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.OnFire] = true;
		NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.CursedInferno] = true;
		NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Shimmer] = true;
		NPCID.Sets.ShouldBeCountedAsBoss[Type] = true;
		Main.npcFrameCount[Type] = 4;
	}
	public override void SetDefaults()
	{
		NPCID.Sets.ShouldBeCountedAsBoss[Type] = true;
		NPC.aiStyle = -1;
		NPC.lifeMax = 1 << 17;
		NPC.defense = 35;
		NPC.damage = 50;
		NPC.width = 280;
		NPC.height = 200;
		NPC.knockBackResist = 0.0f;
		NPC.boss = true;
		NPC.lavaImmune = true;
		Music = 0;

		NPC.HitSound = new($"{nameof(PathOfTerraria)}/Assets/Sounds/HitEffects/FleshHit", 3) { Volume = 0.4f, Pitch = -0.5f, MaxInstances = 5 };
		NPC.DeathSound = SoundID.NPCDeath23 with { Pitch = -0.85f, PitchVariance = 0.15f, Identifier = $"{Name}Death" };

		NPC.TryEnableComponent<NPCNavigation>(e =>
		{
			e.JumpRange = new(20, 30);
		});
		NPC.TryEnableComponent<NPCMovement>(e =>
		{
			e.Data.MaxSpeed = 5f;
			e.Data.Acceleration = 16f;
			e.Data.Friction = (8f, 2f);
		});
		NPC.TryEnableComponent<NPCAnimations>(e =>
		{
			e.BaseFrame = new SpriteFrame(1, 1) with { PaddingX = 0, PaddingY = 0 };
			e.SpriteOffset = new(0f, -142f);
		});
		NPC.TryEnableComponent<NPCTargeting>();
		NPC.TryEnableComponent<NPCAttacking>(e =>
		{
			e.Data.LengthInTicks = 120;
			e.Data.CooldownLength = 60;
			e.Data.NoGravityLength = 0;
			e.Data.InitiationRange = new(512f, 192f);
			e.Data.Dash = (60, 80, new(0f, 0f));
			e.Data.Damage = (60, 80, DamageInstance.EnemyAttackFilter);
			// e.Data.Hitbox = (new(512, 512), new(+0f, +0f), new(+0f, +0f));
			e.Data.Hitbox = default;
			e.Data.Movement = (0.0f, 0.80f, 0.95f);

			if (!Main.dedServ)
			{
				e.Data.Sounds =
				[
					(20, new($"{nameof(PathOfTerraria)}/Assets/Sounds/Conflux/CryoStalkerCast") {
						MaxInstances = 2,
						Volume = 0.3f,
						Pitch = -0f,
						PitchVariance = 0.15f,
					}),
				];
			}
		});
		NPC.TryEnableComponent<NPCTeleports>(e =>
		{
			// General
			e.Data.Movement = TeleportType.Interpolated;
			e.Data.MaxCooldown = 60;
			e.Data.Disappear = (0, 12);
			e.Data.Reappear = (42, 54);
			e.Data.Invulnerability = (0, 45);
			e.Data.CooldownDamage = (0f, 0);
			e.Data.Velocity = (new(1f, -1f), new(1f, -2f));
			e.Data.LightColor = Color.IndianRed.ToVector3();
			e.Data.DisappearSound = (0, SoundID.Shimmer1 with { Pitch = -0.4f, PitchVariance = 0.1f });
			e.Data.ReappearSound = (41, SoundID.DD2_DarkMageAttack with { Pitch = -0.8f, PitchVariance = 0.1f });
			// Triggers
			e.Data.TriggerAtDistance = (400f, float.PositiveInfinity);
			// e.Data.TriggerAtDistance = (0f, 64f);
			// Placement
			e.Data.PlaceOriginAtTarget = false;
			// e.Data.RequireDifferentDirection = true;
			e.Data.RequireReachablePoint = true;
			e.Data.RequireLineOfSightOnExit = true;
			e.Data.RequiredTargetDistance = (250f, 350f);
			e.Data.BasePlacement.Area = new Rectangle() with { Width = 64, Height = 48 };
			e.Data.BasePlacement.OnGround = false;
			e.Data.BasePlacement.MinDistanceFromPlayers = 250f;
		});
		NPC.TryEnableComponent<NPCFootsteps>(e =>
		{
			e.Data.Frames = new() { { "walk", [0, 3] } };
			e.Data.StepSound = new SoundStyle($"{PoTMod.ModName}/Assets/Sounds/Footsteps/MonsterStomp", 3)
			{
				Volume = 0.25f,
				Pitch = 0.0f,
				PitchVariance = 0.2f,
				MaxInstances = 8,
			};
		});
		NPC.TryEnableComponent<NPCVoice>(e =>
		{
			e.Data.PainSound = (15, SoundID.NPCHit56 with { Pitch = 0f, PitchVariance = 0.5f, Identifier = "ShamanHit" });
		});
		NPC.TryEnableComponent<NPCHitEffects>(c =>
		{
			c.AddGore(new($"{PoTMod.ModName}/{nameof(BloodSplatSmall)}", 1));
			c.AddGore(new($"{PoTMod.ModName}/{nameof(BloodSplatMedium)}", 1));
			c.AddGore(new($"{PoTMod.ModName}/{nameof(BloodSplatLarge)}", 1));

			c.AddGore(new($"{PoTMod.ModName}/{nameof(BloodSplatSmall)}", 15, NPCHitEffects.OnDeath));
			c.AddGore(new($"{PoTMod.ModName}/{nameof(BloodSplatMedium)}", 10, NPCHitEffects.OnDeath));
			c.AddGore(new($"{PoTMod.ModName}/{nameof(BloodSplatLarge)}", 5, NPCHitEffects.OnDeath));
			// c.AddGore(new($"{PoTMod.ModName}/{Name}_GoreHead", +1, NPCHitEffects.OnDeath) { Position = (new(0, 0), new(+00, -16), new(3, 3)) });
			// c.AddGore(new($"{PoTMod.ModName}/{Name}_GoreChest", 1, NPCHitEffects.OnDeath) { Position = (new(0, 0), new(+00, +00), new(3, 3)) });
			// c.AddGore(new($"{PoTMod.ModName}/{Name}_GoreLeg", +1, NPCHitEffects.OnDeath) { Position = (new(0, 0), new(+16, -16), new(3, 3)) });
			// c.AddGore(new($"{PoTMod.ModName}/{Name}_GoreLeg", +1, NPCHitEffects.OnDeath) { Position = (new(0, 0), new(+16, +16), new(3, 3)) });
			// c.AddGore(new($"{PoTMod.ModName}/{Name}_GoreStaff1", +1, NPCHitEffects.OnDeath) { Position = (new(0, 0), new(-16, -32), new(3, 3)) });
			// c.AddGore(new($"{PoTMod.ModName}/{Name}_GoreStaff2", +1, NPCHitEffects.OnDeath) { Position = (new(0, 0), new(+00, -16), new(3, 3)) });
			// c.AddGore(new($"{PoTMod.ModName}/{Name}_GoreStaff3", +1, NPCHitEffects.OnDeath) { Position = (new(0, 0), new(+16, +00), new(3, 3)) });
			// c.AddGore(new($"{PoTMod.ModName}/{Name}_GoreCloth1", +1, NPCHitEffects.OnDeath) { Position = (new(0, 0), new(+16, +00), new(3, 3)) });
			// c.AddGore(new($"{PoTMod.ModName}/{Name}_GoreCloth2", +1, NPCHitEffects.OnDeath) { Position = (new(0, 0), new(+16, +00), new(3, 3)) });
			c.AddGore(new($"{PoTMod.ModName}/{nameof(BloodSplatSmall)}", 15, NPCHitEffects.OnDeath));
			c.AddGore(new($"{PoTMod.ModName}/{nameof(BloodSplatMedium)}", 10, NPCHitEffects.OnDeath));
			c.AddGore(new($"{PoTMod.ModName}/{nameof(BloodSplatLarge)}", 5, NPCHitEffects.OnDeath));
		});
	}

	public override void AI()
	{
		Context ctx = new(NPC);

		// Invoke behaviors.
		CustomBehavior(in ctx);
		ctx.Animations.Advance();
		ctx.Targeting.ManualUpdate(new(NPC));
		// ctx.Teleports.ManualUpdate(new(NPC));
		ctx.Attacking.ManualUpdate(new(NPC));
		ctx.Movement.ManualUpdate(new(NPC));
		ctx.Animations.Set(PickAnimation(in ctx));
	}

	private void CustomBehavior(in Context ctx)
	{
		bool atkActive = ctx.Attacking.Active;
		int atkProg = ctx.Attacking.Data.Progress;
		int atkBase = ctx.Attacking.Data.Dash.Start;

		// Reset overrides.
		ctx.Movement.Data.TargetOverride = null;
		ctx.Attacking.Data.ManualInitiation = true;

		// Shoot projectiles during non-respawn attacks.
		for (int i = 0; i < (atkActive ? 15 : 0); i++)
		{
			if (atkProg == 1)
			{
				SoundEngine.PlaySound(new($"{nameof(PathOfTerraria)}/Assets/Sounds/Conflux/FallenShamanAttack") {
					MaxInstances = 2,
					Volume = 0.5f,
					PitchVariance = 0.15f,
				}, ctx.Center);
			}
			
			if (atkProg != atkBase + (i * 5)) { continue; }

			Vector2 targetCenter = ctx.Targeting.GetTargetCenter(NPC);

			if (Main.netMode != NetmodeID.MultiplayerClient)
			{
				int projType = ModContent.ProjectileType<FallenShamanRocket>();
				int projDamage = ModeUtils.ProjectileDamage(NPC.damage);
				// Move out of solid tiles.
				Vector2 position = NPC.Center + new Vector2(0f, -48f);
				for (Point16 xy = position.ToTileCoordinates16(); xy.Y < 0 || WorldUtilities.SolidTile(xy.X, xy.Y);)
				{
					position.Y += 8f;
					xy = position.ToTileCoordinates16();
				}
				float projAngle = targetCenter.AngleFrom(position) + (Main.rand.NextFloatDirection() * MathHelper.ToRadians(40f));
				Vector2 projVel = projAngle.ToRotationVector2() * 0.5f;
				Projectile.NewProjectileDirect(NPC.GetSource_FromThis(), position, projVel, projType, projDamage, 1f);
			}

			if (!Main.dedServ)
			{
				SoundEngine.PlaySound(SoundID.Item92 with { Volume = 0.70f, MaxInstances = 5, Pitch = -0.1f, PitchVariance = 0.5f }, ctx.Center);
			}
		}

		// Effects.
		if (!Main.dedServ)
		{
			Lighting.AddLight(NPC.Top, Color.Red.ToVector3() * 0.5f);

			if (true)
			{
				Dust.NewDustDirect(NPC.BottomLeft + new Vector2(0f, -8f), NPC.width, 8, DustID.SomethingRed, NPC.velocity.X * 2f, NPC.velocity.Y * 2f, Scale: 1f);
			}

			var loopSound = new SoundStyle($"{nameof(PathOfTerraria)}/Assets/Sounds/Gore/FleshLoopChaotic")
			{
				Volume = 0.1f,
				IsLooped = true,
				PauseBehavior = PauseBehavior.PauseWithGame,
			};
			float speed = NPC.velocity.Length();
			float halfStep = 1f * TimeSystem.LogicDeltaTime;
			float target = 1f;
			float volume = flightVolume = MathUtils.StepTowards(MathHelper.Lerp(flightVolume, target, halfStep), target, halfStep);
			float pitch = Math.Clamp(-0.9f + (speed * 0.1f), -0.9f, 0.2f);
			SoundUtils.UpdateLoopingSound(ref flightSound, ctx.Center, volume, pitch, loopSound,_ => Main.npc[NPC.whoAmI] is { active: true } n && n == NPC);
		}
	}

	private SpriteAnimation? PickAnimation(in Context ctx)
	{
		const float MinWalkSpeed = 0.5f;

		Vector2 vel = NPC.position - NPC.oldPosition;

		return ctx.Animations.Current switch
		{
			_ when ctx.Attacking.Active => animAttack,
			// _ when ctx.Teleports.Active => ctx.Teleports.Data.Progress <= ctx.Teleports.Data.Reappear.Start ? animVanish with { Speed = 6f } : animAppear,
			_ when vel.Y < 0f => animJump,
			_ when vel.Y > 0f => animFall,
			_ when vel.Y == 0f && Math.Abs(vel.X) >= MinWalkSpeed => animWalk with { Speed = vel.X * animWalk.Speed * 4f * NPC.spriteDirection },
			_ when vel.Y == 0f && Math.Abs(vel.X) <= MinWalkSpeed => animIdle,
			_ => null,
		};
	}
}


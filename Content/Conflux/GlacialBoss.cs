// #define IK_PREVIEW
#define DEBUG_GIZMOS
// #define FRIENDLY
// #define FOLLOW_MOUSE
// #define NO_MINIONS
#define NO_TELEPORTS
// #define NO_FLAMES
#define DEBUG_KEYS
// #define HIDE_SWORD
// #define FORCE_PHASE_II
// #define LOW_HEALTH

using System.IO;
using MonoMod.Cil;
using PathOfTerraria.Common.AI;
using PathOfTerraria.Common.NPCs.Components;
using PathOfTerraria.Common.NPCs.Effects;
using PathOfTerraria.Common.Utilities;
using PathOfTerraria.Common.Utilities.Extensions;
using PathOfTerraria.Content.Gores;
using PathOfTerraria.Core.Camera;
using PathOfTerraria.Core.Interface;
using PathOfTerraria.Core.Time;
using PathOfTerraria.Utilities;
using PathOfTerraria.Utilities.Terraria;
using PathOfTerraria.Utilities.Xna;
using ReLogic.Content;
using ReLogic.Utilities;
using SubworldLibrary;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;

#nullable enable
#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable CA1822 // Mark members as static

namespace PathOfTerraria.Content.Conflux;

internal sealed class GlacialBossBar : ModBossBar
{
	public override bool PreDraw(SpriteBatch spriteBatch, NPC npc, ref BossBarDrawParams drawParams)
	{
		if (npc.ModNPC is not GlacialBoss { Phase: > 0, CutsceneActive: false } boss)
		{
			return false;
		}

		// Alter displayed health to keep the second phase a secret.
		float lifeFactor = GlacialBoss.SecondPhaseHealthFactor;
		bool secondPhase = boss.Phase is GlacialBoss.PhaseType.Second;
		lifeFactor = secondPhase ? lifeFactor : (1f - lifeFactor);
		drawParams.Life -= secondPhase ? 0 : (int)(drawParams.LifeMax * (1f - lifeFactor));
		drawParams.LifeMax = (int)(drawParams.LifeMax * lifeFactor);
		drawParams.Life = Math.Clamp(drawParams.Life, 0, drawParams.LifeMax);

		return true;
	}
}

internal static class GlacialBossCollision
{
	internal sealed class GlacialBossNPC : GlobalNPC
	{
		private Vector2? lastSegmentVelocity;

		public override bool InstancePerEntity => true;
	
		public override void Load()
		{
			IL_NPC.UpdateCollision += CheckNPCCollision;
		}

		private static void CheckNPCCollision(ILContext ctx)
		{
			var il = new ILCursor(ctx);
			il.EmitLdarg0();
			il.EmitDelegate(static (NPC npc) =>
			{
				if (!npc.noGravity && !npc.noTileCollide && npc.TryGetGlobalNPC(out GlacialBossNPC global))
				{
					ApplyEntityCollision<NPC>(npc, ref global.lastSegmentVelocity);
				}
			});
		}
	}
	internal sealed class GlacialBossPlayer : ModPlayer
	{
		private Vector2? lastSegmentVelocity;

		public override void PreUpdateMovement()
		{
			ApplyEntityCollision<Player>(Player, ref lastSegmentVelocity);
		}
	}
	
	public static void ApplyEntityCollision<T>(Entity entity, ref Vector2? lastSegmentVelocity) where T : Entity
	{
		bool ridingSegment = false;
		Player player = typeof(T) == typeof(Player) ? (Player)entity : null!;
	
		if (entity.velocity.Y <= 0 || (typeof(T) == typeof(Player) && player.controlDown))
		{
			goto SkipCollision;
		}

		const float DepthCutoff = -0.15f;

		// Allow players to walk on this boss.
		foreach (NPC npc in Main.ActiveNPCs)
		{
			if (npc.ModNPC is not GlacialBoss boss) { continue; }
		
			Rectangle npcRect = npc.getRect();
			Rectangle entityRect = new((int)entity.position.X, (int)entity.position.Y, entity.width, entity.height);
			Rectangle lowRect = entityRect with { Height = entity.height / 2, Y = (int)entity.position.Y + entity.height / 2 };

			if (!lowRect.Intersects(npcRect)) { continue; }

			foreach (ref readonly GlacialBoss.Segment segment in boss.segments.AsSpan())
			{
				Rectangle segAabb = segment.GetAabb();
				if (segment.Depth < DepthCutoff) { continue; }
				if (!lowRect.Intersects(segAabb)) { continue; }

				int standY = (segAabb.Top - entity.height) + 16;
				Vector2 oldPos = entity.position;
				Vector2 newPos = (oldPos with { Y = Math.Min(entity.position.Y, standY) }) + segment.Velocity;
			
				if (!Collision.SolidCollision(newPos, entity.width, entity.height))
				{
					entity.position = newPos;
					entity.velocity.Y = 0;
					lastSegmentVelocity = segment.Velocity;
					ridingSegment = true;

					if (typeof(T) == typeof(Player))
					{
						player.gfxOffY = Math.Max(player.gfxOffY, oldPos.Y - newPos.Y);
					}
				}
			}
		}

		SkipCollision:;
		if (!ridingSegment && lastSegmentVelocity is Vector2 vel && vel.Floor() != default)
		{
			const float maxSpeed = 10;

			var direction = Vector2.Normalize(vel);
			entity.velocity = MovementUtils.DirAccel(entity.velocity, direction, vel.Length(), vel.Length());
			
			if (vel.Length() > maxSpeed)
			{
				vel = Vector2.Normalize(vel) * maxSpeed;
			}
		
			// entity.velocity += vel;
			lastSegmentVelocity = null;
		}
	}
}

internal sealed class GlacialBoss : ModNPC
{
	public enum CutsceneType : byte
	{
		None = 0,
		Intro,
		Transformation,
		Death,
	}
	public record struct CutsceneData(CutsceneType Type, ushort Counter, ushort Length)
	{
		public bool Initialized;
	}
	public enum PhaseType : byte
	{
		Idle = 0,
		First = 1,
		Second = 2,
	}
	public record struct Segment()
	{
		public required string TexturePath;
		public required float Length;
		public Asset<Texture2D>? TextureCache = null;
		public Asset<Texture2D>? GlowCache = null;
		public SpriteFrame Frame;
		public Vector2 Position;
		public Vector2 Velocity;
		public float Depth;
		public float BaseRotation;
		public float Rotation;
		public bool WasSubmerged;

		public readonly Rectangle GetAabb()
		{
			float length = Length * 1.5f;
			return new Rectangle((int)(Position.X - length), (int)(Position.Y - length), (int)length, (int)length);
		}
	}

	private enum AttackType
	{
		None,
		Bite,
		BulletHell,
	}
	private record struct AnimationKey(Vector2 Position, float Depth = 0f)
	{
		static AnimationKey()
		{
			Gradient<AnimationKey>.Lerp = Lerp;
		}

		public static AnimationKey Lerp(AnimationKey a, AnimationKey b, float step)
		{
			return new(Vector2.Lerp(a.Position, b.Position, step), MathHelper.Lerp(a.Depth, b.Depth, step));
		}
	}
	private readonly struct Context(NPC npc)
	{
		public Vector2 Center { get; } = npc.Center;
		public Vector2 TargetCenter { get; } = npc.GetGlobalNPC<NPCTargeting>().GetTargetCenter(npc);
		public NPCMovement Movement { get; } = npc.GetGlobalNPC<NPCMovement>();
		public NPCTargeting Targeting { get; } = npc.GetGlobalNPC<NPCTargeting>();
		public NPCAttacking Attacking { get; } = npc.GetGlobalNPC<NPCAttacking>();
		public NPCAnimations Animations { get; } = npc.GetGlobalNPC<NPCAnimations>();
		public NPCFootsteps Footsteps { get; } = npc.GetGlobalNPC<NPCFootsteps>();
		public NPCTeleports Teleports { get; } = npc.GetGlobalNPC<NPCTeleports>();
		public NPCVoice Voice { get; } = npc.GetGlobalNPC<NPCVoice>();
	}

	public const float SecondPhaseHealthFactor = 0.6f;

	private static readonly SpriteAnimation animIdle = new() { Id = "idle", Frames = [0], Speed = 3f };
	private static readonly int phase1Music = MusicID.OtherworldlyBoss2;
	private static readonly int phase2Music = MusicID.OtherworldlyLunarBoss;
	private static int[] minionTypes = [];
	private static Gradient<AnimationKey> testPath = [];

	// Synchronized state:
	public PhaseType Phase = PhaseType.Idle;
	private CutsceneData Cutscene;
	private uint globalCounter;
	private ushort grabCooldown;
	private ushort spawnCooldown;
	private ushort bulletAttackCounter;
	private AttackType attackType;
	internal Segment[] segments = [];

	// Non-synchronized state:
	private int lifeOld;
	/// <summary> World-relative body angle. </summary>
	private (float Target, float Current) bodyAngle;
	/// <summary> Body-relative head angle. </summary>
	private (float Target, float Current) headAngle;
	/// <summary> Value increased by current movement speed. </summary>
	private float movementAnimation;
	private (SlotId Handle, float Intensity) movementSound;

	public bool CutsceneActive => Cutscene.Type != 0;
	public Vector2 Center => segments.Length > 0 ? segments[0].Position : NPC.Center;

	public override void Load()
	{
		//
	}

	public override void SetStaticDefaults()
	{
		NPCID.Sets.UsesNewTargetting[Type] = true;
		NPCID.Sets.TeleportationImmune[Type] = true;
		NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Frostburn] = true;
		NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Frostburn2] = true;
		NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Frozen] = true;
		NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Shimmer] = true;
		NPCID.Sets.ShouldBeCountedAsBoss[Type] = true;
		Main.npcFrameCount[Type] = 1;

		minionTypes =
		[
			ModContent.NPCType<Shambling>(),
			ModContent.NPCType<CryoStalker>(),
			ModContent.NPCType<Abominable>(),
		];
	}
	public override void SetDefaults()
	{
		NPC.BossBar = ModContent.GetInstance<GlacialBossBar>();
		NPC.aiStyle = -1;
		NPC.lifeMax = 90000;
#if LOW_HEALTH
		NPC.lifeMax = 5000;
#endif
		NPC.defense = 80;
		NPC.damage = 50;
		NPC.width = 125;
		NPC.height = 125;
		NPC.knockBackResist = 0.0f;
		NPC.boss = true;

		NPC.HitSound = new($"{nameof(PathOfTerraria)}/Assets/Sounds/HitEffects/FleshHit", 3) { Volume = 0.85f, Pitch = -0.25f, PitchVariance = 0.3f, MaxInstances = 5 };
		NPC.DeathSound = new($"{nameof(PathOfTerraria)}/Assets/Sounds/HitEffects/PainedScreechSplatter", 3) { Volume = 1.00f, Pitch = -0.25f, Identifier = $"{Name}Death" };

		NPC.TryEnableComponent<NPCMovement>(e =>
		{
			e.Data.Friction = (0f, 0f);
		});
		NPC.TryEnableComponent<NPCAnimations>(e =>
		{
			e.BaseFrame = new SpriteFrame(1, 1) with { PaddingX = 0, PaddingY = 0 };
			e.SpriteOffset = new(-16, -80);
			e.ManualInvoke = true;
		});
		NPC.TryEnableComponent<NPCTargeting>();
		NPC.TryEnableComponent<NPCAttacking>(e =>
		{
			e.Data.ManualInitiation = true;
			e.Data.NoGravityLength = 0;
		});
		NPC.TryEnableComponent<NPCTeleports>(e =>
		{
			// General
			e.Data.Movement = TeleportType.Interpolated;
			e.Data.Disappear = (0, 5);
			e.Data.Reappear = (20, 25);
			e.Data.Invulnerability = (5, 20);
			e.Data.CooldownDamage = (0.5f, 0);
			e.Data.LightColor = Color.Red.ToVector3() * 3f;
			e.Data.DisappearSound = (0, SoundID.DD2_DarkMageAttack with { Volume = 1f, Pitch = -0.9f, PitchVariance = 0.1f });
			e.Data.Velocity = (null, new Vector2(-15, -10));
			// Triggers
			e.Data.TriggerIfEndangered = true;
			e.Data.TriggerAtDistance = (250, float.PositiveInfinity);
			// Placement
			e.Data.PlaceOriginAtTarget = true;
			e.Data.RequireLineOfSightOnExit = true;
			e.Data.RequiredTargetDistance = (210, 510);
			e.Data.RequiredTargetDistanceDiff = (320, float.PositiveInfinity);
			e.Data.BasePlacement.Area = new Rectangle() with { Width = 80, Height = 60 };
			e.Data.BasePlacement.OnGround = false;
			e.Data.BasePlacement.MinDistanceFromPlayers = 50;
			e.Data.BasePlacement.MinDistanceFromEnemies = 0;
		});
		NPC.TryEnableComponent<NPCVoice>(e =>
		{
			e.Data.PainSound = (ChanceX: 5, Style: new($"{nameof(PathOfTerraria)}/Assets/Sounds/HitEffects/PainedScreech", 3)
			{
				SoundLimitBehavior = SoundLimitBehavior.IgnoreNew,
				Pitch = -0.8f,
				PitchVariance = 0.2f,
				Identifier = $"{Name}Hit"
			});
		});
		NPC.TryEnableComponent<NPCHitEffects>(c =>
		{
			c.AddGore(new($"{PoTMod.ModName}/{nameof(BloodSplatSmall)}", 1));
			c.AddGore(new($"{PoTMod.ModName}/{nameof(BloodSplatMedium)}", 1));
			c.AddGore(new($"{PoTMod.ModName}/{nameof(BloodSplatLarge)}", 1));

			c.AddGore(new($"{PoTMod.ModName}/{nameof(BloodSplatSmall)}", 15, NPCHitEffects.OnDeath));
			c.AddGore(new($"{PoTMod.ModName}/{nameof(BloodSplatMedium)}", 10, NPCHitEffects.OnDeath));
			c.AddGore(new($"{PoTMod.ModName}/{nameof(BloodSplatLarge)}", 5, NPCHitEffects.OnDeath));
		});

		Context ctx = new(NPC);
		SetupSegments(in ctx);
		SetupPaths(in ctx);

		// Initial cooldowns.
		spawnCooldown = 60 * 3;
	}

	public override void SendExtraAI(BinaryWriter writer)
	{
		writer.Write((byte)Phase);
		writer.Write((uint)globalCounter);
		writer.Write((ushort)grabCooldown);
		writer.Write((ushort)spawnCooldown);
		writer.Write((ushort)bulletAttackCounter);
		writer.Write((byte)Cutscene.Type);
		if (Cutscene.Type != 0)
		{
			writer.Write((ushort)Cutscene.Counter);
			writer.Write((ushort)Cutscene.Length);
		}
	}
	public override void ReceiveExtraAI(BinaryReader reader)
	{
		Phase = (PhaseType)reader.ReadByte();
		globalCounter = reader.ReadUInt32();
		grabCooldown = reader.ReadUInt16();
		spawnCooldown = reader.ReadUInt16();
		bulletAttackCounter = reader.ReadUInt16();

		// Only synchronize the cutscene if it is a different one from the one playing.
		// Do not interrupt ongoing cutscenes, let them end properly.
		if (((CutsceneType)reader.ReadByte()) is { } newType && newType != 0)
		{
			var cutscene = new CutsceneData(newType, reader.ReadUInt16(), reader.ReadUInt16());
			if (cutscene.Type != Cutscene.Type) { Cutscene = cutscene; }
		}
	}

	public override void AI()
	{
		Context ctx = new(NPC);

#if IK_PREVIEW
		NPC.Center = Main.MouseWorld;
#endif

		NPC.scale = 2.0f; //1.5f;

		// Reset overrides.
		ctx.Movement.Data.TargetOverride = null;

		globalCounter++;
		movementAnimation += MathF.Min(10f, NPC.velocity.Length());

		// Invoke behaviors.
		ctx.Targeting.ManualUpdate(new(NPC));
		PhaseLogic(in ctx);
		CutsceneLogic(in ctx);

		// Short-circuit if the death cutscene just ended.
		if (!NPC.active) { return; }

		IdentityLogic(in ctx);
		ResetOffsets(in ctx);
		SpawnMinions(in ctx);
		ShootProjectiles(in ctx);
		BodyMovement(in ctx);
		UpdateCollision(in ctx);
		UpdateBodyAngles(in ctx);
		UpdateSegments(in ctx);
		InitiateAttacks(in ctx);
		ctx.Teleports.ManualUpdate(new(NPC)
		{
			IsBusy = (Phase is PhaseType.Idle || CutsceneActive)
#if NO_TELEPORTS
			|| true
#endif
		});
		ctx.Attacking.ManualUpdate(new(NPC));
		// ctx.Movement.ManualUpdate(new(NPC));

		UpdateEffects(in ctx);
		ResetOffsets(in ctx);

#if IK_PREVIEW
		NPC.rotation = bodyAngle.Target = bodyAngle.Current = 0f;
		NPC.velocity = default;
#endif
	}

	private void PhaseLogic(in Context ctx)
	{
		// Phase-conditional data.
		if (Phase is PhaseType.First or PhaseType.Idle)
		{
			ctx.Movement.Data.MaxSpeed = 15;
			ctx.Movement.Data.Acceleration = 5;
			ctx.Teleports.Data.MaxCooldown = 900;
		}
		else if (Phase == PhaseType.Second)
		{
			ctx.Movement.Data.MaxSpeed = 15;
			ctx.Movement.Data.Acceleration = 5;
			ctx.Teleports.Data.MaxCooldown = 900;
		}

		// Start intro.
		if (Phase == PhaseType.Idle && !CutsceneActive)
		{
			const float introRange = 600;
			foreach (Player player in Main.ActivePlayers)
			{
				if (player.WithinRange(Center, introRange))
				{
					Phase = PhaseType.First;
					Cutscene = new(CutsceneType.Intro, 0, 250);
					break;
				}
			}
		}

		// Switch to second phase.
		if (!CutsceneActive && Phase == PhaseType.First
#if !FORCE_PHASE_II
		&& NPC.life < NPC.lifeMax * SecondPhaseHealthFactor
#endif
		)
		{
			Phase = PhaseType.Second;
			Cutscene = new(CutsceneType.Transformation, 0, 600);
			NPC.life = (int)(NPC.lifeMax * SecondPhaseHealthFactor);
		}

		// Faint glow in the second phase.
		if (!Main.dedServ && Phase is PhaseType.Second)
		{
			Lighting.AddLight(ctx.Center, new Vector3(0.3f, 0.2f, 1.0f) * 0.5f);
		}
	}
	private void IdentityLogic(in Context ctx)
	{
		_ = ctx;
		NPC.dontTakeDamage = Phase == PhaseType.Idle || CutsceneActive;
		NPC.boss = Phase != PhaseType.Idle;
		Music = Phase switch
		{
			PhaseType.First => phase1Music,
			PhaseType.Second => phase2Music,
			_ => -1,
		};
	}
	private void CutsceneLogic(in Context ctx)
	{
		if (Cutscene.Type == 0) { return; }

		bool initialize = !Cutscene.Initialized;
		bool justEnded = Cutscene.Counter >= Cutscene.Length;
		float lengthInSeconds = Cutscene.Length * TimeSystem.LogicDeltaTime;

		if (Cutscene.Type == CutsceneType.Intro)
		{
			if (initialize)
			{
				if (!Main.dedServ)
				{
					Main.musicFade[phase1Music] = 1f;
				}

				CameraCurios.Create(new()
				{
					Identifier = $"{nameof(GlacialBoss)}_Intro",
					Weight = 0.5f,
					LengthInSeconds = lengthInSeconds,
					FadeOutLength = 0.2f,
					Position = ctx.Center + new Vector2(0, 128),
					Range = new(Min: 1024, Max: 3072, Exponent: 1.5f),
					Zoom = +0.5f,
				});
				OverlayText.Create(new OverlayTextLine
				{
					AnimationLength = lengthInSeconds,
					Position = (new Vector2(0.5f, 0.25f), new Vector2(0f, -30f)),
					Text = this.GetLocalizedValue("OverlayTitle1"),
					Scale = Vector2.One * 1.10f,
					FontOverride = FontAssets.DeathText,
					PrimaryColor = Color.White,
					OutlineColor = Color.DarkRed,
					FadeInEffect = (0.00f, 0.20f),
					FadeOutEffect = (0.85f, 1.00f),
					ShakeEffect = (15f, Vector2.One * 3f),
				});
				OverlayText.Create(new OverlayTextLine
				{
					AnimationLength = lengthInSeconds,
					Position = (new Vector2(0.5f, 0.25f), new Vector2(0f, 20f)),
					Text = this.GetLocalizedValue("OverlaySubTitle1"),
					Scale = Vector2.One * 0.75f,
					FontOverride = FontAssets.DeathText,
					PrimaryColor = Color.White,
					OutlineColor = Color.DarkRed,
					FadeInEffect = (0.30f, 0.50f),
					FadeOutEffect = (0.85f, 1.00f),
					ShakeEffect = (15f, Vector2.One * 3f),
				});
				SoundEngine.PlaySound(style: new($"{nameof(PathOfTerraria)}/Assets/Sounds/Conflux/PyralisTransition1")
				{
					Volume = 0.35f,
				});
			}
		}
		else if (Cutscene.Type == CutsceneType.Transformation)
		{
			if (initialize)
			{
				// Kill minions.
				NPCUtil.KillAllWithType(minionTypes);

				if (!Main.dedServ)
				{
					// Skip music fade-in.
					Main.musicFade[phase1Music] = 0f;
					Main.musicFade[phase2Music] = 1f;

					CameraCurios.Create(new()
					{
						Identifier = $"{nameof(GlacialBoss)}_Phase2",
						Weight = 0.5f,
						LengthInSeconds = lengthInSeconds,
						FadeOutLength = 0.2f,
						Position = ctx.Center,
						Range = new(Min: 1024, Max: 3072, Exponent: 1.5f),
						Callback = PositionTracker(),
						Zoom = +0.5f,
					});
					SoundEngine.PlaySound(style: new($"{nameof(PathOfTerraria)}/Assets/Sounds/Conflux/PyralisTransition2")
					{
						Volume = 0.85f,
					});
				}
			}
			else if (justEnded)
			{
				// Prevent immediate teleportation & flame attacks.
				ctx.Teleports.Data.Cooldown.Set(600);
				bulletAttackCounter = 0;
			}

			// Effects.
			if (justEnded && !Main.dedServ)
			{
				OverlayText.Create(new OverlayTextLine
				{
					AnimationLength = lengthInSeconds,
					Position = (new Vector2(0.5f, 0.25f), new Vector2(0f, -30f)),
					Text = this.GetLocalizedValue("OverlayTitle2"),
					Scale = Vector2.One * 1.10f,
					FontOverride = FontAssets.DeathText,
					PrimaryColor = Color.White,
					OutlineColor = Color.DarkRed,
					FadeInEffect = (0.00f, 0.20f),
					FadeOutEffect = (0.85f, 1.00f),
					ShakeEffect = (15f, Vector2.One * 3f),
				});
				OverlayText.Create(new OverlayTextLine
				{
					AnimationLength = lengthInSeconds,
					Position = (new Vector2(0.5f, 0.25f), new Vector2(0f, 20f)),
					Text = this.GetLocalizedValue("OverlaySubTitle2"),
					Scale = Vector2.One * 0.75f,
					FontOverride = FontAssets.DeathText,
					PrimaryColor = Color.White,
					OutlineColor = Color.DarkRed,
					FadeInEffect = (0.30f, 0.50f),
					FadeOutEffect = (0.85f, 1.00f),
					ShakeEffect = (15f, Vector2.One * 3f),
				});

				SoundEngine.PlaySound(position: ctx.Center, style: new($"{nameof(PathOfTerraria)}/Assets/Sounds/Gore/SuperSplatter1")
				{
					Volume = 0.9f,
					MaxInstances = 3,
					PitchVariance = 0.2f,
				});

				foreach (ref Segment segment in segments.AsSpan())
				{
					for (int i = 0; i < 50; i++)
					{
						Vector2 pos = segment.Position + Main.rand.NextVector2Circular(32, 32);
						Vector2 vel = Main.rand.NextVector2Circular(25, 25);
						float scale = 1f + (Main.rand.NextFloat() * Main.rand.NextFloat());
						Dust.NewDustPerfect(pos, DustID.Blood, vel, Scale: scale);
					}

					for (int i = 0; i < 5; i++)
					{
						Vector2 pos = segment.Position + Main.rand.NextVector2Circular(64, 64);
						Vector2 vel = Main.rand.NextVector2Circular(10, 10);
						Gore.NewGorePerfect(NPC.GetSource_FromThis(), pos, vel, ModContent.GoreType<BloodSplatLarge>());
						Gore.NewGorePerfect(NPC.GetSource_FromThis(), pos, vel, ModContent.GoreType<BloodSplatMedium>());
						Gore.NewGorePerfect(NPC.GetSource_FromThis(), pos, vel, 135);
					}
				}
			}
		}
		else if (Cutscene.Type == CutsceneType.Death)
		{
			if (initialize)
			{
				// Kill minions.
				NPCUtil.KillAllWithType(minionTypes);

				if (!Main.dedServ)
				{
					// Stop music immediately.
					Main.musicFade[Main.curMusic] = 0f;

					// Play death audio.
					SoundEngine.PlaySound(position: ctx.Center, style: new($"{nameof(PathOfTerraria)}/Assets/Sounds/Conflux/PyralisDeath")
					{
						Volume = 1.20f,
						PauseBehavior = PauseBehavior.PauseWithGame,
					});

					CameraCurios.Create(new()
					{
						Identifier = $"{nameof(GlacialBoss)}_Death",
						Weight = 0.5f,
						LengthInSeconds = lengthInSeconds,
						FadeOutLength = 0.2f,
						Position = ctx.Center,
						Range = new(Min: 1024, Max: 3072, Exponent: 1.5f),
						Callback = PositionTracker(),
						Zoom = +0.5f,
					});
				}
			}
			else if (Cutscene.Counter == Cutscene.Length - 10)
			{
				SoundEngine.PlaySound(style: new($"{nameof(PathOfTerraria)}/Assets/Sounds/Conflux/PyralisTransition3")
				{
					Volume = 0.85f,
					PauseBehavior = PauseBehavior.PauseWithGame,
				});
			}
			else if (justEnded)
			{
				// Die for real.
				NPC.dontTakeDamage = false;
				// Do not synchronize this strike in any way to prevent interruptions.
				using (ValueOverride.Create(ref Main.netMode, NetmodeID.SinglePlayer))
				{
					NPC.StrikeInstantKill();
				}
			}

			// Death effects.
			if (justEnded && !Main.dedServ)
			{
				// Defeated text overlay.
				const float OverlayLength = 7.5f;
				(float, float) defeatedFadeIn = (0.25f, 0.30f);
				OverlayText.Create(new OverlayTextLine
				{
					AnimationLength = OverlayLength,
					Position = (new(0.5f, 0.25f), new(0f, 0f)),
					Text = this.GetLocalizedValue("OverlayDefeatedTitle"),
					FontOverride = FontAssets.DeathText,
					PrimaryColor = Color.White,
					OutlineColor = Color.DarkRed,

					FadeInEffect = (0.00f, 0.20f),
					FadeOutEffect = defeatedFadeIn,
				});
				OverlayText.Create(new OverlayTextLine
				{
					AnimationLength = OverlayLength,
					Position = (new(0.5f, 0.25f), new(0f, 0f)),
					Text = this.GetLocalizedValue("OverlayDefeatedTitle"),
					FontOverride = FontAssets.DeathText,
					PrimaryColor = ColorUtils.FromHexRgb(0x8338c0),
					OutlineColor = Color.Black,

					FadeInEffect = defeatedFadeIn,
					FadeOutEffect = (0.60f, 1.00f),
					ShakeEffect = (10f, Vector2.One * 3f),
				});
				OverlayText.Create(new OverlayTextLine
				{
					AnimationLength = OverlayLength,
					Position = (new(0.5f, 0.25f), new(0f, 60f)),
					Text = this.GetLocalizedValue("OverlayDefeatedSubTitle"),
					FontOverride = FontAssets.DeathText,
					PrimaryColor = ColorUtils.FromHexRgb(0x8338c0),
					OutlineColor = Color.Black,

					FadeInEffect = defeatedFadeIn,
					FadeOutEffect = (0.60f, 1.00f),
					ShakeEffect = (10f, Vector2.One * 3f),
				});

				SoundEngine.PlaySound(position: ctx.Center, style: new($"{nameof(PathOfTerraria)}/Assets/Sounds/Gore/SuperSplatter1")
				{
					Volume = 0.9f,
					MaxInstances = 3,
					Pitch = -0.5f,
					PitchVariance = 0.2f,
				});
			}
		}

		Cutscene.Initialized = true;
		if (justEnded) { Cutscene = default; }
		else { Cutscene.Counter++; }
	}

	private void UpdateCollision(in Context ctx)
	{
		_ = ctx;
		if (segments.Length == 0)
		{
			NPC.width = 32;
			NPC.height = 32;
			return;
		}
		
		ref readonly Segment s0 = ref segments[0];
		var aabb = new Vector4
		(
			s0.Position.X - s0.Length,
			s0.Position.Y - s0.Length,
			s0.Position.X + s0.Length,
			s0.Position.Y + s0.Length
		);

		foreach (ref Segment segment in segments.AsSpan())
		{
			aabb.X = MathF.Min(aabb.X, segment.Position.X - segment.Length);
			aabb.Y = MathF.Min(aabb.Y, segment.Position.Y - segment.Length);
			aabb.Z = MathF.Max(aabb.Z, segment.Position.X + segment.Length);
			aabb.W = MathF.Max(aabb.W, segment.Position.Y + segment.Length);
		}

		NPC.position.X = aabb.X;
		NPC.position.Y = aabb.Y;
		NPC.width = (int)MathF.Ceiling(aabb.Z - aabb.X);
		NPC.height = (int)MathF.Ceiling(aabb.W - aabb.Y);
	}
	public override bool CanBeHitByNPC(NPC attacker)
	{
		return false;
	}
	public override bool? CanBeHitByItem(Player player, Item item)
	{
		return base.CanBeHitByItem(player, item);
	}
	public override bool? CanBeHitByProjectile(Projectile projectile)
	{
		Rectangle npcAabb = NPC.getRect();
		Rectangle projAabb = projectile.getRect();

		if (!projAabb.Intersects(npcAabb)) { return false; }
		if (!projectile.friendly) { return false; }

		foreach (ref Segment segment in segments.AsSpan())
		{
			var segAabb = new Rectangle
			(
				(int)(segment.Position.X - segment.Length),
				(int)(segment.Position.Y - segment.Length),
				(int)segment.Length,
				(int)segment.Length
			);

			if (segAabb.Intersects(projAabb))
			{
				return true;
			}
		}
		
		return false;
	}

	// Trigger death cutscene.
	public override bool CheckDead()
	{
		if (Cutscene.Type != CutsceneType.Death || Cutscene.Counter != Cutscene.Length)
		{
			if (Cutscene.Type != CutsceneType.Death) { Cutscene = new(CutsceneType.Death, 0, 240); }
			Phase = PhaseType.Second;
			NPC.dontTakeDamage = true;
			NPC.life = 1;
			return false;
		}

		return true;
	}

	private void BodyMovement(in Context ctx)
	{
		NPC.noGravity = true;
		NPC.noTileCollide = true;
		NPC.GravityMultiplier = MultipliableFloat.One * 0.5f;

		if (segments.Length == 0) { return; }

		ref Vector2 position = ref segments[0].Position;
		ref Vector2 velocity = ref segments[0].Velocity;

		// Manually apply gravity.
		velocity.Y += NPC.gravity;

		bool hasTarget = NPC.HasValidTarget;
		NPCAimedTarget target = NPC.GetTargetData(ignorePlayerTankPets: true);
		Vector2 targetPos = target.Center;
		Vector2 walkPos = targetPos;
		bool fixedPosition = false;

		const float approachDistance = 1000;
		if (Phase == PhaseType.First && hasTarget)
		{
			float strafingAngle = globalCounter * -0.004f * MathHelper.TwoPi;
			Vector2 shiftPos = walkPos + strafingAngle.ToRotationVector2() * approachDistance * new Vector2(1.0f, 1.0f);
			var shiftStart = Vector2Int.Clamp(position.ToTileCoordinates(), default, new(Main.maxTilesX - 1, Main.maxTilesY - 1));
			var shiftEnd = Vector2Int.Clamp(shiftPos.ToTileCoordinates(), default, new(Main.maxTilesX - 1, Main.maxTilesY - 1));
			walkPos = shiftPos;
		}

#if FOLLOW_MOUSE
		walkPos = Main.MouseWorld;
#endif

		float maxSpeed = ctx.Movement.Data.MaxSpeed;
		float acceleration = ctx.Movement.Data.Acceleration * TimeSystem.LogicDeltaTime;
		Vector2 targetDir = position.SafeDirection(targetPos, Vector2.UnitY);
		Vector2 walkDir = position.SafeDirection(walkPos, Vector2.UnitY);

		if (Phase == PhaseType.Idle && !CutsceneActive)
		{
			velocity = default;
			position = walkPos + new Vector2(0, -8);
			maxSpeed = 0.0f;
			acceleration = 0f;
		}
		else if (CutsceneActive)
		{
			velocity *= 0.98f;

			if (Cutscene.Type is CutsceneType.Intro)
			{
				maxSpeed *= 0.25f;
				acceleration *= 0.5f;
				walkDir *= -1;
			}
			else if (Cutscene.Type is CutsceneType.Transformation)
			{
				maxSpeed *= 0.125f;
				acceleration *= 0.9f;
			}
			else if (Cutscene.Type is CutsceneType.Death)
			{
				maxSpeed *= 0.125f;
				acceleration *= 0.5f;
			}
		}

#if DEBUG && DEBUG_GIZMOS
		DebugUtils.DrawStringInWorld("X walkPos", walkPos, Color.Aqua);
#endif

		Point16 tilePoint = position.ToTileCoordinates16();
		bool submerged = TileUtils.InWorld(tilePoint) && (TileUtils.HasSolid(Main.tile[tilePoint]) || Main.tile[tilePoint].LiquidAmount > 0);
		bool qFunction = false;

		if (!submerged)
		{
			acceleration *= 0.15f;
			qFunction = true;
		}
		else
		{
			acceleration *= 0.4f;
			maxSpeed *= 1.8f;
		}

		if (fixedPosition)
		{
			position = walkPos;
			velocity = default;
		}
		// Accelerate.
		else if (maxSpeed > 0f)
		{
			if (qFunction)
			{
				velocity = MovementUtils.DirAccelQ(velocity, walkDir, maxSpeed, acceleration);
			}
			else
			{
				velocity = MovementUtils.DirAccel(velocity, walkDir, maxSpeed, acceleration);
			}
		}

		// Apply hard & soft speed capping.
		if (velocity != default)
		{
			float hardCap = maxSpeed * 2.50f;
			float softCap = maxSpeed * 1.00f;
			float softLoss = 8 * TimeSystem.LogicDeltaTime;
			velocity = MovementUtils.ApplyHardSpeedCap(velocity, hardCap);
			velocity = MovementUtils.ApplySoftSpeedCap(velocity, softCap, softLoss);
		}

		// Do not change the direction if the target is close or if an attack is ongoing.
		if (hasTarget && !ctx.Attacking.Active && Math.Abs(targetPos.X - position.X) > 100)
		{
			NPC.direction = targetDir.X >= 0f ? 1 : -1;
		}
		// Face left when idle.
		else if (Phase is PhaseType.Idle || Cutscene.Type is CutsceneType.Intro)
		{
			NPC.direction = -1;
		}

		// Always have the same visual direction.
		NPC.spriteDirection = -1;

		// Prevent navigation-driven movement, allow going through platforms.
		ctx.Movement.Data.InputOverride = new()
		{
			FallThroughPlatforms = true,
		};
	}

	private void UpdateBodyAngles(in Context ctx)
	{
		// Update head rotation. Body-relative.
		const float HeadAngleMul = 0.5f;
		bool lookAtTarget = NPC.HasValidTarget && (Phase > 0 || CutsceneActive);
		bool targetInFront = ((ctx.Center.X - ctx.TargetCenter.X) >= 0 ? 1 : -1) != NPC.spriteDirection;
		bool isFlipped = NPC.spriteDirection < 0;
		float flip01 = isFlipped ? 1f : 0f;
		headAngle.Target = 0f;
		headAngle.Target = lookAtTarget && targetInFront ? ctx.TargetCenter.AngleFrom(ctx.Center) : flip01 * MathHelper.Pi;
		headAngle.Target += lookAtTarget && !targetInFront ? (MathHelper.PiOver4 * NPC.spriteDirection) : 0f;
		headAngle.Target += (flip01 * MathHelper.Pi);
		headAngle.Target = MathUtils.LerpRadians(headAngle.Target, 0f, 1f - HeadAngleMul);
		headAngle.Current = MathUtils.LerpRadians(headAngle.Current, headAngle.Target, 2.5f * TimeSystem.LogicDeltaTime);
	}

	private void InitiateAttacks(in Context ctx)
	{
#if FRIENDLY
		return;
#endif
		bool swingingDuringIntro = Cutscene.Type == CutsceneType.Intro && Cutscene.Counter > (Cutscene.Length - 80);

		if (Phase == PhaseType.Idle) { return; }
		if (Cutscene.Type != CutsceneType.None && !swingingDuringIntro) { return; }
		if (!NPC.HasValidTarget && !swingingDuringIntro) { return; }

		bulletAttackCounter++;

		if (ctx.Attacking.Active) { return; }

		Vector2 targetCenter = ctx.Targeting.GetTargetCenter(NPC);
		float distance = targetCenter.Distance(ctx.Center);
		AttackType nextAttackType = attackType switch
		{
			_ => AttackType.None,
		};
		float attackRange = nextAttackType switch
		{
			_ => 0,
		};

		bool inRange = distance < attackRange;
		bool ignoreRange = false;

		// Occassional bullethell bursts.
		int bulletAttackRate = (int)(60 * (Phase is PhaseType.First ? 6 : 5));
		if (bulletAttackCounter > bulletAttackRate)
		{
			nextAttackType = AttackType.BulletHell;
			ignoreRange = true;
		}

		if (nextAttackType != AttackType.None && (inRange || ignoreRange) && ctx.Attacking.TryStarting(new(NPC)))
		{
			attackType = nextAttackType;

			// Reset dynamic data.
			ctx.Attacking.Data.Hitbox = default;
			ctx.Attacking.Data.Damage = default;
			ctx.Attacking.Data.Dash = default;
			ctx.Attacking.Data.Slash = null;
			ctx.Attacking.Data.Sounds = [];
			// Configure dynamic data.
			if (attackType is AttackType.Bite)
			{
				ctx.Attacking.Data.LengthInTicks = 70;
				ctx.Attacking.Data.CooldownLength = 20;
				ctx.Attacking.Data.AimLag = ((new(0.0f), new(0.15f)), (0f, 0.99f));
				ctx.Attacking.Data.Hitbox = (new(340, 340), new(48, 48), new(0, 0));
				ctx.Attacking.Data.Damage = (35, 43, DamageInstance.EnemyAttackFilter);
				ctx.Attacking.Data.Dash = (5, 20, new(29, 24));
				ctx.Attacking.Data.Movement = (0.0f, 1.00f, 1.00f);

				if (!Main.dedServ)
				{
					ctx.Attacking.Data.Slash = (ModContent.Request<Texture2D>($"{nameof(PathOfTerraria)}/Assets/Misc/Slash"), null, Color.IndianRed);
					ctx.Attacking.Data.Sounds =
					[
						(0, new($"{nameof(PathOfTerraria)}/Assets/Sounds/Conflux/PyralisThrow", 3) { MaxInstances = 3, Volume = 0.9f, Pitch = -0.5f, PitchVariance = 0.2f }),
					];
				}
			}
			else if (attackType is AttackType.BulletHell)
			{
				bulletAttackCounter = 0;
				ctx.Attacking.Data.LengthInTicks = 120;
				ctx.Attacking.Data.CooldownLength = 30;
				ctx.Attacking.Data.Damage = (75, 75, (EntityKind)0);
				ctx.Attacking.Data.Movement = (0.0f, 1.00f, 1.00f);

				if (!Main.dedServ)
				{
					bool poweredUp = Phase is PhaseType.Second;
					var style = new SoundStyle($"{nameof(PathOfTerraria)}/Assets/Sounds/Conflux/CryoStalkerCast")
					{
						Volume = 2.00f,
						Pitch = -0.6f,
						PitchVariance = 0.2f,
						MaxInstances = 3,
						PauseBehavior = PauseBehavior.PauseWithGame,
					};
					ctx.Attacking.Data.Sounds =
					[
						(0, style),
						(poweredUp ? (ushort)15 : ushort.MaxValue, style),
						(poweredUp ? (ushort)30 : ushort.MaxValue, style),
					];
				}
			}

			if (swingingDuringIntro)
			{
				ctx.Attacking.Data.Hitbox = default;
			}
		}
	}

	private void ShootProjectiles(in Context ctx)
	{
		if (Phase is PhaseType.Idle) { return; }
		if (CutsceneActive) { return; }
		if (!ctx.Attacking.Active) { return; }
		if (attackType != AttackType.BulletHell) { return; }

		if (!Main.dedServ)
		{
			float midTick = MathHelper.Lerp(ctx.Attacking.Data.Damage.Start, ctx.Attacking.Data.Damage.End, 0.1f);
			float intensity = MathF.Pow(MathUtils.DistancePower(MathF.Abs(ctx.Attacking.Progress - midTick), 0, 60), 2f);
			Lighting.AddLight(ctx.Center, Color.Cyan.ToVector3() * 3f * intensity);
		}

		if (Main.netMode == NetmodeID.MultiplayerClient) { return; }

		float progressFactor = ctx.Attacking.Progress / (float)ctx.Attacking.Data.LengthInTicks;
		var source = (IEntitySource)NPC.GetSource_FromThis();
		
		int tickBase = ctx.Attacking.Data.Damage.Start;
		bool poweredUp = true; //Phase is PhaseType.Second;
		int numInstances = poweredUp ? 3 : 1;
		int numProjectiles = poweredUp ? 12 : 24;
		for (int instance = 0; instance < numInstances; instance++)
		{
			if (ctx.Attacking.Progress != tickBase + (instance * 15)) { continue; }

			float instanceFactor = instance / (float)(numInstances);
			float baseAngle = instance * MathHelper.PiOver4;
			float baseSpeed = MathHelper.Lerp(3, 5, 1f - instanceFactor);

			for (int i = 0; i < numProjectiles; i++)
			{
				float iFactor = i / (float)numProjectiles;
				int tick = (int)(ctx.Attacking.Data.LengthInTicks * iFactor);
			
				var projPos = (Vector2)Center;
				float projSpeed = (float)(baseSpeed + Main.rand.NextFloat(0, 3));
				var projVel = (Vector2)((baseAngle + (iFactor * MathHelper.TwoPi)).ToRotationVector2() * projSpeed);
				int projType = ModContent.ProjectileType<CryoStalkerIcicle>();
				Projectile.NewProjectileDirect(source, projPos, projVel, projType, 1, 1f);
 			}
		}
	}

	private void SpawnMinions(in Context ctx)
	{
#if FRIENDLY || NO_MINIONS
		return;
#endif
		if (Phase == PhaseType.Idle) { return; }
		if (CutsceneActive) { return; }

		if (spawnCooldown > 0)
		{
			spawnCooldown--;
			return;
		}

		if (Main.netMode == NetmodeID.MultiplayerClient) { return; }

		int numShamblings = 0, numStalkers = 0, numAbominables = 0;
		foreach (NPC npc in Main.ActiveNPCs)
		{
			if (npc.type == ModContent.NPCType<Shambling>()) { numShamblings++; }
			else if (npc.type == ModContent.NPCType<CryoStalker>()) { numStalkers++; }
			else if (npc.type == ModContent.NPCType<Abominable>()) { numAbominables++; }
		}

		int type = 0;
		int numSteps = Phase is PhaseType.Second ? 2 : 1;
		for (int i = 1; i <= numSteps; i++)
		{
			if (numShamblings < i * 1.5f) { type = ModContent.NPCType<Shambling>(); break; }
			else if (numStalkers < i) { type = ModContent.NPCType<CryoStalker>(); break; }
			else if (numAbominables < i * 0.9f) { type = ModContent.NPCType<Abominable>(); break; }
		}

		if (type > 0)
		{
			var npc = NPC.NewNPCDirect(NPC.GetSource_FromThis(), (int)ctx.Center.X, (int)ctx.Center.Y, type);
			SoundEngine.PlaySound(position: ctx.Center, style: new($"{nameof(PathOfTerraria)}/Assets/Sounds/Enemies/ResurrectBloody", 3)
			{
				Pitch = -0.5f,
				PitchVariance = 0.03f,
				MaxInstances = 3,
			});

			spawnCooldown = 250;
		}
	}

	private void ResetOffsets(in Context ctx)
	{
		_ = ctx;
		NPC.gfxOffY = 0;
	}

	private void UpdateEffects(in Context ctx)
	{
		if (Main.dedServ) { return; }

		// Bias the camera towards the boss.
		bool fightStarted = Phase is not PhaseType.Idle && Cutscene.Type is not CutsceneType.Intro;
		bool introStarted = Phase is PhaseType.Idle && Cutscene.Type is not CutsceneType.Intro;
		if (fightStarted || !introStarted)
		{
			CameraCurios.Create(new()
			{
				Identifier = $"{nameof(GlacialBoss)}{(fightStarted ? "" : "_Idle")}",
				Weight = fightStarted ? 0.2f : 0.3f,
				Zoom = fightStarted ? 0f : 0.5f,
				LengthInSeconds = 1f,
				Position = Center + (!introStarted ? new Vector2(0, +128) : default),
				Range = fightStarted ? new(Min: 1000, Max: 2000, Exponent: 2.0f) : new(Min: 300, Max: 1000, Exponent: 1.5f),
				Callback = fightStarted ? PositionTracker() : null,
			});
		}

		// Movement sound.
		{
			bool quiet = Cutscene.Type == CutsceneType.Death;
			var loopSound = new SoundStyle($"{nameof(PathOfTerraria)}/Assets/Sounds/Gore/FleshLoopChaotic") { Volume = 0.1f, IsLooped = true, PauseBehavior = PauseBehavior.PauseWithGame };
			float halfStep = 1f * TimeSystem.LogicDeltaTime;
			float target = quiet ? 0 : MathUtils.Clamp01(NPC.velocity.Length() * 0.1f);
			float intensity = movementSound.Intensity = MathUtils.StepTowards(MathHelper.Lerp(movementSound.Intensity, target, halfStep), target, halfStep);
			float volume = MathHelper.Lerp(quiet ? 0 : 0.75f, 1.00f, intensity);
			float pitch = MathHelper.Lerp(-0.9f, +0.2f, intensity);
			SoundUtils.UpdateLoopingSound(ref movementSound.Handle, ctx.Center, volume, pitch, loopSound, _ => Main.npc[NPC.whoAmI] is { active: true } n && n == NPC);
		}
	}

	private void SetupSegments(in Context ctx)
	{
		_ = ctx;
		
		Vector2 nextPosition = segments.Length == 0 ? NPC.Center : segments[0].Position;
		
		segments = new Segment[200];
		
		for (int segmentIndex = 0; segmentIndex < segments.Length; segmentIndex++)
		{
			ref Segment segment = ref segments[segmentIndex];
			
			if (segmentIndex == 0)
			{
				segment = new()
				{
					TexturePath = $"{Texture}_Head",
					Frame = new SpriteFrame(1, 1) { PaddingX = 0, PaddingY = 0 },
					Position = nextPosition,
					BaseRotation = MathHelper.ToRadians(30),
					Length = 30,
				};
			}
			else
			{
				segment = new()
				{
					TexturePath = $"{Texture}_Body",
					Frame = new SpriteFrame(1, 2) { PaddingX = 0, PaddingY = 0 },
					Position = nextPosition,
					BaseRotation = -MathHelper.PiOver2,
					Length = 30,
				};

				if (segmentIndex >= segments.Length * 0.85f)
				{
					segment.Frame.CurrentRow = 1;
				}
			}

			nextPosition += new Vector2(0, segment.Length);
		}
	}

	private void UpdateSegments(in Context ctx)
	{
#if DEBUG && DEBUG_KEYS
		if (Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.N))
		{
			globalCounter = 0;
			SetupSegments(in ctx);
			SetupPaths(in ctx);
		}
#endif

		// Gravity & ground collision.
		foreach (ref Segment segment in segments.AsSpan(start: 1, length: 1))
		{
			Point16 tilePoint = segment.Position.ToTileCoordinates16();
			bool isSubmerged = TileUtils.InWorld(tilePoint) && TileUtils.HasSolid(Main.tile[tilePoint]);
			if (isSubmerged)
			{
				segment.Velocity = default;
				continue;
			}
			
			// segment.Velocity += Vector2.UnitY * NPC.gravity * 1.0f;
		}

		// Pull segments together.
		if (segments.Length != 0)
		{
			// segments[0].Position = ctx.Center;
			segments[0].Rotation = NPC.velocity.ToRotation();// + segments[0].BaseRotation;
			// segments[0].Velocity = NPC.velocity;
		}
		for (int segmentIndex = 1; segmentIndex < segments.Length; segmentIndex++)
		{
			ref Segment curr = ref segments[segmentIndex + 0];
			ref Segment prev = ref segments[segmentIndex - 1];

			//float step = MathF.Sin(((globalCounter * TimeSystem.LogicDeltaTime * 1f) + (segmentIndex / 3f)) / 10f);
			//Vector2 sideDir = (curr.Rotation).ToRotationVector2();
			//curr.Velocity = sideDir * step * 2;
			curr.Velocity = default;

			float prevLength = prev.Length * NPC.scale;
			float distance = prev.Position.Distance(curr.Position);
			Vector2 direction = distance > 0 ? ((curr.Position - prev.Position) / distance) : Vector2.UnitY;
			if (distance > prevLength)
			{
				// curr.Position = prev.Position + (direction * prevLength);
				curr.Velocity += (prev.Position + (direction * prevLength)) - curr.Position;

				//	// Pull velocity.
				//	// Sometimes even non-zero vectors return NaNs when normalized.
				//	if (curr.Velocity != default && Vector2.Normalize(curr.Velocity) is { } velDir && !velDir.HasNaNs())
				//	{
				//		Debug.Assert(!curr.Velocity.HasNaNs());
				//
				//		float velDot = Vector2.Dot(velDir, direction);
				//		var newVelocity = Vector2.Lerp(prev.Velocity, curr.Velocity, MathUtils.Clamp01(velDot + 0.5f));
				//		Debug.Assert(!newVelocity.HasNaNs());
				//		curr.Velocity = newVelocity;
				//	}
			}
			else
			{
			}

			// DebugUtils.DrawStringInWorld($"vel: [{curr.Velocity.X:0}, {curr.Velocity.Y:0}]", curr.Position, Color.Azure);
			// DebugUtils.DrawStringInWorld($"depth: {curr.Depth:0.00}]", curr.Position, Color.Azure);
		}
		
		// Follow path.
		Gradient<AnimationKey> path = testPath;
		bool inNativeArena = SubworldSystem.IsActive<GlacialRealm>();
		Vector2 arenaCenter = (inNativeArena ? GlacialRealm.ArenaCenter.Get()?.ToWorldCoordinates() : null)
			?? (new Point16(Main.maxTilesX / 2, Main.maxTilesY / 2).ToWorldCoordinates());
			
		for (int segmentIndex = 0; segmentIndex < segments.Length; segmentIndex++)
		{
			float timeOffset = segmentIndex * -1.8f;
			float GetStep(uint tick)
			{
				return MathUtils.Modulo((tick * 10f * TimeSystem.LogicDeltaTime) + timeOffset, testPath.Range.Max);
			}

			float prevStep = GetStep(globalCounter - 1);
			float currStep = GetStep(globalCounter + 0);
			AnimationKey prevSample = path.GetValue(prevStep);
			AnimationKey currSample = path.GetValue(currStep);

			ref Segment segment = ref segments[segmentIndex];
			segment.Position = arenaCenter + prevSample.Position;
			segment.Velocity = currSample.Position - prevSample.Position;
			segment.Depth = currSample.Depth;
		}

		// Apply segment velocity.
		foreach (ref Segment segment in segments.AsSpan())
		{
			segment.Position += segment.Velocity;
		}

		// Calculate rotations
		for (int segmentIndex = 1; segmentIndex < segments.Length; segmentIndex++)
		{
			ref Segment curr = ref segments[segmentIndex + 0];
			ref Segment prev = ref segments[segmentIndex - 1];

			Vector2 direction = prev.Position.SafeDirection(curr.Position, Vector2.UnitY);
			curr.Rotation = direction.ToRotation() + curr.BaseRotation;
		}

		// Effects.
		foreach (ref Segment segment in segments.AsSpan())
		{
			Point16 tilePoint = segment.Position.ToTileCoordinates16();
			bool isSubmerged = TileUtils.InWorld(tilePoint) && TileUtils.HasSolid(Main.tile[tilePoint]);
			bool wasSubmerged = segment.WasSubmerged;

			if (isSubmerged != wasSubmerged && !Main.dedServ)
			{
				SoundEngine.PlaySound(SoundID.WormDig, segment.Position);
				Main.instance.CameraModifiers.Add
				(
					new PunchCameraModifier(segment.Position, segment.Velocity * 0.5f, 1, 3, 45, 1600, "WormSurfacing" + Main.rand.Next(100))
				);
			}

			segment.WasSubmerged = isSubmerged;
		}
	}
	private void SetupPaths(in Context ctx)
	{
		_ = ctx;
		
		// Path positions are relative to the ArenaCenter.
		float x1 = -400;
		float x2 = +400;
		float yMul = -400;
		int numTurnsToTop = 20;
		int resolution = 400;
		testPath = new Gradient<AnimationKey>(capacity: resolution);

		AnimationKey GetKey(float factor)
		{
			float step = MathUtils.PingPong(factor, 1);
			float turn = factor * numTurnsToTop * 2;
			Func<float, float> sine = factor <= 0.5f ? MathUtils.Sin01 : MathUtils.Cos01;
			
			var pos = new Vector2
			(
				MathHelper.Lerp(x1, x2, sine(step * numTurnsToTop)),
				step * numTurnsToTop * yMul
			);
			float depth = 0.5f - sine(step * numTurnsToTop * 1f + 1.0f);
			// float depth = ((int)(turn + 0.5f)) % 2 == 0 ? 0.5f : -0.5f;

			return new AnimationKey(pos, depth);
		}

		for (int i = 0; i < resolution; i++)
		{
			// float depth = (i / 1) % 2 == 0 ? 0.5f : -0.5f;
			// var pos = new Vector2(i % 2 == 0 ? x1 : x2, i * yMul);
			// float step = MathUtils.PingPong(i / (float)resolution, 1f);
			// testPath.Add(i, new AnimationKey(pos, +depth));
			// testPath.Add(i, new AnimationKey(pos, -depth));
			testPath.Add(i, GetKey(i / (float)resolution));
		}
		// for (int i = numStepsToTop; i < numStepsToTop * 2; i++)
		// {
		// 	float depth = (i / 1) % 2 == 0 ? 0.5f : -0.5f;
		// 	var pos = new Vector2(i % 2 != 0 ? x1 : x2, (numStepsToTop - (i - numStepsToTop)) * yMul);
		// 	testPath.Add(i, new AnimationKey(pos, +depth));
		// 	testPath.Add(i, new AnimationKey(pos, -depth));
		// }
		// {
		// 	{ 00, new Vector2(x1, 0 * yMul) },
		// 	{ 01, new Vector2(x2, 1 * yMul) },
		// 	{ 02, new Vector2(x1, 2 * yMul) },
		// 	{ 03, new Vector2(x2, 3 * yMul) },
		// 	{ 04, new Vector2(x1, 4 * yMul) },
		// 	{ 05, new Vector2(x2, 5 * yMul) },
		// 	{ 06, new Vector2(x1, 6 * yMul) },
		// 	{ 07, new Vector2(x2, 7 * yMul) },
		// 	{ 08, new Vector2(x1, 7 * yMul) },
		// 	{ 09, new Vector2(x2, 6 * yMul) },
		// 	{ 10, new Vector2(x1, 5 * yMul) },
		// 	{ 11, new Vector2(x2, 4 * yMul) },
		// 	{ 12, new Vector2(x1, 3 * yMul) },
		// 	{ 13, new Vector2(x2, 2 * yMul) },
		// 	{ 14, new Vector2(x1, 1 * yMul) },
		// 	{ 15, new Vector2(x2, 0 * yMul) },
		// };
		// testPath.Normalize();
	}

	private SpriteAnimation? PickAnimation(in Context ctx)
	{
		Vector2 vel = NPC.position - NPC.oldPosition;

		return ctx.Animations.Current switch
		{
			_ => animIdle,
		};
	}

	public override void FindFrame(int frameHeight)
	{
		if (!NPC.active) { return; }

		Context ctx = new(NPC);
		ctx.Animations.Advance();
		ctx.Animations.Set(PickAnimation(in ctx));
	}

	public override void DrawBehind(int index)
	{
		Main.instance.DrawCacheNPCsMoonMoon.Add(index);
	}
	public override bool PreDraw(SpriteBatch sb, Vector2 screenPos, Color drawColor)
	{
		Context ctx = new(NPC);

		if (NPC.IsABestiaryIconDummy)
		{
			return true;
		}

		SpriteBatchArgs sbArgs = sb.GetArguments();

		bool drawingBackground = NPCUtil.GetCachedDrawContext() == NPCCachedDraw.BehindWalls;

		// Render segments back-to-front.
		for (int segmentIndex = segments.Length - 1; segmentIndex >= 0; segmentIndex--)
		{
			ref Segment segment = ref segments[segmentIndex];

			bool isBackground = segment.Depth < 0f;
			if (isBackground != drawingBackground) { continue; }

			if (!AssetUtils.AsyncValue(segment.TexturePath, ref segment.TextureCache, out Texture2D segTex)) { continue; }

			var segFrame = (Rectangle)segment.Frame.GetSourceRectangle(segTex);
			var segOrigin = (Vector2)(segFrame.Size() * 0.5f);
			var segPos = (Vector2)segment.Position;
			var segColor = (Color)Lighting.GetColor(segPos.ToTileCoordinates());
			float depthEffect = MathUtils.Clamp01(-segment.Depth * 2f);
			segColor = segColor.MultiplyRGB(new Color(Vector3.One * (1f - (depthEffect * 0.45f))));
			
			float usedScale = NPC.scale * MathHelper.Lerp(1f, 0.95f, depthEffect);
			using var _ = ValueOverride.Create(ref NPC.scale, usedScale);
			
			ctx.Animations.Render(NPC, segTex, segFrame, screenPos, segColor, center: segPos, origin: segOrigin, rotation: segment.Rotation);

			string glowPath = $"{segment.TexturePath}_Glow";
			if (ModContent.HasAsset(glowPath) && AssetUtils.AsyncValue(glowPath, ref segment.GlowCache, out Texture2D segGlow))
			{
				ctx.Animations.Render(NPC, segGlow, segFrame, screenPos, Color.White, center: segPos, origin: segOrigin, rotation: segment.Rotation);
			}
		}

		return false;
	}

	// Hide 'name: life/lifeMax' mousetext.
	public override bool PreHoverInteract(bool mouseIntersects)
	{
		return false;
	}
	// Hide in-world healthbar.
	public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
	{
		return false;
	}

	public SoundUpdateCallback AudioTracker()
	{
		var tracker = new NPCTracker(NPC);
		
		bool Method(ActiveSound sound)
		{
			if (tracker.Npc() is NPC { ModNPC: GlacialBoss boss })
			{
				sound.Position = boss.Center;
				return true;
			} 

			return false;
		}
		
		return Method;
	}
	public Func<Vector2?> PositionTracker()
	{
		var tracker = new NPCTracker(NPC);
		
		Vector2? Method()
		{
			return tracker.Npc() is NPC { ModNPC: GlacialBoss boss } ? boss.Center : null;
		}
		
		return Method;
	}
}


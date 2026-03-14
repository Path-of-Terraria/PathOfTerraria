// #define IK_PREVIEW

using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using PathOfTerraria.Common.AI;
using PathOfTerraria.Common.NPCs.Components;
using PathOfTerraria.Common.NPCs.Effects;
using PathOfTerraria.Common.Utilities;
using PathOfTerraria.Common.World.Utilities;
using PathOfTerraria.Content.Gores;
using PathOfTerraria.Core.Camera;
using PathOfTerraria.Core.IK;
using PathOfTerraria.Core.Interface;
using PathOfTerraria.Core.Lighting;
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
#pragma warning disable IDE0042 // Deconstruct variable declaration

namespace PathOfTerraria.Content.Conflux;

internal sealed class InfernalBossBar : ModBossBar
{
	public override bool PreDraw(SpriteBatch spriteBatch, NPC npc, ref BossBarDrawParams drawParams)
	{
		return npc.ModNPC is InfernalBoss { Phase: > 0 };
	}
}

/// <summary> Non-moving flames that block areas for a bit of time. </summary>
internal sealed class InfernalFlames : ModProjectile
{
	private ref float Progress => ref Projectile.ai[0];
	private Vector2 StartPos
	{
		get => new(Projectile.localAI[1], Projectile.localAI[2]);
		set => (Projectile.localAI[1], Projectile.localAI[2]) = (value.X, value.Y);
	}
	private Vector2 TargetPos
	{
		get => new(Projectile.ai[1], Projectile.ai[2]);
		set => (Projectile.ai[1], Projectile.ai[2]) = (value.X, value.Y);
	}

	public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.Flames}";

	public override void SetStaticDefaults()
	{
		Main.projFrames[Type] = 7;
	}
	public override void SetDefaults()
	{
		Projectile.aiStyle = -1;
		Projectile.damage = 50;
		Projectile.Size = new(32);
		Projectile.timeLeft = 300;
		Projectile.hostile = true;
		Projectile.friendly = false;
		Projectile.aiStyle = -1;
		Projectile.usesLocalNPCImmunity = true;
		Projectile.localNPCHitCooldown = -1;
	}

	public override void AI()
	{
		if (StartPos == default)
		{
			StartPos = Projectile.Center;
		}

		if (Progress >= 1f)
		{
			Projectile.Kill();
			return;
		}

		if (TargetPos != default)
		{
			float step = MathUtils.Clamp01(Progress / 0.05f);
			step = 1f - MathF.Pow(1f - step, 2f);
			Projectile.Center = Vector2.Lerp(StartPos, TargetPos, step);
		}

		Projectile.scale = Utils.Remap(Progress, 0.0f, 0.1f, 0.25f, 1.00f);
		Projectile.velocity *= 0.99f;

		int frameCount = Main.projFrames[Type];
		Progress += TimeSystem.LogicDeltaTime / 8f;
		Projectile.frame = (int)MathF.Floor(Progress / frameCount);

		Projectile.rotation = Progress * 40f * MathHelper.TwoPi * (Projectile.whoAmI % 2 == 0 ? 1 : -1);
		float radSnap = MathHelper.ToRadians(5f);
		Projectile.rotation = MathF.Floor(Projectile.rotation * radSnap) / radSnap;

		Lighting.AddLight(Projectile.Center, Color.OrangeRed.ToVector3() * (0.3f + 1f - Progress));
	}

	public override bool? CanDamage()
	{
		return Progress >= 0.06f && Progress <= 0.90f;
	}

	public override void OnHitPlayer(Player target, Player.HurtInfo info)
	{
		target.AddBuff(BuffID.OnFire, 180);
	}
	public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
	{
		target.AddBuff(BuffID.OnFire, 180);
	}

	private static readonly Color[] layerColors =
	[
		new(255, 80, 20, 200),
		new(255, 255, 20, 070),
		new(255, 80, 20, 100),
		new(80, 80, 80, 100),
	];
	public override bool PreDraw(ref Color lightColor)
	{
		// [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "DrawProj_Flamethrower")]
		// static extern void DrawFlame(Main? main, Projectile p);
		// DrawFlame(null, Projectile);

		int numLayers = layerColors.Length;
		int numFrames = Main.projFrames[Projectile.type];
		int numLayeredFrames = numFrames + numLayers;

		Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
		Vector2 worldPos = Projectile.Center.ToPoint().ToVector2();
		Vector2 screenPos = worldPos - Main.screenPosition;

		const float animMul = 8f;
		int baseFrame = (int)Utils.Remap(Progress, 0f, 1f, 0, numLayeredFrames * animMul);
		// Loop/wrap up until a certain point.
		if (baseFrame >= numFrames)
		{
			int loopEnd = (int)(numLayeredFrames * (animMul - 1));
			baseFrame = baseFrame < loopEnd ? (2 + (baseFrame % 4) * 1) : (baseFrame - loopEnd + 2);
		}

		for (int i = 0; i < numLayers; i++)
		{
			int frame = baseFrame - i;
			if (frame < 0 || frame >= numFrames) { continue; }

			Color color = layerColors[i];
			Rectangle srcRect = texture.Frame(verticalFrames: numFrames, frameY: frame);
			Main.EntitySpriteDraw(texture, screenPos, srcRect, color, Projectile.rotation, srcRect.Size() * 0.5f, Projectile.scale, 0, 0f);
		}

		return false;
	}
}

internal sealed class InfernalBoss : ModNPC
{
	public enum PhaseType : byte
	{
		Idle = 0,
		Bladewielder = 1,
		Abomination = 2,
	}
	[Flags]
	public enum LimbRole
	{
		None = 0,
		Walking = 1 << 0,
		Wielding = 1 << 1,
		EntityGrabbing = 1 << 2,
	}
	public record struct Limb()
	{
		public required IKLimb IK;
		public required string TexturePath;
		public required LimbRole Role;
		public Vector2 StartPosition;
		public Vector2 TargetPosition;
		public Asset<Texture2D>? TextureCache;
		public EntityRef EntityAttachment;
		public Point16? TileAttachment;
		public Vector2? BladeAttachment;
		public bool CanBeGibbed = true;
		public bool IsGibbed;
		public float Layer;
		public (int Stage, float Progress) Animation;
		public bool PlayedSound;
		public bool LiftInAnimation;
		public PhaseType MinPhaseToGib;

		public void ResetState()
		{
			Animation = default;
			LiftInAnimation = false;
			PlayedSound = false;
			StartPosition = IK.Target;
			EntityAttachment = default;
			TileAttachment = default;
			BladeAttachment = default;
		}

		public void SetGibbed(bool value)
		{
			IsGibbed = value;
			foreach (ref IKSegment segment in IK.Segments.AsSpan())
			{
				segment.Frame.CurrentRow = (byte)(value ? 1 : 0);
			}
		}
	}

	public enum Flag : byte
	{
	}
	private enum AttackType
	{
		None,
		Slash,
		Reverse,
		Stab,
	}
	private readonly struct Context(NPC npc)
	{
		public Vector2 Center { get; } = npc.Center;
		public Vector2 TargetCenter { get; } = npc.GetGlobalNPC<NPCTargeting>().GetTargetCenter(npc);
		public NPCMovement Movement { get; } = npc.GetGlobalNPC<NPCMovement>();
		// public NPCNavigation Navigation { get; } = npc.GetGlobalNPC<NPCNavigation>();
		public NPCTargeting Targeting { get; } = npc.GetGlobalNPC<NPCTargeting>();
		public NPCAttacking Attacking { get; } = npc.GetGlobalNPC<NPCAttacking>();
		public NPCAnimations Animations { get; } = npc.GetGlobalNPC<NPCAnimations>();
		public NPCFootsteps Footsteps { get; } = npc.GetGlobalNPC<NPCFootsteps>();
	}
	public record struct BladeTransform(Vector2 Position, float Rotation);

	private static readonly InverseKinematics.Config ikCfg = new()
	{
		RadianQuantization = MathHelper.ToRadians(4f),
	};
	private static readonly SpriteAnimation animIdle = new() { Id = "idle", Frames = [0], Speed = 3f };
	private static Asset<Texture2D>? helmetTexture;
	private static Asset<Texture2D>? bodyTexture;
	private static Asset<Texture2D>? robeTexture;
	private static Asset<Texture2D>? bladeBaseTexture;
	private static Asset<Texture2D>? bladeGlowTexture;
	private static Asset<Effect>? bladeShader;

	public ref Flag Flags => ref Unsafe.As<float, Flag>(ref NPC.ai[3]);
	public ref float Counter => ref NPC.ai[2];

	public ref BladeTransform Blade => ref BladeHistory[0];
	public BladeTransform[] BladeHistory = new BladeTransform[10];
	private (Vector2 Pos, float Rot) bladeVelocity;

	/// <summary> World-relative body angle. </summary>
	private (float Target, float Current) bodyAngle;
	/// <summary> Body-relative head angle. </summary>
	private (float Target, float Current) headAngle;
	private float movementAnimation;
	private AttackType attackType;
	private Limb[] limbs = [];
	private (int NextIndex, float Cooldown) limbMovement = (0, 0f);
	private (SlotId Handle, float Intensity) movementSound;
	private (SlotId Handle, float Intensity) bladeBurnSound;
	private ushort grabCooldown;
	private ushort spawnCooldown;
	private ushort voluntaryAttackCounter;
	private ushort introCounter;
	private float strafingAngle;
	private int lifeOld;
	private readonly HashSet<Point16> flamePoints = [];

	public PhaseType Phase = PhaseType.Idle;

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
		NPC.BossBar = ModContent.GetInstance<InfernalBossBar>();
		NPC.aiStyle = -1;
		NPC.lifeMax = 75000;
		NPC.defense = 90;
		NPC.damage = 50;
		NPC.width = 125;
		NPC.height = 125;
		NPC.knockBackResist = 0.0f;
		NPC.boss = true;
		NPC.lavaImmune = true;

		NPC.HitSound = new($"{nameof(PathOfTerraria)}/Assets/Sounds/HitEffects/FleshHit", 3) { Volume = 0.4f, Pitch = -0.5f, MaxInstances = 5 };
		NPC.DeathSound = SoundID.NPCDeath23 with { Pitch = -0.85f, PitchVariance = 0.15f, Identifier = $"{Name}Death" };

		// NPC.TryEnableComponent<NPCNavigation>(e =>
		// {
		// 	e.JumpRange = new(25, 50);
		// });
		NPC.TryEnableComponent<NPCMovement>(e =>
		{
			// e.Data.MaxSpeed = 0f;
			e.Data.MaxSpeed = 6.5f;
			e.Data.Acceleration = 4f;
			e.Data.Friction = (8f, 1f);
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
			e.Data.LengthInTicks = 95;
			e.Data.CooldownLength = 00;
			e.Data.NoGravityLength = 0;
			e.Data.InitiationRange = new(450, 192);
			e.Data.AimLag = ((new(0.0f), new(0.3f)), (0f, 0.99f));
			// e.Data.InitiationRange = new(1024, 512);
			e.Data.Dash = (50, 65, new(9f, 1f));
			e.Data.Damage = (45, 60, DamageInstance.EnemyAttackFilterWithInfighting);
			e.Data.Hitbox = (new(350, 350), new(+290, +100), new(+0, +0));
			e.Data.Movement = (0.0f, 0.80f, 0.95f);

			if (!Main.dedServ)
			{
				e.Data.Sounds =
				[
					// (20, new($"{nameof(PathOfTerraria)}/Assets/Sounds/Conflux/CryoStalkerCast") {
					// 	MaxInstances = 2,
					// 	Volume = 0.3f,
					// 	Pitch = -0f,
					// 	PitchVariance = 0.15f,
					// }),
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
			e.Data.NoJumpSounds = true;
			e.Data.NoLandSounds = true;
			// e.Data.Frames = new() { { "walk", [0, 3] } };
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
			e.Data.PainSound = (9, SoundID.NPCHit56 with { Pitch = 0f, PitchVariance = 0.5f, Identifier = "ShamanHit" });
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

		Context ctx = new(NPC);
		SetupLimbs(in ctx);
		SetupBlade(in ctx);

		// Initial cooldowns.
		spawnCooldown = 60 * 3;
	}

	public override void AI()
	{
		Context ctx = new(NPC);

#if IK_PREVIEW
		NPC.Center = Main.MouseWorld;
#endif

		// Reset overrides.
		ctx.Movement.Data.TargetOverride = null;
		ctx.Attacking.Data.ManualInitiation = true;

		// Invoke behaviors.
		ctx.Targeting.ManualUpdate(new(NPC));
		PhaseLogic(in ctx);
		ResetOffsets(in ctx);
		SpawnMinions(in ctx);
		UpdateBlade(in ctx);
		SpreadBladeFlames(in ctx);
		LimbWielding(in ctx);
		LimbGrabbing(in ctx);
		LimbMovement(in ctx);
		LimbFlailing(in ctx);
		BodyMovement(in ctx);
		UpdateBodyAngles(in ctx);
		UpdateLimbs(in ctx);
		GibLimbs(in ctx);
		InitiateAttacks(in ctx);
		// ctx.Teleports.ManualUpdate(new(NPC));
		ctx.Attacking.ManualUpdate(new(NPC));
		ctx.Movement.ManualUpdate(new(NPC));

		UpdateEffects(in ctx);
		BladeEffects(in ctx);
		ResetOffsets(in ctx);

#if IK_PREVIEW
		NPC.rotation = bodyAngle.Target = bodyAngle.Current = 0f;
		NPC.velocity = default;
#endif
	}

	private void PhaseLogic(in Context ctx)
	{
		_ = ctx;

		const float introSeconds = 4.5f;
		const float transformSeconds = 10f;
		const int introTicks = (int)(introSeconds * 60);
		const int transformTicks = (int)(transformSeconds * 60);

		int phase1Music = MusicID.OtherworldlyBoss1;
		int phase2Music = MusicID.OtherworldlyPlantera;

		bool inIntro = Phase == PhaseType.Idle && introCounter != 0;
		if (Phase == PhaseType.Idle && introCounter == 0)
		{
			bool startIntro = false;
			const float introRange = 512f;
			const float introRangeSqr = introRange * introRange;
			foreach (Player player in Main.ActivePlayers)
			{
				if (player.DistanceSQ(ctx.Center) < introRangeSqr)
				{
					startIntro = true;
				}
			}

			if (startIntro)
			{
				inIntro = true;
				introCounter = introTicks;

				if (!Main.dedServ)
				{
					Main.musicFade[phase1Music] = 1f;
				}

				CameraCurios.Create(new()
				{
					Identifier = $"{nameof(InfernalBoss)}_Intro",
					Weight = 0.5f,
					LengthInSeconds = introSeconds,
					FadeOutLength = 0.2f,
					Position = ctx.Center,
					Range = new(Min: 1024, Max: 3072, Exponent: 1.5f),
					Zoom = +0.5f,
				});
				OverlayText.Create(new OverlayTextLine
				{
					AnimationLength = introSeconds,
					Position = (new Vector2(0.5f, 0.25f), new Vector2(0f, -30f)),
					Text = "Pyralis",
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
					AnimationLength = introSeconds,
					Position = (new Vector2(0.5f, 0.25f), new Vector2(0f, 20f)),
					Text = "The Fallen",
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
		if (Phase == PhaseType.Idle && introCounter != 0 && --introCounter == 0)
		{
			Phase = PhaseType.Bladewielder;
			inIntro = false;
		}

		if (Phase == PhaseType.Bladewielder && NPC.life < NPC.lifeMax * 0.5f)
		{
			Phase = PhaseType.Abomination;
			introCounter = transformTicks;
			
			// Throw the sword.
			bladeVelocity.Pos = new Vector2(0, -21) + Main.rand.NextVector2Circular(5f, 3f);
			bladeVelocity.Rot = MathHelper.TwoPi * 20f * (Main.rand.NextBool() ? +1 : -1);

			// Skip music fade-in.
			if (!Main.dedServ)
			{
				Main.musicFade[phase1Music] = 0f;
				Main.musicFade[phase2Music] = 1f;
			}
			
			CameraCurios.Create(new()
			{
				Identifier = $"{nameof(InfernalBoss)}_Phase2",
				Weight = 0.5f,
				LengthInSeconds = transformTicks / 60,
				FadeOutLength = 0.2f,
				Position = ctx.Center,
				Range = new(Min: 1024, Max: 3072, Exponent: 1.5f),
				Callback = new NPCTracker(NPC).Center,
				Zoom = +0.5f,
			});
			SoundEngine.PlaySound(style: new($"{nameof(PathOfTerraria)}/Assets/Sounds/Conflux/PyralisTransition2")
			{
				Volume = 0.85f,
			});
		}

		if (Phase == PhaseType.Abomination)
		{
			if (introCounter != 0 && --introCounter == 0)
			{
				// Grow new limbs!
				SetupLimbs(in ctx);

				OverlayText.Create(new OverlayTextLine
				{
					AnimationLength = transformSeconds,
					Position = (new Vector2(0.5f, 0.25f), new Vector2(0f, -30f)),
					Text = "Pyralis",
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
					AnimationLength = transformSeconds,
					Position = (new Vector2(0.5f, 0.25f), new Vector2(0f, 20f)),
					Text = "The Cursed",
					Scale = Vector2.One * 0.75f,
					FontOverride = FontAssets.DeathText,
					PrimaryColor = Color.White,
					OutlineColor = Color.DarkRed,
					FadeInEffect = (0.30f, 0.50f),
					FadeOutEffect = (0.85f, 1.00f),
					ShakeEffect = (15f, Vector2.One * 3f),
				});

				SoundEngine.PlaySound(position: ctx.Center, style: new($"{nameof(PathOfTerraria)}/Assets/Sounds/Gore/SuperSplatter")
				{
					Volume = 0.9f,
					MaxInstances = 3,
					PitchVariance = 0.2f,
				});

				// Temp effects.
				foreach (Limb limb in limbs)
				{
					Vector2 logicalBodyCenter = ctx.Center;
					Vector2 limbPos = limb.IK.GetPosition(logicalBodyCenter, rotation: bodyAngle.Current, xDir: NPC.spriteDirection);
				
					for (int i = 0; i < 50; i++)
					{
						Vector2 pos = limbPos + Main.rand.NextVector2Circular(32, 32);
						Vector2 vel = Main.rand.NextVector2Circular(25, 25);
						float scale = 1f + (Main.rand.NextFloat() * Main.rand.NextFloat());
						Dust.NewDustPerfect(pos, DustID.Blood, vel, Scale: scale);
					}

					for (int i = 0; i < 5; i++)
					{
						Vector2 pos = limbPos + Main.rand.NextVector2Circular(64, 64);
						Vector2 vel = Main.rand.NextVector2Circular(10, 10);
						Gore.NewGorePerfect(NPC.GetSource_FromThis(), pos, vel, ModContent.GoreType<BloodSplatLarge>());
						Gore.NewGorePerfect(NPC.GetSource_FromThis(), pos, vel, ModContent.GoreType<BloodSplatMedium>());
						Gore.NewGorePerfect(NPC.GetSource_FromThis(), pos, vel, 135);
					}
				}
			}
			
			Lighting.AddLight(ctx.Center, new Vector3(1.0f, 0.2f, 0.8f) * 0.5f);
		}

		NPC.dontTakeDamage = Phase == PhaseType.Idle;
		NPC.boss = Phase > PhaseType.Idle || inIntro;
		Music = Phase switch
		{
			PhaseType.Bladewielder => phase1Music,
			_ when inIntro => phase1Music,
			PhaseType.Abomination => phase2Music,
			_ => -1,
		};
	}

	private void BodyMovement(in Context ctx)
	{
		int numAttached = limbs.Count(l => l.TileAttachment != null);
		int numWalkingLimbs = limbs.Count(l => l.Role.HasFlag(LimbRole.Walking) && !l.IsGibbed);
		bool isClimbing = numAttached >= Math.Clamp(numWalkingLimbs / 2, 1, 4) || NPC.lavaWet;
		NPC.noGravity = isClimbing;

		if (isClimbing)
		{
			bool hasTarget = NPC.HasValidTarget && (Phase > 0 || introCounter != 0);
			NPCAimedTarget target = NPC.GetTargetData(ignorePlayerTankPets: true);
			Vector2 targetPos = target.Center;
			Vector2 walkPos = targetPos;

			// Go to the center of the arena if lacking a target in the infernal realm.
			if (!hasTarget)
			{
				bool inNativeArena = SubworldSystem.IsActive<InfernalRealm>();
				targetPos = inNativeArena ? InfernalRealm.ArenaCenter.ToWorldCoordinates() : ctx.Center;
				walkPos = targetPos;
			}

			// In phase I, when using the sword, keep some distance away from the target and also rotate around it.
			const float approachDistance = 290;
			if (Phase == PhaseType.Bladewielder && hasTarget)
			{
				Vector2 shiftPos = walkPos + strafingAngle.ToRotationVector2() * approachDistance * new Vector2(1.0f, 0.5f);
				var shiftStart = Vector2Int.Clamp(ctx.Center.ToTileCoordinates(), default, new(Main.maxTilesX - 1, Main.maxTilesY - 1));
				var shiftEnd = Vector2Int.Clamp(shiftPos.ToTileCoordinates(), default, new(Main.maxTilesX - 1, Main.maxTilesY - 1));
				foreach (Vector2Int point in new GeometryUtils.BresenhamLine(shiftStart, shiftEnd))
				{
					if (Main.tile[point] is { HasTile: true } t && Main.tileSolid[t.TileType] && !Main.tileSolidTop[t.TileType])
					{
						shiftPos = ((Point)point).ToWorldCoordinates();
						break;
					}
				}
				walkPos = shiftPos;
				strafingAngle += -0.2f * MathHelper.TwoPi * TimeSystem.LogicDeltaTime;
			}

			// Stand up, but only if that doesn't prevent the creature from getting into tight spaces.
			Vector2Int sizeInTiles = (NPC.Size * new Vector2(0.9f, 2.2f)).ToTileCoordinates();
			if (TileUtils.TryFitRectangleIntoTilemap(walkPos.ToTileCoordinates() - new Point(0, 1), sizeInTiles, out Vector2Int adjustedPoint))
			{
				walkPos = (adjustedPoint + (sizeInTiles * 0.5f)).ToWorldCoordinates();
			}

			float maxSpeed = ctx.Movement.Data.MaxSpeed;
			float acceleration = ctx.Movement.Data.Acceleration * TimeSystem.LogicDeltaTime;
			Vector2 targetDir = ctx.Center.DirectionTo(targetPos);
			Vector2 walkDir = ctx.Center.DirectionTo(walkPos);

			if (ctx.Attacking.Active)
			{
				acceleration *= 0.65f;
				ctx.Attacking.Data.Dash.Velocity = new(20f, 10f);
			}
			else if (Phase == PhaseType.Abomination)
			{
				acceleration *= 1.1f;
			}

			if (Phase == PhaseType.Idle && introCounter == 0)
			{
				NPC.velocity = default;
				NPC.Center = walkPos;
				maxSpeed = 0f;
				acceleration = 0f;
			}
			else if (introCounter != 0)
			{
				NPC.velocity *= 0.98f;

				if (Phase == PhaseType.Idle)
				{
					maxSpeed *= 0.25f;
					acceleration *= 0.5f;
					walkDir *= -1;
				}
				else
				{
					maxSpeed *= 0.125f;
					acceleration *= 0.5f;
				}
			}

			NPC.velocity = MovementUtils.DirAccelQ(NPC.velocity, walkDir, maxSpeed, acceleration);

			float speedCap = ctx.Movement.Data.MaxSpeed * 1.1f;
			if (NPC.velocity.Length() > speedCap)
			{
				NPC.velocity = NPC.velocity.SafeNormalize(Vector2.UnitX) * speedCap;
			}

			// Do not change the direction if the target is close or if an attack is ongoing.
			if (hasTarget && !ctx.Attacking.Active && Math.Abs(targetPos.X - ctx.Center.X) > 100)
			{
				NPC.direction = targetDir.X >= 0f ? 1 : -1;
			}
			// Phase left when idle.
			else if (Phase == 0)
			{
				NPC.direction = -1;
				NPC.spriteDirection = NPC.direction;
			}

			// Prevent navigation-driven movement, allow going through platforms.
			ctx.Movement.Data.InputOverride = new()
			{
				FallThroughPlatforms = true,
			};
		}
		else
		{
			NPC.direction = -1;
			// Prevent navigation-driven movement, disallow going through platforms.
			ctx.Movement.Data.InputOverride = new()
			{
				FallThroughPlatforms = false,
			};
		}
	}

	private void UpdateBodyAngles(in Context ctx)
	{
		int numAttached = 0;
		Vector2 averageDir = default;
		Vector2 logicalBodyCenter = NPC.Center;
		Vector2 visualBodyCenter = logicalBodyCenter + new Vector2(0f, NPC.gfxOffY);

		// Compute ground normal.
		foreach (ref Limb limb in limbs.AsSpan())
		{
			if (limb.TileAttachment != null)
			{
				numAttached++;
				averageDir += limb.TargetPosition.DirectionFrom(visualBodyCenter);
			}
		}
		// Compute average angle.
		float averageAngle = numAttached == 0 ? 0f : ((averageDir / numAttached).ToRotation());
		averageAngle = !float.IsNaN(averageAngle) ? averageAngle : 0f;

		if (Phase == PhaseType.Idle)
		{
			averageAngle = 0f;
		}

		// Update body rotation. Rotate towards zero faster than from it.
		float limbNormal = averageAngle;
		bodyAngle.Target = limbNormal * 0.30f;
		bool anglingBodyTowardsZero = MathF.Abs(bodyAngle.Target) < MathF.Abs(bodyAngle.Current);
		bodyAngle.Current = MathUtils.LerpRadians(bodyAngle.Current, bodyAngle.Target, (anglingBodyTowardsZero ? 2.10f : 0.80f) * TimeSystem.LogicDeltaTime);
		NPC.rotation = bodyAngle.Current;
		Debug.Assert(!float.IsNaN(bodyAngle.Target));
		Debug.Assert(!float.IsNaN(bodyAngle.Current));
		Debug.Assert(!float.IsNaN(NPC.rotation));

		// Update head rotation. Body-relative.
		const float HeadAngleMul = 0.5f;
		bool targetInFront = ((ctx.Center.X - ctx.TargetCenter.X) >= 0 ? 1 : -1) != NPC.spriteDirection;
		bool isFlipped = NPC.spriteDirection < 0;
		float flip01 = isFlipped ? 1f : 0f;
		headAngle.Target = 0f;
		headAngle.Target = NPC.HasValidTarget && targetInFront ? ctx.TargetCenter.AngleFrom(ctx.Center) : flip01 * MathHelper.Pi;
		headAngle.Target += NPC.HasValidTarget && !targetInFront ? (MathHelper.PiOver4 * NPC.spriteDirection) : 0f;
		headAngle.Target += (flip01 * MathHelper.Pi);
		headAngle.Target = MathUtils.LerpRadians(headAngle.Target, 0f, 1f - HeadAngleMul);
		headAngle.Current = MathUtils.LerpRadians(headAngle.Current, headAngle.Target, 2.5f * TimeSystem.LogicDeltaTime);
	}

	private void InitiateAttacks(in Context ctx)
	{
		if (Phase != PhaseType.Bladewielder && !(Phase == PhaseType.Idle && introCounter is > 0 and <= 180)) { return; }
		if (!NPC.HasValidTarget) { return; }
		if (ctx.Attacking.Active) { return; }

		// Do not start attacks when holding some entity.
		if (limbs.Any(l => l.EntityAttachment.IsValid)) { return; }

		Vector2 targetCenter = ctx.Targeting.GetTargetCenter(NPC);
		float distance = targetCenter.Distance(ctx.Center);
		AttackType nextAttackType = attackType switch
		{
			AttackType.Slash => AttackType.Reverse,
			AttackType.Reverse => AttackType.Stab,
			_ => AttackType.Slash
		};
		float attackRange = nextAttackType switch
		{
			AttackType.Stab => 610,
			_ => 400,
		};

		// An attack will be triggered regardless of range if enough time passes without cathing up.
		// This is done to spread flames in order to interfere with the player's escape.
		const int voluntaryAttackRate = (int)(60 * 1.1f);
		bool inRange = distance < attackRange;
		bool ignoreRange = voluntaryAttackCounter > voluntaryAttackRate;
		if ((inRange || ignoreRange) && ctx.Attacking.TryStarting(new(NPC)))
		{
			attackType = nextAttackType;
			voluntaryAttackCounter = 0;

			if (attackType != AttackType.Stab)
			{
				ctx.Attacking.Data.AimLag = ((new(0.01f), new(0.27f)), (0f, 0.95f));
				ctx.Attacking.Data.Hitbox = (new(350, 350), new(+280, +280), new(+0, +0));
			}
			else
			{
				ctx.Attacking.Data.AimLag = ((new(0.01f), new(0.06f)), (0f, 0.95f));
				ctx.Attacking.Data.Hitbox = (new(200, 200), new(+500, +500), new(+0, +0));
			}
		}
		else
		{
			voluntaryAttackCounter = (ushort)Math.Min(voluntaryAttackCounter + 1, ushort.MaxValue - 2);
		}
	}

	private void SpawnMinions(in Context ctx)
	{
		if (Phase == PhaseType.Idle) { return; }
		if (introCounter != 0) { return; }

		if (spawnCooldown > 0)
		{
			spawnCooldown--;
			return;
		}

		if (Main.netMode == NetmodeID.MultiplayerClient) { return; }

		// Find the limb to use for grabbing.
		if (Array.FindIndex(limbs, l => l.Role.HasFlag(LimbRole.EntityGrabbing) && !l.EntityAttachment.IsValid) is int limbIndex && limbIndex < 0) { return; }
		ref Limb limb = ref limbs[limbIndex];

		int numSavages = 0, numSchemers = 0, numTyrants = 0, numShamans = 0;
		foreach (NPC npc in Main.ActiveNPCs)
		{
			if (npc.type == ModContent.NPCType<FallenSavage>()) { numSavages++; }
			else if (npc.type == ModContent.NPCType<FallenSchemer>()) { numSchemers++; }
			else if (npc.type == ModContent.NPCType<FallenTyrant>()) { numTyrants++; }
			else if (npc.type == ModContent.NPCType<FallenShaman>()) { numShamans++; }
		}

		int type = 0;
		int numSteps = Phase == PhaseType.Abomination ? 3 : 1;
		for (int i = 1; i <= numSteps; i++)
		{
			if (numSavages < i * 1.5f) { type = ModContent.NPCType<FallenSavage>(); break; }
			else if (numSchemers < i) { type = ModContent.NPCType<FallenSchemer>(); break; }
			// else if (numTyrants < i) { type = ModContent.NPCType<FallenTyrant>(); break; }
			else if (numShamans < i * 0.9f) { type = ModContent.NPCType<FallenShaman>(); break; }
		}

		if (type > 0)
		{
			var npc = NPC.NewNPCDirect(NPC.GetSource_FromThis(), (int)ctx.Center.X, (int)ctx.Center.Y, type);
			SoundEngine.PlaySound(position: ctx.Center, style: new($"{nameof(PathOfTerraria)}/Assets/Sounds/Enemies/ResurrectBloody", 3)
			{
				PitchVariance = 0.03f,
				MaxInstances = 3,
			});

			// Grab the spawned NPC.
			limb.ResetState();
			limb.EntityAttachment = (EntityRef)npc;

			spawnCooldown = 300;
			grabCooldown = 120;
		}
	}

	private void ResetOffsets(in Context ctx)
	{
		_ = ctx;
		movementAnimation += MathF.Min(10f, NPC.velocity.Length());
		NPC.gfxOffY = MathF.Sin(((float)Main.timeForVisualEffects / 25f) + (movementAnimation / 40f)) * 6f;

#if IK_PREVIEW
		NPC.gfxOffY = 0f;
#endif
	}
	private void UpdateEffects(in Context ctx)
	{
		if (Main.dedServ) { return; }

		// Bias the camera towards the boss.
		CameraCurios.Create(new()
		{
			Identifier = nameof(InfernalBoss),
			Weight = 0.2f,
			LengthInSeconds = 1f,
			Position = ctx.Center,
			Range = new(Min: 1024, Max: 2000, Exponent: 2.0f),
			Callback = new NPCTracker(NPC).Center,
		});

		// Movement sound.
		{
			var loopSound = new SoundStyle($"{nameof(PathOfTerraria)}/Assets/Sounds/Gore/FleshLoopChaotic") { Volume = 0.1f, IsLooped = true, PauseBehavior = PauseBehavior.PauseWithGame };
			float halfStep = 1f * TimeSystem.LogicDeltaTime;
			float target = MathUtils.Clamp01(NPC.velocity.Length() * 0.1f);
			float intensity = movementSound.Intensity = MathUtils.StepTowards(MathHelper.Lerp(movementSound.Intensity, target, halfStep), target, halfStep);
			float volume = MathHelper.Lerp(0.75f, 1.00f, intensity);
			float pitch = MathHelper.Lerp(-0.9f, +0.2f, intensity);
			SoundUtils.UpdateLoopingSound(ref movementSound.Handle, ctx.Center, volume, pitch, loopSound, _ => Main.npc[NPC.whoAmI] is { active: true } n && n == NPC);
		}
	}

	private void BladeEffects(in Context ctx)
	{
		if (Main.dedServ) { return; }
		if (Phase != PhaseType.Bladewielder) { return; }

		// Blade tile collision effects.
		Vector2 forward = Blade.Rotation.ToRotationVector2();
		Vector2 sideways = forward.RotatedBy(MathHelper.PiOver2);
		bool sufficientlyDeepInGround = false;
		int numLevelsSliced = 0;
		const float start = 80;
		const float end = 400;
		for (float extent = start; extent < end; extent += 32)
		{
			bool slicedAtThisLevel = false;
			for (float offset = -32; offset <= 32; offset += 32)
			{
				Vector2 point = Blade.Position + (forward * extent) + (sideways * offset);
				Point16 tilePoint = point.ToTileCoordinates16();
				(int x, int y) = (tilePoint.X, tilePoint.Y);

				if (x < 1 || y < 1 || x >= Main.maxTilesX - 1 || y >= Main.maxTilesY - 1) { continue; }

				point = tilePoint.ToWorldCoordinates(8, 0);

				if (!WorldUtilities.SolidTile(x, y))
				{
					// Illuminate free spaces.
					Lighting.AddLight(point, Color.Orange.ToVector3());
					continue;
				}

				if (!IsTileSuitableForFlames(x, y)) { continue; }
				
				Dust.NewDustPerfect(point, DustID.Torch, Scale: 2f, Alpha: 128);
				slicedAtThisLevel = true;
			}

			if (slicedAtThisLevel && ++numLevelsSliced > 3)
			{
				sufficientlyDeepInGround = true;
			}
		}

		// Blade ground collision sound.
		{
			// Calculate distance to any part of the blade.
			float minDistance = float.PositiveInfinity;
			for (int i = 0; i <= 400; i += 100)
			{
				Vector2 bladeEnd = Blade.Position + Blade.Rotation.ToRotationVector2() * 400f;
				minDistance = MathF.Min(minDistance, MathF.Min(bladeEnd.Distance(CameraSystem.ScreenCenter), bladeEnd.Distance(Main.LocalPlayer.Center)));
			}

			var loopSound = new SoundStyle($"{nameof(PathOfTerraria)}/Assets/Sounds/Conflux/PyralisBladeBurn") { Volume = 0.95f, IsLooped = true, PauseBehavior = PauseBehavior.PauseWithGame };
			float halfStep = 8 * TimeSystem.LogicDeltaTime;
			float distanceTarget = MathF.Pow(MathUtils.DistancePower(minDistance, 64, 1024), 2.3f);
			float slicingTarget = MathUtils.Clamp01(numLevelsSliced / 3f) * 0.5f;
			float target = MathF.Max(distanceTarget, slicingTarget);
			float intensity = bladeBurnSound.Intensity = MathUtils.StepTowards(MathHelper.Lerp(bladeBurnSound.Intensity, target, halfStep), target, halfStep);
			float volume = MathHelper.Lerp(0.01f, 1f, intensity);
			float pitch = sufficientlyDeepInGround ? +0.5f : +0.0f;
			SoundUtils.UpdateLoopingSound(ref bladeBurnSound.Handle, ctx.Center, volume, pitch, loopSound, _ => Main.npc[NPC.whoAmI] is { active: true } n && n == NPC);
		}
	}
	private void SpreadBladeFlames(in Context ctx)
	{
		if (Main.netMode == NetmodeID.MultiplayerClient) { return; }
		if (Phase != PhaseType.Bladewielder) { return; }

		if (!ctx.Attacking.DealingDamage)
		{
			flamePoints.Clear();
			return;
		}

		Vector2 forward = Blade.Rotation.ToRotationVector2();
		Vector2 sideways = forward.RotatedBy(MathHelper.PiOver2);
		(float Start, float End, float Step) extents = attackType != AttackType.Stab ? (300, 900, 32) : (200, 1100, 32);
		(float Start, float End, float Step) offsets = attackType != AttackType.Stab ? (-32, +32, 32) : (-64, +64, 32);

		for (float extent = extents.Start; extent < extents.End; extent += extents.Step)
		{
			for (float offset = offsets.Start; offset <= offsets.End; offset += offsets.Step)
			{
				Vector2 point = Blade.Position + (forward * extent) + (sideways * offset);
				Point16 tilePoint = point.ToTileCoordinates16();
				(int x, int y) = (tilePoint.X, tilePoint.Y);

				if (x < 1 || y < 1 || x >= Main.maxTilesX - 1 || y >= Main.maxTilesY - 1) { continue; }
				if (!WorldUtilities.SolidTile(x, y)) { continue; }
				if (!IsTileSuitableForFlames(x, y)) { continue; }
				if (!flamePoints.Add(tilePoint)) { continue; }

				point = tilePoint.ToWorldCoordinates(8, 0);

				int projType = ModContent.ProjectileType<InfernalFlames>();
				Vector2 projPos = Blade.Position;
				Vector2 projVel = default;
				float ai0 = Main.rand.NextFloat(0.00f, 0.05f);
				var proj = Projectile.NewProjectileDirect(null, projPos, projVel, projType, 30, 0f, ai0: ai0, ai1: point.X, ai2: point.Y);
				proj.friendly = false;
				proj.hostile = true;
				proj.timeLeft = 3000;

				// Mark nearby tiles in order to place flames only at every other point.
				flamePoints.Add(tilePoint + new Point16(+1, +0));
				flamePoints.Add(tilePoint + new Point16(-1, +0));
				flamePoints.Add(tilePoint + new Point16(+0, +1));
				flamePoints.Add(tilePoint + new Point16(+0, -1));
			}
		}
	}
	/// <summary> Accesses neighbouring tiles, valid inputs are [1..maxTiles - 2]. </summary>
	private static bool IsTileSuitableForFlames(int x, int y)
	{
		Tile uTile = Main.tile[x + 0, y - 1];
		Tile dTile = Main.tile[x + 0, y + 1];
		Tile lTile = Main.tile[x - 1, y + 0];
		Tile rTile = Main.tile[x + 1, y + 0];
		bool uFull = (WorldUtilities.SolidTile(uTile) | uTile.LiquidAmount != 0);
		bool dFull = (WorldUtilities.SolidTile(dTile) | dTile.LiquidAmount != 0);
		bool lFull = (WorldUtilities.SolidTile(lTile) | lTile.LiquidAmount != 0);
		bool rFull = (WorldUtilities.SolidTile(rTile) | rTile.LiquidAmount != 0);
		bool tileSurrounded = uFull & dFull & lFull & rFull;
		bool tileOrphaned = !uFull & !dFull & !lFull & !rFull;
		return !tileSurrounded & !tileOrphaned;
	}

	private void SetupLimbs(in Context ctx)
	{
		_ = ctx;
		var baseFraming = new SpriteFrame(4, 1) { PaddingX = 0, PaddingY = 0 };
		var gibbableFraming = new SpriteFrame(4, 2) { PaddingX = 0, PaddingY = 0 };
		_ = baseFraming;

		// When facing right...
		limbs =
		[
			// Front right limb.
			new Limb()
			{
				Role = LimbRole.Walking,
				// Role = LimbRole.None,
				TexturePath = $"{Texture}_FrontArm1",
				Layer = +5,
				IK = new()
				{
					Offset = new Vector2(-28, +7),
					Segments = IKSegment.GenerateFromPoints(gibbableFraming, [ new(20, 12), new(40, 34), new(80, 80), new(87, 152), new(87, 183) ]),
				},
			},
			// Front left limb.
			new Limb()
			{
				Role = LimbRole.Walking | LimbRole.EntityGrabbing,
				// Role = LimbRole.EntityGrabbing,
				TexturePath = $"{Texture}_BackArm1",
				Layer = -1,
				CanBeGibbed = false,
				IK = new()
				{
					Offset = new Vector2(+84, -1),
					Segments = IKSegment.GenerateFromPoints(gibbableFraming, [ new(114, 18), new(100, 44), new(58, 98), new(24, 142), new(2, 176) ]),
				},
			},
			// Middle right limb.
			new Limb()
			{
				Role = LimbRole.Walking,
				TexturePath = $"{Texture}_FrontArm4",
				Layer = +4,
				IK = new()
				{
					Offset = new Vector2(-48, +29),
					Segments = IKSegment.GenerateFromPoints(gibbableFraming, [ new(6, 62), new(36, 32), new(88, 6), new(142, 32), new(169, 32) ]),
				},
			},
			// Middle left limb.
			new Limb()
			{
				Role = LimbRole.Walking,
				TexturePath = $"{Texture}_BackArm2",
				Layer = -4,
				IK = new()
				{
					Offset = new Vector2(+69, -17),
					Segments = IKSegment.GenerateFromPoints(gibbableFraming, [ new(58, 10), new(56, 34), new(48, 98), new(26, 182), new(26, 203) ]),
				},
			},
			// Back right limb.
			new Limb()
			{
				Role = LimbRole.Walking,
				TexturePath = $"{Texture}_FrontArm5",
				CanBeGibbed = false,
				Layer = +3,
				IK = new()
				{
					Offset = new Vector2(-69, +25),
					Segments = IKSegment.GenerateFromPoints(gibbableFraming, [ new(6, 12), new(26, 24), new(100, 76), new(136, 138), new(136, 157) ]),
				},
			},
			// Hanging leg 1.
			new Limb()
			{
				// Role = LimbRole.None,
				Role = LimbRole.Walking,
				TexturePath = $"{Texture}_BackArm4",
				Layer = -3,
				IK = new()
				{
					Offset = new Vector2(-7, +43),
					Segments = IKSegment.GenerateFromPoints(gibbableFraming, [ new(68, 10), new(52, 20), new(34, 52), new(22, 130), new(22, 143) ]),
				},
			},
			// Hanging leg 2.
			new Limb()
			{
				// Role = LimbRole.None,
				Role = LimbRole.Walking,
				TexturePath = $"{Texture}_BackArm5",
				Layer = -2,
				IK = new()
				{
					Offset = new Vector2(-51, +31),
					Segments = IKSegment.GenerateFromPoints(gibbableFraming, [ new(24, 10), new(20, 22), new(12, 62), new(34, 142), new(34, 155) ]),
				},
			},
			// Lowest wield.
			new Limb()
			{
				Role = LimbRole.Wielding,
				TexturePath = $"{Texture}_FrontArm2",
				Layer = +3,
				IK = new()
				{
					Offset = new Vector2(-12, -5),
					Segments = IKSegment.GenerateFromPoints(gibbableFraming, [ new(10, 110), new(34, 94), new(98, 40), new(48, 24), new(22, 10) ]),
				},
			},
			// Center wield.
			new Limb()
			{
				Role = LimbRole.Wielding, // | LimbRole.EntityGrabbing,
				TexturePath = $"{Texture}_FrontArm3",
				Layer = +2,
				IK = new()
				{
					Offset = new Vector2(-6, -25),
					Segments = IKSegment.GenerateFromPoints(gibbableFraming, [ new(28, 152), new(16, 136), new(12, 72), new(36, 26), new(36, 6) ]),
				},
			},
			// Highest wield.
			new Limb()
			{
				Role = LimbRole.Wielding,
				TexturePath = $"{Texture}_BackArm3",
				Layer = -2,
				MinPhaseToGib = PhaseType.Abomination,
				IK = new()
				{
					Offset = new Vector2(+66, -27),
					Segments = IKSegment.GenerateFromPoints(gibbableFraming, [ new(28, 190), new(30, 162), new(12, 82), new(100, 14), new(122, 14) ]),
				},
			},
		];

		if (Phase == PhaseType.Abomination)
		{
			// Double limbs.
			List<Limb> list = limbs.ToList();
			for (int i = 0; i < limbs.Length * 2; i++)
			{
				bool firstPass = i < limbs.Length;
				ref Limb oldLimb = ref limbs[i % limbs.Length];
				Limb newLimb = oldLimb;
				if (newLimb.Role.HasFlag(LimbRole.Wielding)) { continue; }
				
				oldLimb.IsGibbed = true;
				newLimb.IK.Offset.X *= firstPass ? 0.5f : 1.2f;
				newLimb.IK.Offset.X += firstPass ? 0 : -8;
				newLimb.IK.Offset.Y *= firstPass ? 1.0f : 0.8f;
				newLimb.IK.Offset.Y += firstPass ? -16 : +16;
				newLimb.IK.IsFlipped ^= firstPass;
				newLimb.IK.RestingAngle = MathF.Abs(newLimb.IK.RestingAngle - MathHelper.Pi);
				newLimb.CanBeGibbed = true;
				newLimb.SetGibbed(i % 3 == 0);
				list.Add(newLimb);
			}
			limbs = list.ToArray();
		}

		// Default and calculated data.
		foreach (ref Limb limb in limbs.AsSpan())
		{
			limb.IK.Target = NPC.Center;
			limb.TargetPosition = NPC.Center;
		}

		// Set the first walking limb.
		limbMovement.NextIndex = Math.Max(0, Array.FindIndex(limbs, l => l.Role.HasFlag(LimbRole.Walking)));
	}

	private void LimbMovement(in Context ctx)
	{
		_ = ctx;
		Vector2 logicalBodyCenter = ctx.Center;
		Vector2 visualBodyCenter = logicalBodyCenter + new Vector2(0f, NPC.gfxOffY);
		bool ignoreGibbing = Phase == PhaseType.Abomination;

		// Cooldown is reduced by the current speed.
		int numMovementLimbs = limbs.Count(l => (!l.IsGibbed || ignoreGibbing) && l.Role.HasFlag(LimbRole.Walking));
		float moveSpeed = NPC.velocity.Length();
		float coolSpeed = MathF.Max(1f, moveSpeed * 2f);
		limbMovement.Cooldown = MathUtils.StepTowards(limbMovement.Cooldown, 0f, coolSpeed * TimeSystem.LogicDeltaTime);
		limbMovement.NextIndex = Math.Clamp(limbMovement.NextIndex, 0, limbs.Length - 1);

		int limbIndex = -1;
		bool placeAll = limbs.All(l => l.TileAttachment == null);
		bool playedSounds = false;
		bool advanceOrder = false;
		foreach (ref Limb limb in limbs.AsSpan())
		{
			limbIndex++;

			if (!limb.Role.HasFlag(LimbRole.Walking)) { continue; }

			float sqrLength = limb.IK.SqrLength;
			bool grabTerrain = placeAll || (limbMovement.Cooldown <= 0f && limbIndex == limbMovement.NextIndex);
			bool isBusy = (limb.IsGibbed && !ignoreGibbing) || limb.EntityAttachment.IsValid || limb.BladeAttachment != null;
			Vector2 visualLimbPos = limb.IK.GetPosition(visualBodyCenter, rotation: bodyAngle.Current, xDir: NPC.spriteDirection);
			Vector2 logicalLimbPos = limb.IK.GetPosition(logicalBodyCenter, rotation: bodyAngle.Current, xDir: NPC.spriteDirection);

			advanceOrder |= grabTerrain;

			// If already attached (to a tile), advance order if needed.
			if (!isBusy && limb.TileAttachment?.ToWorldCoordinates() is { } attach && (attach.DistanceSQ(logicalLimbPos) <= sqrLength || attach.DistanceSQ(visualLimbPos) <= sqrLength))
			{
				limb.TargetPosition = attach;
			}
			// Otherwise, if walking, find a terrain point to grab.
			else if (!isBusy && grabTerrain && PickTileTarget(in ctx, ref limb, out Point16 tilePos))
			{
				limbMovement.Cooldown = (moveSpeed / numMovementLimbs);
				limb.ResetState();
				limb.LiftInAnimation = limb.TileAttachment != null;
				limb.TileAttachment = tilePos;
				limb.TargetPosition = ((Point16)tilePos).ToWorldCoordinates();
			}
			// Release attachment if all else fails.
			else
			{
				if (!isBusy) { limb.ResetState(); }
				continue;
			}

			float animSpeed = MathHelper.Lerp(MathUtils.Clamp01(limb.StartPosition.Distance(limb.TargetPosition) / 400), 1.5f, 1.4f);
			limb.Animation.Progress = MathUtils.StepTowards(limb.Animation.Progress, 1f, animSpeed * TimeSystem.LogicDeltaTime);

			const float soundRange = 16f;
			if (!Main.dedServ && limb.TileAttachment != null && !playedSounds && limb.IK.Target.Distance(limb.TargetPosition) <= soundRange && !limb.PlayedSound)
			{
				(playedSounds, limb.PlayedSound) = (true, true);

				Main.instance.CameraModifiers.Add(new PunchCameraModifier(limb.TargetPosition, new Vector2(0, -2), 1f, 4f, 15, 1000f, $"Footstep{limbIndex}"));
				SoundEngine.PlaySound(position: limb.TargetPosition, style: new($"{nameof(PathOfTerraria)}/Assets/Sounds/Footsteps/FleshyStomp", 3)
				{
					MaxInstances = 10,
					Volume = 0.35f,
					PitchVariance = 0.2f,
				});
			}
		}

		// Enqueue the next walking limb.
		if (advanceOrder)
		{
			ref int index = ref limbMovement.NextIndex;
			int startIndex = index;
			do { index = MathUtils.Modulo(index + NPC.spriteDirection, limbs.Length); }
			while (index != startIndex && !limbs[index].Role.HasFlag(LimbRole.Walking));
		}
	}
	private void LimbWielding(in Context ctx)
	{
		// Reset blade hold points.
		foreach (ref Limb limb in limbs.AsSpan())
		{
			limb.BladeAttachment = null;
		}

		if (Phase > PhaseType.Bladewielder)
		{
			return;
		}

		foreach (ref Limb limb in limbs.AsSpan())
		{
			if (limb.EntityAttachment.IsValid) { continue; }
			if (!limb.Role.HasFlag(LimbRole.Wielding)) { continue; }

			// If wielding, find a blade point to grab.
			if (!PickBladePoint(in ctx, ref limb, out (Vector2 Local, Vector2 Global) bladePoint)) { continue; }

			limb.LiftInAnimation = false;
			limb.TileAttachment = null;
			limb.BladeAttachment = bladePoint.Local;
			limb.Animation.Progress = MathUtils.StepTowards(limb.Animation.Progress, 1f, 3f * TimeSystem.LogicDeltaTime);
			limb.TargetPosition = bladePoint.Global;
		}
	}
	private void LimbGrabbing(in Context ctx)
	{
		_ = ctx;

		if (grabCooldown > 0)
		{
			grabCooldown--;
		}

		// Can grab slightly before the attack is considered over.
		const int postDmgWindow = 15;
		bool canGrab = Phase > 0 && introCounter == 0 && grabCooldown <= 0 && (!ctx.Attacking.Active || ctx.Attacking.Data.Progress > ctx.Attacking.Data.Damage.End + postDmgWindow);

		Vector2 logicalBodyCenter = ctx.Center;

		foreach (ref Limb limb in limbs.AsSpan())
		{
			if (limb.IsGibbed) { continue; }
			if (!limb.Role.HasFlag(LimbRole.EntityGrabbing)) { continue; }

			const float postInitiationRangeBonus = 16;
			float npcGrabRange = limb.IK.Length * 1.05f;
			float playerGrabRange = limb.IK.Length * 0.78f;
			float sqrNpcGrabRange = npcGrabRange * npcGrabRange;
			float sqrPlayerGrabRange = playerGrabRange * playerGrabRange;
			Vector2 logicalLimbPos = limb.IK.GetPosition(logicalBodyCenter, rotation: bodyAngle.Current, xDir: NPC.spriteDirection);

			// If already holding something.
			if (limb.EntityAttachment.Entity is { } grabEntity)
			{
			}
			else
			{
				if (!canGrab)
				{
					continue;
				}

				// Find closest entity to grab.

				foreach (Player player in Main.ActivePlayers)
				{
					// continue;
					if (player.DistanceSQ(logicalLimbPos) > sqrPlayerGrabRange) { continue; }
					if (limbs.Any(l => l.EntityAttachment.Player == player)) { continue; }

					limb.ResetState();
					limb.EntityAttachment = (EntityRef)player;

					break;
				}
#if false
				foreach (NPC npc in Main.ActiveNPCs)
				{
					if (npc == NPC) { continue; }
					if (npc.immortal || NPCID.Sets.ImmuneToAllBuffs[npc.type]) { continue; }
					if (npc.DistanceSQ(logicalLimbPos) > sqrNpcGrabRange) { continue; }
					if (limbs.Any(l => l.EntityAttachment.NPC == npc)) { continue; }
				
					limb.ResetState();
					limb.EntityAttachment = (EntityRef)npc;
				
					break;
				}
#endif

				if (!limb.EntityAttachment.IsValid) { continue; }

				grabEntity = limb.EntityAttachment.Entity!;
				limb.TargetPosition = grabEntity.Center;
				grabCooldown = 120;

				SoundEngine.PlaySound(position: ctx.Center, style: new($"{nameof(PathOfTerraria)}/Assets/Sounds/Conflux/PyralisGrab", 2)
				{
					MaxInstances = 3,
					PitchVariance = 0.125f,
					Volume = 1.0f,
				});

				// Play a special cue when grabbing the local player.
				if (!Main.dedServ && grabEntity == Main.LocalPlayer)
				{
					SoundEngine.PlaySound(position: ctx.Center, style: new($"{nameof(PathOfTerraria)}/Assets/Sounds/Conflux/PyralisGrabCue", 3)
					{
						MaxInstances = 3,
						PitchVariance = 0.05f,
						Volume = 0.6f,
					});
				}
			}

			// Advance animation stages.
			bool progressed = false;
			if (limb.Animation.Progress >= 1f)
			{
				limb.Animation.Stage++;
				limb.Animation.Progress = 0f;
				limb.StartPosition = limb.IK.Target;
				progressed = true;
			}

			Vector2 throwDirection = (ctx.TargetCenter - grabEntity.Center).SafeNormalize(Vector2.UnitX * NPC.direction);

			// Stage 0 - Grab.
			if (limb.Animation.Stage == 0)
			{
				limb.TargetPosition = grabEntity.Center;
				limb.Animation.Progress = MathUtils.StepTowards(limb.Animation.Progress, 1f, 2.0f * TimeSystem.LogicDeltaTime);
			}
			// Stage 1 - Hold.
			else if (limb.Animation.Stage == 1)
			{
				// Halt the grab if out of range by the end of the grabbing animation.
				// Unless this is a non-friendly NPC.
				if (progressed
				&& grabEntity is not NPC { friendly: false }
				&& grabEntity.Distance(logicalLimbPos) > (postInitiationRangeBonus + (grabEntity is Player ? playerGrabRange : npcGrabRange)))
				{
					limb.ResetState();
					continue;
				}

				limb.TargetPosition = logicalLimbPos + ((Vector2.UnitX * NPC.direction).RotatedBy(MathHelper.Pi * +0.2f * NPC.direction) * limb.IK.Length * +0.25f);
				limb.Animation.Progress = MathUtils.StepTowards(limb.Animation.Progress, 1f, 1.5f * TimeSystem.LogicDeltaTime);
			}
			// Stage 2 - Throw.
			else if (limb.Animation.Stage == 2)
			{
				limb.TargetPosition = logicalLimbPos + (throwDirection * limb.IK.Length);
				limb.Animation.Progress = MathUtils.StepTowards(limb.Animation.Progress, 1f, 4.5f * TimeSystem.LogicDeltaTime);
			}
			
			// Adjust target points during hold & throw, so that they do not put anyone into a solid wall.
			if (limb.Animation.Stage is >= 1 and <= 2)
			{
				Point basePoint = limb.TargetPosition.ToTileCoordinates();
				Vector2 size = grabEntity.Size + new Vector2(32, 32);
				Point sizeInTiles = grabEntity.Size.ToTileCoordinates();
				if (TileUtils.TryFitRectangleIntoTilemap(basePoint, sizeInTiles, out Vector2Int adjustedPoint))
				{
					limb.TargetPosition = ((Point)adjustedPoint).ToWorldCoordinates(0, 0) + (size * 0.5f);
				}
				else
				{
					limb.TargetPosition = ctx.Center;
				}
			}

			if (!Main.dedServ)
			{
				// Red glow during grab, purple flash during throw.
				Vector3 color = limb.Animation.Stage != 2 ? new Vector3(1.0f, 0.2f, 0.0f) : new Vector3(1.0f, 0.2f, 1.0f);
				Lighting.AddLight(limb.IK.Target, color);
			}
			
			// Release.
			if (limb.Animation is { Stage: 2, Progress: >= 0.25f } or { Stage: >= 3 })
			{
				limb.ResetState();
				grabEntity.velocity = throwDirection * 30f;

				Vector2? sndPos = grabEntity != Main.LocalPlayer ? ctx.Center : null;
				SoundEngine.PlaySound(position: sndPos, style: new($"{nameof(PathOfTerraria)}/Assets/Sounds/Conflux/PyralisThrow", 2)
				{
					MaxInstances = 3,
					PitchVariance = 0.125f,
					Volume = 1.0f,
				});

				if (!Main.dedServ)
				{
					if (grabEntity == Main.LocalPlayer)
					{
						var punch = new PunchCameraModifier(limb.TargetPosition, throwDirection.RotatedBy(MathHelper.PiOver2), 6, 3f, 90, 1000f, "PlayerThrow");
						Main.instance.CameraModifiers.Add(punch);
					}

					// Purple flash.
					Lighting.AddLight(limb.IK.Target, new Vector3(1.0f, 0.2f, 1.0f));
				}
			}

			// Move entity into hand.
			if (limb.Animation.Stage is >= 1 and <= 2)
			{
				grabEntity.Center = limb.IK.Target + new Vector2(grabEntity.width * 0.5f * NPC.direction, -grabEntity.height * 0.25f);
			}
		}
	}
	private void LimbFlailing(in Context ctx)
	{
		_ = ctx;
		Vector2 logicalBodyCenter = NPC.Center;
		Vector2 visualBodyCenter = logicalBodyCenter + new Vector2(0f, NPC.gfxOffY);

		foreach (ref Limb limb in limbs.AsSpan())
		{
			if (!limb.IsGibbed)
			{
				if (limb.TileAttachment != null) { continue; }
				if (limb.BladeAttachment != null) { continue; }
				if (limb.EntityAttachment.IsValid) { continue; }
			}

			Vector2 visualLimbPos = limb.IK.GetPosition(visualBodyCenter, rotation: bodyAngle.Current, xDir: NPC.spriteDirection);
			var flailOffset = new Vector2(0, 128);

			if (limb.Role.HasFlag(LimbRole.Wielding))
			{
				flailOffset = new Vector2(0, -64);
			}

			var flailTarget = Vector2.Lerp(visualLimbPos + flailOffset, NPC.GetTargetData().Center, 0.1f);
			limb.Animation.Progress = MathUtils.StepTowards(limb.Animation.Progress, 1f, 0.8f * TimeSystem.LogicDeltaTime);
			limb.TargetPosition = flailTarget;
		}
	}

	private void UpdateLimbs(in Context ctx)
	{
#if DEBUG && true
		if (Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.N)) { SetupLimbs(in ctx); }
#endif

		Vector2 logicalBodyCenter = NPC.Center;
		Vector2 visualBodyCenter = logicalBodyCenter + new Vector2(0f, NPC.gfxOffY);

		int limbIndex = -1;
		bool placeAll = limbs.All(l => l.TileAttachment == null);
		foreach (ref Limb limb in limbs.AsSpan())
		{
			limbIndex++;

			float sqrLength = limb.IK.Length * limb.IK.Length;
			bool grabBlade = limb.Role.HasFlag(LimbRole.Wielding);
			bool grabTerrain = (placeAll && limb.Role.HasFlag(LimbRole.Walking)) || (limbMovement.Cooldown <= 0f && limbIndex == limbMovement.NextIndex);
			Vector2 visualLimbPos = limb.IK.GetPosition(visualBodyCenter, rotation: bodyAngle.Current, xDir: NPC.spriteDirection);
			Vector2 logicalLimbPos = limb.IK.GetPosition(logicalBodyCenter, rotation: bodyAngle.Current, xDir: NPC.spriteDirection);

#if IK_PREVIEW
			limb.IK.Reset(new() { Center = visualLimbPos, FlipHorizontally = NPC.spriteDirection > 0 });
#else
			limb.IK.Resolve(out _, in ikCfg, new()
			{
				Center = visualLimbPos,
				FlipHorizontally = NPC.spriteDirection > 0,
			});
#endif

			// Shift the current target point.
			float anim = limb.Animation.Progress;
			float liftAnim = limb.LiftInAnimation ? MathUtils.Clamp01(anim * 8) : 0f;
			float liftEffect = LiftEasing(MathUtils.Clamp01(liftAnim <= 0.5f ? (liftAnim * 2f) : ((1f - liftAnim) * 2f)));
			var liftPos = new Vector2(limb.TargetPosition.X, visualLimbPos.Y);
			var usedTarget = Vector2.Lerp(limb.TargetPosition, visualLimbPos, liftEffect * 0.2f);
			limb.IK.Target = Vector2.Lerp(limb.StartPosition, usedTarget, MoveEasing(anim));

			static float MoveEasing(float x)
			{
				return x >= 1f ? 1 : 1 - MathF.Pow(2, -10 * x);
			}
			static float LiftEasing(float x)
			{
				return 1f - MathF.Pow(1f - x, 3f);
			}
		}
	}

	private void GibLimbs(in Context ctx)
	{
		int lifeNew = NPC.life;
		float factorNew = MathUtils.Clamp01(NPC.life / MathF.Max(1, NPC.lifeMax));
		float factorOld = MathUtils.Clamp01(lifeOld / MathF.Max(1, NPC.lifeMax));

		bool CanGibLimb(int i)
		{
			ref Limb limb = ref limbs[i];
			return !limb.IsGibbed && limb.CanBeGibbed && (int)Phase >= (int)limb.MinPhaseToGib && limb.IK.Segments[0].Frame.RowCount > 1;
		}

		int divisions = 20 * (Phase == PhaseType.Abomination ? 2 : 1);
		if (lifeNew < lifeOld && (int)MathF.Ceiling(factorNew * divisions) < (int)MathF.Ceiling(factorOld * divisions))
		{
			if (Enumerable.Range(0, limbs.Length).Where(CanGibLimb).ToArray() is { Length: > 0 } gibbables)
			{
				int gibIndex = gibbables[Main.rand.Next(gibbables.Length)];
				ref Limb limb = ref limbs[gibIndex];
				Vector2 logicalBodyCenter = ctx.Center;
				Vector2 limbPos = limb.IK.GetPosition(logicalBodyCenter, rotation: bodyAngle.Current, xDir: NPC.spriteDirection);

				limb.ResetState();
				limb.SetGibbed(true);
				
				SoundEngine.PlaySound(position: ctx.Center, style: new($"{nameof(PathOfTerraria)}/Assets/Sounds/Gore/SuperSplatter")
				{
					Volume = 0.9f,
					MaxInstances = 3,
					PitchVariance = 0.3f,
				});

				for (int i = 0; i < 200; i++)
				{
					Vector2 pos = limbPos + Main.rand.NextVector2Circular(32, 32);
					Vector2 vel = Main.rand.NextVector2Circular(25, 25);
					float scale = 1f + (Main.rand.NextFloat() * Main.rand.NextFloat());
					Dust.NewDustPerfect(pos, DustID.Blood, vel, Scale: scale);
				}

				for (int i = 0; i < 10; i++)
				{
					Vector2 pos = limbPos + Main.rand.NextVector2Circular(64, 64);
					Vector2 vel = Main.rand.NextVector2Circular(10, 10);
					Gore.NewGorePerfect(NPC.GetSource_FromThis(), pos, vel, ModContent.GoreType<BloodSplatLarge>());
					Gore.NewGorePerfect(NPC.GetSource_FromThis(), pos, vel, ModContent.GoreType<BloodSplatMedium>());
					Gore.NewGorePerfect(NPC.GetSource_FromThis(), pos, vel, 135);
				}
			}
		}

		lifeOld = NPC.life;
	}

	/// <summary> Finds a tile for a limb to grab during walking or climbing. </summary>
	private bool PickTileTarget(in Context ctx, ref Limb limb, [MaybeNullWhen(false)] out Point16 result)
	{
		// Pick a direction to use for dot products.
		// With some bias towards the bottom.
		Vector2 movDir = (NPC.velocity * new Vector2(1.5f, 1.0f) + new Vector2(0f, 3.5f)).SafeNormalize(Vector2.UnitY);

		Vector2 startPos = limb.IK.GetPosition(NPC.Center, rotation: bodyAngle.Current, xDir: NPC.spriteDirection);
		Vector2Int startTile = startPos.ToTileCoordinates();
		float placeRange = limb.IK.Length * 0.97f;
		int tileRange = (int)MathF.Floor(placeRange / TileUtils.TileSizeInPixels);

		int x1 = Math.Max(startTile.X - tileRange, 1);
		int y1 = Math.Max(startTile.Y - tileRange, 1);
		int x2 = Math.Min(startTile.X + tileRange, Main.maxTilesX - 2);
		int y2 = Math.Min(startTile.Y + tileRange, Main.maxTilesY - 2);
		(float bestScore, Point16 bestTile) = (float.NegativeInfinity, default);
		for (int x = x1; x <= x2; x++)
		{
			for (int y = y1; y <= y2; y++)
			{
				var tilePos = new Point16(x, y);
				var tile = (Tile)Main.tile[tilePos];
				// if (!tile.HasTile || (!Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType]))
				if (!tile.HasTile)
				{
					continue;
				}

				var worldPos = (Vector2)tilePos.ToWorldCoordinates();
				var diffFromCenter = (Vector2)(worldPos - ctx.Center);
				float distFromLimb = (float)(worldPos.Distance(startPos));

				// Short-circuit if this is too far.
				if (distFromLimb >= placeRange) { continue; }

				var dirFromCenter = (Vector2)(diffFromCenter.SafeNormalize(Vector2.UnitY));

				// Compute score for this tile.

				// The further from the NPC center, the better.
				float distScore = distFromLimb;

				// The more directionally aligned with the movement direction, the better.
				float dirDot = Vector2.Dot(dirFromCenter, movDir);
				float dirScore = dirDot + 1.25f;

				float score = distScore * dirScore;
				if (score > bestScore)
				{
					// Penalties are computed last.

					// Penalize using a tile that another limb is already grabbing.
					float sharePenalty = limbs.Any(l => l.TileAttachment == tilePos) ? 0.1f : 1f;
					score *= sharePenalty;

					// Penalize using a tile that another limb is already grabbing.
					Tile uTile = Main.tile[x + 0, y - 1];
					Tile dTile = Main.tile[x + 0, y + 1];
					Tile lTile = Main.tile[x - 1, y + 0];
					Tile rTile = Main.tile[x + 1, y + 0];
					bool uFull = (WorldUtilities.SolidTile(uTile) | uTile.LiquidAmount != 0);
					bool dFull = (WorldUtilities.SolidTile(dTile) | dTile.LiquidAmount != 0);
					bool lFull = (WorldUtilities.SolidTile(lTile) | lTile.LiquidAmount != 0);
					bool rFull = (WorldUtilities.SolidTile(rTile) | rTile.LiquidAmount != 0);
					bool tileSurrounded = uFull || dFull || lFull || rFull;
					float surroundPenalty = tileSurrounded ? 0.1f : 1f;
					score *= surroundPenalty;

					if (score > bestScore)
					{
						(bestScore, bestTile) = (score, tilePos);
					}
				}
			}
		}

		if (bestScore >= 0f)
		{
			result = bestTile;
			return true;
		}

		result = default;
		return false;
	}

	private record struct BladeAnimKey(Vector2 Pos, float Angle)
	{
		static BladeAnimKey()
		{
			Gradient<BladeAnimKey>.Lerp = Lerp;
		}

		public static BladeAnimKey Lerp(BladeAnimKey a, BladeAnimKey b, float step)
		{
			// Using non-radian lerp for angles on purpose.
			return new(Vector2.Lerp(a.Pos, b.Pos, step), MathHelper.Lerp(a.Angle, b.Angle, step));
		}
	}

	private void SetupBlade(in Context ctx)
	{
		Blade.Position = ctx.Center + new Vector2(0f, -32f);
		Blade.Rotation = MathHelper.Pi * 1.5f;
	}
	private void UpdateBlade(in Context ctx)
	{
		// Store historical information.
		Array.Copy(BladeHistory, 0, BladeHistory, 1, BladeHistory.Length - 1);

		if (Phase > PhaseType.Bladewielder)
		{
			bladeVelocity.Pos.Y += 20f * TimeSystem.LogicDeltaTime;
			Blade.Position += bladeVelocity.Pos;
			Blade.Rotation += bladeVelocity.Rot;
			return;
		}

		var slashAnimation = new Gradient<BladeAnimKey>
		([
			new(0.00f, new(Pos: new(-096, +008), Angle: MathHelper.TwoPi * 0.25f)),
			new(0.30f, new(Pos: new(+096, -128), Angle: MathHelper.TwoPi * 0.55f)),
			new(0.80f, new(Pos: new(+112, -000), Angle: MathHelper.TwoPi * 1.15f)),
			new(0.90f, new(Pos: new(+112, -016), Angle: MathHelper.TwoPi * 1.13f)),
		]);
		var reverseAnimation = new Gradient<BladeAnimKey>
		([
			new(0.00f, new(Pos: new(-096, +008), Angle: MathHelper.TwoPi * -0.25f)),
			new(0.30f, new(Pos: new(+096, -128), Angle: MathHelper.TwoPi * -0.55f)),
			new(0.80f, new(Pos: new(+112, -000), Angle: MathHelper.TwoPi * -1.15f)),
			new(0.90f, new(Pos: new(+112, -016), Angle: MathHelper.TwoPi * -1.13f)),
		]);
		var stabAnimation = new Gradient<BladeAnimKey>
		([
			new(0.00f, new(Pos: new(-096, +008), Angle: MathHelper.TwoPi * 0.00f)),
			new(0.30f, new(Pos: new(+096, -128), Angle: MathHelper.TwoPi * 0.00f)),
			new(0.80f, new(Pos: new(+236, -000), Angle: MathHelper.TwoPi * 0.00f)),
			new(0.90f, new(Pos: new(+220, -016), Angle: MathHelper.TwoPi * 0.00f)),
		]);

		static float AnimEasing(float x)
		{
			return x < 0.5f ? (1 - MathF.Sqrt(1 - MathF.Pow(2 * x, 2))) / 2 : (MathF.Sqrt(1 - MathF.Pow(-2 * x + 2, 2)) + 1) / 2;
		}

		AttackingData atk = ctx.Attacking.Data;

		float rotChange;
		float posChange;
		float maxDistance;
		float targetRotation;
		Vector2 targetPosition;

		if (ctx.Attacking.Active)
		{
			// Attack animations.
			Gradient<BladeAnimKey> animation = attackType switch
			{
				AttackType.Stab => stabAnimation,
				AttackType.Reverse => reverseAnimation,
				_ => slashAnimation,
			};
			bool mirror = NPC.spriteDirection < 0;
			float prevAnimProgress = AnimEasing(MathUtils.Clamp01((atk.Progress + 0) / (float)atk.LengthInTicks));
			float currAnimProgress = AnimEasing(MathUtils.Clamp01((atk.Progress + 1) / (float)atk.LengthInTicks));
			BladeAnimKey sample = animation.GetValue(currAnimProgress);

			// Position.
			posChange = Utils.Remap(currAnimProgress, 0.00f, 0.10f, 0.01f, 1.00f);
			targetPosition = ctx.Center + (sample.Pos.RotatedBy(ctx.Attacking.Data.Angle));
			maxDistance = 192;
			// Angle.
			rotChange = Utils.Remap(currAnimProgress, 0.00f, 0.10f, 0.01f, 1.00f);
			targetRotation = sample.Angle;
			targetRotation = mirror ? (MathHelper.Pi - targetRotation) : targetRotation;
			targetRotation += mirror ? (ctx.Attacking.Data.Angle - MathHelper.Pi) : ctx.Attacking.Data.Angle;

			if (!Main.dedServ && prevAnimProgress < 0.2f && currAnimProgress >= 0.2f)
			{
				SoundEngine.PlaySound(SoundID.Item77 with { Volume = 1.5f, Pitch = -0.1f, MaxInstances = 3 }, ctx.Center);
				SoundEngine.PlaySound(SoundID.Item77 with { Volume = 1.5f, Pitch = -0.1f, MaxInstances = 3 }, ctx.Center);
			}
		}
		else
		{
			// Idle animations.
			targetPosition = ctx.Center + new Vector2(0, -64);
			posChange = 0.08f;

			targetRotation = (MathHelper.PiOver2 + (MathHelper.TwoPi * 0.13f * NPC.spriteDirection));
			rotChange = 0.09f;
			maxDistance = 192;
		}

		// Apply transformation.
		Blade.Rotation = MathUtils.LerpRadians(Blade.Rotation, targetRotation, rotChange);
		Blade.Position = Vector2.Lerp(Blade.Position, targetPosition, posChange);
		// Clip blade position if it's too far away.
		if (targetPosition.Distance(Blade.Position) > maxDistance)
		{
			Blade.Position = targetPosition + (targetPosition.DirectionTo(Blade.Position) * maxDistance);
		}
	}
	private static IEnumerable<Vector2> GetBladeGrabPoints()
	{
		yield return new(+0, +0);
		yield return new(-48, +0);
		yield return new(+48, +0);
	}
	/// <summary> Finds the closest free and reachable blade grab point. Object-relative. </summary>
	private bool PickBladePoint(in Context ctx, ref Limb limb, [MaybeNullWhen(false)] out (Vector2 Local, Vector2 Global) result)
	{
		_ = ctx;
		Vector2 limbOrigin = limb.IK.GetPosition(NPC.Center, rotation: bodyAngle.Current, xDir: NPC.spriteDirection);
		float reachRange = limb.IK.Length * 0.97f;
		float reachRangeSqr = reachRange * reachRange;

		(float SqrDistance, Vector2 Local, Vector2 Global) bestResult = (float.PositiveInfinity, default, default);

		foreach (Vector2 local in GetBladeGrabPoints())
		{
			Vector2 global = Blade.Position + (local.RotatedBy(Blade.Rotation));
			float sqrDistance = limbOrigin.DistanceSQ(global);

			if (sqrDistance > reachRangeSqr) { continue; }
			if (sqrDistance > bestResult.SqrDistance) { continue; }
			if (limbs.Any(l => l.BladeAttachment == local)) { continue; }

			bestResult = (sqrDistance, local, global);
		}

		if (bestResult.SqrDistance != float.PositiveInfinity)
		{
			result = (bestResult.Local, bestResult.Global);
			return true;
		}

		result = default;
		return false;
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
		Context ctx = new(NPC);

		ctx.Animations.Advance();
		ctx.Animations.Set(PickAnimation(in ctx));
	}

	public override bool PreDraw(SpriteBatch sb, Vector2 screenPos, Color drawColor)
	{
		Context ctx = new(NPC);

		if (NPC.IsABestiaryIconDummy && limbs.Length == 0)
		{
			SetupLimbs(in ctx);
		}

		helmetTexture ??= ModContent.Request<Texture2D>($"{Texture}_Helmet", AssetRequestMode.ImmediateLoad);
		bodyTexture ??= ModContent.Request<Texture2D>($"{Texture}_Body", AssetRequestMode.ImmediateLoad);
		robeTexture ??= ModContent.Request<Texture2D>($"{Texture}_Robe", AssetRequestMode.ImmediateLoad);

		SpriteBatchArgs sbArgs = sb.GetArguments();
		// sb.End();
		// sb.Begin(sbArgs with { Effect = LitShaders.LitSprite(sbArgs.Matrix) });

		// Draw the back arms.
		RenderArms(layerSign: -1, screenPos, drawColor);

		// Body and head.
		float bAngle = bodyAngle.Current;
		float hAngle = headAngle.Current + bAngle;
		Vector2 bOrigin = new(83, 93);
		Vector2 rOrigin = new(101, 97);
		Vector2 hOrigin = new(62, 74);
		Vector2 bCenter = ctx.Center;
		Vector2 hCenter = ctx.Center + new Vector2(48 * NPC.spriteDirection, -8).RotatedBy(bAngle);
		ctx.Animations.Render(NPC, bodyTexture.Value, bodyTexture.Value.Frame(), screenPos, drawColor, center: bCenter, origin: bOrigin, rotation: bAngle);
		if (Phase <= PhaseType.Bladewielder)
		{
			ctx.Animations.Render(NPC, robeTexture.Value, robeTexture.Value.Frame(), screenPos, drawColor, center: bCenter, origin: rOrigin, rotation: bAngle);
			ctx.Animations.Render(NPC, helmetTexture.Value, helmetTexture.Value.Frame(), screenPos, drawColor, center: hCenter, origin: hOrigin, rotation: hAngle);
		}

		RenderBlade(sb, screenPos);

		// Draw the front arms.
		RenderArms(layerSign: +1, screenPos, drawColor);

		// sb.End();
		// sb.Begin(sbArgs);

		return false;
	}

	private void RenderBlade(SpriteBatch sb, Vector2 screenPos)
	{
		if (NPC.IsABestiaryIconDummy) { return; }

		Context ctx = new(NPC);
		SpriteBatchArgs sbArgs = sb.GetArguments();

		Vector2 bladeOrigin = new(146, 86); // Approximate hilt center.
		bladeBaseTexture ??= ModContent.Request<Texture2D>($"{Texture}_Blade", AssetRequestMode.ImmediateLoad);
		bladeGlowTexture ??= ModContent.Request<Texture2D>($"{Texture}_Blade_Glow", AssetRequestMode.ImmediateLoad);

		sb.End();
		RenderBladeInner(screenPos, bladeBaseTexture.Value, bladeGlowTexture.Value, bladeOrigin);
		sb.Begin(in sbArgs);
	}
	private static readonly short[] QuadTriangles = [0, 2, 3, 0, 1, 2];
	private void RenderBladeInner(Vector2 screenPos, Texture2D diffuse, Texture2D glowmask, Vector2 origin)
	{
		if (BladeHistory.Length == 0)
		{
			return;
		}

		Vector2 maskExtensionOffset = Main.sceneTilePos - Main.screenPosition;
		Vector2 maskTargetSize = Main.instance.tileTarget.Size();
		Matrix viewMatrix = Main.GameViewMatrix.NormalizedTransformationmatrix;

		using var vertices = new RentedArray<VertexPositionColorTexture>(4);
		Rectangle bounds = diffuse.Bounds;
		var pos = new Vector4(bounds.Left, bounds.Top, bounds.Right, bounds.Bottom);
		var uv0 = new Vector4(0f, 0f, 1f, 1f);

		GraphicsDevice gfx = Main.instance.GraphicsDevice;
		Effect effect = (bladeShader ??= ModContent.Request<Effect>($"{PoTMod.ModName}/Assets/Effects/PyralisBlade", AssetRequestMode.ImmediateLoad)).Value;
		effect.Parameters["Glowmask"].SetValue(glowmask);
		effect.Parameters["Light"].SetValue(LightingBuffer.GetOrWhite());
		effect.Parameters["Tiles"].SetValue(Main.instance.tileTarget);
		effect.Parameters["Black"].SetValue(Main.instance.blackTarget);
		effect.Parameters["MaskSize"].SetValue(maskTargetSize);
		effect.Parameters["MaskOffset"].SetValue(maskExtensionOffset);
		effect.Parameters["ScreenPosition"].SetValue(Main.screenPosition);
		effect.Parameters["ScreenResolution"].SetValue(Main.ScreenSize.ToVector2());
		effect.Parameters["View"].SetValue(viewMatrix);
		gfx.BlendState = BlendState.AlphaBlend;
		gfx.DepthStencilState = DepthStencilState.None;
		gfx.RasterizerState = Main.Rasterizer;

		Texture2D nothing = ModContent.Request<Texture2D>($"{nameof(PathOfTerraria)}/Assets/Empty", AssetRequestMode.ImmediateLoad).Value;

		//TODO: Could be made faster. Does it matter?
		int numLayers = 40;
		int numFrames = 5;
		float layersPerFrame = numLayers / (float)numFrames;
		for (int i = numLayers - 1; i >= 0; i--)
		{
			// Use different frames, as well as interpolate between them.
			float globalStep = i / (float)(numLayers - 1);
			int thisFrame = (int)(i / layersPerFrame);
			int nextFrame = thisFrame + 1;
			float frameStep = i % layersPerFrame;
			BladeTransform thisTransform = BladeHistory[thisFrame];
			BladeTransform nextTransform = BladeHistory[nextFrame];
			float angle = MathUtils.LerpRadians(thisTransform.Rotation, nextTransform.Rotation, frameStep);
			var worldPos = Vector2.Lerp(thisTransform.Position, nextTransform.Position, frameStep);

			Color col = i == 0 ? Color.White : new(Vector4.One * 0.05f);
			vertices[0] = new(new(pos.X, pos.Y, 0f), col, new(uv0.X, uv0.Y));
			vertices[1] = new(new(pos.Z, pos.Y, 0f), col, new(uv0.Z, uv0.Y));
			vertices[2] = new(new(pos.Z, pos.W, 0f), col, new(uv0.Z, uv0.W));
			vertices[3] = new(new(pos.X, pos.W, 0f), col, new(uv0.X, uv0.W));

			effect.Parameters["Texture"].SetValue(i == 0 ? diffuse : nothing);
			Matrix worldMatrix =
			(
				Matrix.CreateTranslation(-origin.X, -origin.Y, 0)
				* Matrix.CreateRotationZ(angle)
				* Matrix.CreateTranslation(worldPos.X - screenPos.X, worldPos.Y - screenPos.Y, 0f)
			);
			effect.Parameters["World"].SetValue(worldMatrix);

			foreach (EffectPass pass in effect.CurrentTechnique.Passes)
			{
				pass.Apply();
				gfx.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices.Array, 0, vertices.Length, QuadTriangles, 0, QuadTriangles.Length / 3);
			}
		}
	}

	private void RenderArms(int? layerSign, Vector2 screenPos, Color drawColor)
	{
		foreach (int limbIndex in Enumerable.Range(0, limbs.Length).OrderBy(i => limbs[i].Layer))
		{
			ref Limb limb = ref limbs[limbIndex];
			if (layerSign != null && (limb.Layer >= 0f ? +1 : -1) != layerSign.Value) { continue; }

			limb.TextureCache ??= ModContent.Request<Texture2D>(limb.TexturePath);

			if (limb.TextureCache is not { IsLoaded: true, Value: { } limbTexture }) { continue; }

			for (int j = 0; j < limb.IK.Segments.Length; j++)
			{
				DrawData drawData = limb.IK.GetDrawParams(limbTexture, j, screenPos, NPC.spriteDirection);
				drawData.color = drawData.color.MultiplyRGBA(drawColor);
				// drawData.color = Lighting.GetColor((drawData.position + Main.screenPosition).ToTileCoordinates());
				drawData.Draw(Main.spriteBatch);
				//Main.EntitySpriteDraw(drawData);
			}

#if DEBUG && false
			for (int j = 0; j < limb.Results.Length; j++)
			{
				Main.spriteBatch.DrawString(FontAssets.MouseText.Value, "x", arm.Results[j] - Main.screenPosition, Color.Cyan);
			}
#endif
		}
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
}


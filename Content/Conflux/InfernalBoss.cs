// #define IK_PREVIEW
// #define DEBUG_GIZMOS
// #define FRIENDLY
// #define FOLLOW_MOUSE
// #define NO_MINIONS
// #define NO_TELEPORTS
// #define NO_FLAMES
// #define DEBUG_KEYS
// #define HIDE_SWORD
// #define FORCE_PHASE_II
// #define LOW_HEALTH

using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using NPCUtils;
using PathOfTerraria.Common.AI;
using PathOfTerraria.Common.NPCs.Components;
using PathOfTerraria.Common.NPCs.Effects;
using PathOfTerraria.Common.Utilities;
using PathOfTerraria.Common.Utilities.Extensions;
using PathOfTerraria.Common.World.Utilities;
using PathOfTerraria.Content.Buffs.ElementalBuffs;
using PathOfTerraria.Content.Gores;
using PathOfTerraria.Content.Items.Gear.Armor.Chestplate;
using PathOfTerraria.Content.Items.Gear.Armor.Helmet;
using PathOfTerraria.Content.Items.Gear.Armor.Leggings;
using PathOfTerraria.Content.Items.Gear.Rings;
using PathOfTerraria.Content.Items.Gear.Weapons.Sword;
using PathOfTerraria.Core.Camera;
using PathOfTerraria.Core.Graphics;
using PathOfTerraria.Core.Graphics.Particles;
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
		if (npc.ModNPC is not InfernalBoss { Phase: > 0, CutsceneActive: false } boss)
		{
			return false;
		}

		// Alter displayed health to keep the second phase a secret.
		float lifeFactor = InfernalBoss.SecondPhaseHealthFactor;
		bool secondPhase = boss.Phase is InfernalBoss.PhaseType.Abomination;
		lifeFactor = secondPhase ? lifeFactor : (1f - lifeFactor);
		drawParams.Life -= secondPhase ? 0 : (int)(drawParams.LifeMax * (1f - lifeFactor));
		drawParams.LifeMax = (int)(drawParams.LifeMax * lifeFactor);
		drawParams.Life = Math.Clamp(drawParams.Life, 0, drawParams.LifeMax);

		return true;
	}
}

/// <summary> Location-bound flames that block areas for a bit of time. </summary>
internal sealed class InfernalFlames : ModProjectile
{
	public ref float Progress => ref Projectile.ai[0];

	public Vector2 StartPos
	{
		get => new(Projectile.localAI[1], Projectile.localAI[2]);
		set => (Projectile.localAI[1], Projectile.localAI[2]) = (value.X, value.Y);
	}
	public Vector2 TargetPos
	{
		get => new(Projectile.ai[1], Projectile.ai[2]);
		set => (Projectile.ai[1], Projectile.ai[2]) = (value.X, value.Y);
	}

	public override string Texture => $"{PoTMod.ModName}/Assets/Particles/InfernalParticle";

	public override void SetStaticDefaults()
	{
		Main.projFrames[Type] = 7;
	}
	public override void SetDefaults()
	{
		Projectile.aiStyle = -1;
		Projectile.damage = 50;
		Projectile.Size = new(52);
		Projectile.timeLeft = 60 * 10;
		Projectile.hostile = true;
		Projectile.friendly = false;
		Projectile.tileCollide = true;
		Projectile.aiStyle = -1;
		Projectile.usesLocalNPCImmunity = true;
		Projectile.localNPCHitCooldown = -1;
		Projectile.rotation = Main.rand.NextFloatDirection();
	}

	public override bool OnTileCollide(Vector2 oldVelocity)
	{
		Projectile.velocity = default;
		return false;
	}

	public override void AI()
	{
		if (Progress >= 1f)
		{
			Projectile.Kill();
			for(int i = 0; i < 48; i++)
			{
				Dust.NewDustPerfect(Projectile.Center, DustID.Torch, Main.rand.NextVector2CircularEdge(12, 12)).noGravity = true;
			}

			return;
		}

		Projectile.rotation -= 0.2f;

		if (StartPos == default) { StartPos = Projectile.Center; }

		if (TargetPos != default)
		{
			float step = MathUtils.Clamp01(Progress / 0.05f);
			step = 1f - MathF.Pow(1f - step, 2f);
			Projectile.Center = Vector2.Lerp(StartPos, TargetPos, step);
		}

		Projectile.scale = Utils.Remap(Progress, 0.0f, 0.05f, 0.05f, 1f);
		Projectile.velocity *= 0.98f;

		int frameCount = Main.projFrames[Type];
		Progress += TimeSystem.LogicDeltaTime / 8f;

		if (!Main.dedServ)
		{
			float lightIntensity = Utils.Remap(Progress, 0.0f, 0.1f, 0.3f, 1.0f) - Utils.Remap(Progress, 0.85f, 1.0f, 0.0f, 1.0f);
			Vector3 lightColor = new Vector3(1.0f, 0.7f, 0.2f) * lightIntensity * 2f;
			Lighting.AddLight(Projectile.Center, lightColor);
		}
	}

	public override bool? CanDamage()
	{
		return Progress >= 0.06f && Progress <= 0.90f;
	}

	public override void OnHitPlayer(Player target, Player.HurtInfo info)
	{
		IgnitedDebuff.ApplyTo(Projectile, target, info.Damage);
	}

	public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
	{
		if (target.HasBuff<IgnitedDebuff>())
		{
			modifiers.FinalDamage *= 1.3f;
		}
	}
	public override bool PreDraw(ref Color lightColor)
	{
		Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
		Vector2 worldPos = Projectile.Center.ToPoint().ToVector2();
		Vector2 screenPos = worldPos - Main.screenPosition;
		for (int i = 0; i < 4; i++)
		{
			Main.EntitySpriteDraw(texture, screenPos, null, new Color(35, 1, 35,0), Projectile.rotation + i / 6f * float.Tau, texture.Size() * 0.5f, Projectile.scale * 1f, 0, 0f);
		}
		for (int i = 0; i < 12; i++)
		{
			Main.EntitySpriteDraw(texture, screenPos, null, new Color(255, 55, 0, 0), Projectile.rotation, texture.Size() * 0.5f, Projectile.scale * .5f * i * .25f, 0, 0f);
		}

		return false;
	}
}

internal sealed class InfernalBoss : ModNPC
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
		Stabbing = 1 << 3,
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
		public int GibLevel;
		public byte MovementGroup;
		public float Layer;
		public (int Stage, float Progress) Animation;
		public bool PlayedSound;
		public bool LiftInAnimation;
		public PhaseType MinPhaseToGib;

		public readonly bool IsGibbedAtAll => GibLevel > 0;
		public readonly bool IsSharpened => GibLevel == 2 || Role.HasFlag(LimbRole.Stabbing);
		public readonly bool IsRenderedUseless => GibLevel > 2;
		public readonly bool IsTotallyGone => GibLevel >= IK.Segments.Length;

		public readonly Limb Copy()
		{
			Limb copy = this;
			copy.IK.Segments = copy.IK.Segments.ToArray();
			copy.IK.Results = copy.IK.Results.ToArray();
			return copy;
		}
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

		public void SetGibbed(int level)
		{
			GibLevel = level;

			int gibbedSegmentIdx = IK.Segments.Length - level;
			for (int segmentIdx = 0; segmentIdx < IK.Segments.Length; segmentIdx++)
			{
				ref IKSegment segment = ref IK.Segments[segmentIdx];
				bool isGibbed = segmentIdx == gibbedSegmentIdx;
				bool isVisible = segmentIdx <= gibbedSegmentIdx;
				segment.Frame.CurrentRow = (byte)(isVisible ? (isGibbed ? 1 : 0) : byte.MaxValue);
			}
		}
	}
	public record struct LimbGroup
	{
		public int NextIndex;
		public float Cooldown;
	}

	private enum AttackType
	{
		None,
		BladeSlash,
		BladeReverse,
		BladeStab,
		Ravage,
		Enflame,
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
		public NPCTeleports Teleports { get; } = npc.GetGlobalNPC<NPCTeleports>();
		public NPCVoice Voice { get; } = npc.GetGlobalNPC<NPCVoice>();
	}
	public record struct BladeTransform(Vector2 Position, float Rotation);

	public const float SecondPhaseHealthFactor = 0.4f;

	private static readonly InverseKinematics.Config ikCfg = new()
	{
		RadianQuantization = MathHelper.ToRadians(4f),
	};
	private static readonly SpriteAnimation animIdle = new() { Id = "idle", Frames = [0], Speed = 3f };
	private static readonly int phase1Music = MusicID.OtherworldlyBoss1;
	private static readonly int phase2Music = MusicID.OtherworldlyPlantera;
	private static int[] minionTypes = [];
	private static Asset<Texture2D>? helmetTexture;
	private static Asset<Texture2D>? bodyTexture;
	private static Asset<Texture2D>? robeTexture;
	private static Asset<Texture2D>? bladeBaseTexture;
	private static Asset<Texture2D>? bladeGlowTexture;
	private static Asset<Effect>? bladeShader;

	public ref BladeTransform Blade => ref BladeHistory[0];
	public BladeTransform[] BladeHistory = new BladeTransform[10];
	private (Vector2 Pos, float Rot) bladeVelocity;

	// Synchronized state:
	public PhaseType Phase = PhaseType.Idle;
	private CutsceneData Cutscene;
	private uint globalCounter;
	private ushort grabCooldown;
	private ushort spawnCooldown;
	private ushort voluntaryAttackCounter;
	private ushort enflameAttackCounter;
	/// <summary> The initiated attack type. Controls blade animation. </summary>
	private AttackType attackType;
	private Limb[] limbs = [];

	// Non-synchronized state:
	private int lifeOld;
	/// <summary> World-relative body angle. </summary>
	private (float Target, float Current) bodyAngle;
	/// <summary> Body-relative head angle. </summary>
	private (float Target, float Current) headAngle;
	/// <summary> Value increased by current movement speed. </summary>
	private float movementAnimation;
	/// <summary> Controls to where and how often the limbs are moved onto new positions when walking. </summary>
	private (Vector2 Direction, float Haste) wishedMovement;
	/// <summary> Array of limb groups, used for movement animations. Resized on demand. </summary>
	private LimbGroup[] limbGroups = [];
	private (SlotId Handle, float Intensity) movementSound;
	private (SlotId Handle, float Intensity) bladeBurnSound;
	private readonly HashSet<Point16> flamePoints = [];

	public bool CutsceneActive => Cutscene.Type != 0;
	public bool CurrentlySitting => Phase is PhaseType.Idle || Cutscene is { Type: CutsceneType.Intro, Counter: < 60 };

	public override void Load()
	{
		GoreLoader.AddGoreFromTexture<SimpleModGore>(Mod, $"{Texture}_Helmet");
	}

	public override void SetStaticDefaults()
	{
		NPCID.Sets.UsesNewTargetting[Type] = true;
		NPCID.Sets.TeleportationImmune[Type] = true;
		NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.OnFire] = true;
		NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.CursedInferno] = true;
		NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Shimmer] = true;
		NPCID.Sets.ShouldBeCountedAsBoss[Type] = true;
		NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, new() { CustomTexturePath = $"{Texture}_Bestiary" });
		Main.npcFrameCount[Type] = 4;

		minionTypes =
		[
			ModContent.NPCType<FallenSavage>(),
			ModContent.NPCType<FallenSchemer>(),
			ModContent.NPCType<FallenTyrant>(),
			ModContent.NPCType<FallenShaman>(),
		];
	}
	public override void SetDefaults()
	{
		NPC.BossBar = ModContent.GetInstance<InfernalBossBar>();
		NPC.aiStyle = -1;
		NPC.lifeMax = 225000;
#if LOW_HEALTH
		NPC.lifeMax = 5000;
#endif
		NPC.defense = 90;
		NPC.damage = 150;
		NPC.width = 125;
		NPC.height = 125;
		NPC.knockBackResist = 0.0f;
		NPC.boss = true;
		NPC.lavaImmune = true;

		NPC.HitSound = new($"{nameof(PathOfTerraria)}/Assets/Sounds/HitEffects/FleshHit", 3) { Volume = 0.85f, Pitch = -0.25f, PitchVariance = 0.3f, MaxInstances = 5 };
		NPC.DeathSound = new($"{nameof(PathOfTerraria)}/Assets/Sounds/HitEffects/PainedScreechSplatter", 3) { Volume = 1.00f, Pitch = -0.25f, Identifier = $"{Name}Death" };

		NPC.TryEnableComponent<NPCMovement>(e =>
		{
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
		SetupLimbs(in ctx);
		SetupBlade(in ctx);

		// Initial cooldowns.
		spawnCooldown = 60 * 3;
	}

	public override void ModifyNPCLoot(NPCLoot npcLoot)
	{
		int chanceCommon = (int)MathF.Ceiling(100f / 15f);
		int chanceUncommon = (int)MathF.Ceiling(100f / 5f);
		int chanceRare = (int)MathF.Ceiling(100f / 2f);

		npcLoot.AddCommon<FirelordsWill>(chanceCommon);
		npcLoot.AddCommon<PyralisHeart>(chanceCommon);
		npcLoot.AddCommon<FallenKingsLegacy>(chanceUncommon);
		npcLoot.AddCommon<ProlifRing>(chanceRare);
		npcLoot.AddCommon<Cryobrand>(chanceRare);
	}

	public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
	{
		if (target.HasBuff<IgnitedDebuff>())
		{
			modifiers.FinalDamage *= 1.3f;
		}
	}

	public override void SendExtraAI(BinaryWriter writer)
	{
		writer.Write((byte)Phase);
		writer.Write((uint)globalCounter);
		writer.Write((ushort)grabCooldown);
		writer.Write((ushort)spawnCooldown);
		writer.Write((ushort)voluntaryAttackCounter);
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
		voluntaryAttackCounter = reader.ReadUInt16();

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
		UpdateBlade(in ctx);
		ShootFireballs(in ctx);
		SpreadTileFlames(in ctx);
		LimbWielding(in ctx);
		LimbGrabbing(in ctx);
		LimbMovement(in ctx);
		LimbFlailing(in ctx);
		BodyMovement(in ctx);
		UpdateBodyAngles(in ctx);
		UpdateLimbs(in ctx);
		GibLimbs(in ctx);
		InitiateAttacks(in ctx);
		ctx.Teleports.ManualUpdate(new(NPC)
		{
			IsBusy = Phase is PhaseType.Idle || CutsceneActive
#if NO_TELEPORTS
			&& false
#endif
		});
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
		// Phase-conditional data.
		if (Phase is PhaseType.Bladewielder or PhaseType.Idle)
		{
			ctx.Movement.Data.MaxSpeed = 20.0f;
			ctx.Movement.Data.Acceleration = 3.6f;
			ctx.Voice.Data.PainSound = (9, SoundID.NPCHit56 with { Pitch = 0f, PitchVariance = 0.5f, Identifier = $"{Name}Hit" });
			ctx.Teleports.Data.MaxCooldown = 120;
		}
		else if (Phase == PhaseType.Abomination)
		{
			ctx.Voice.Data.PainSound = (ChanceX: 5, Style: new($"{nameof(PathOfTerraria)}/Assets/Sounds/HitEffects/PainedScreech", 3)
			{
				SoundLimitBehavior = SoundLimitBehavior.IgnoreNew,
				Pitch = -0.4f,
				PitchVariance = 0.2f,
				Identifier = $"{Name}Hit"
			});
			ctx.Movement.Data.MaxSpeed = 17.8f;
			ctx.Movement.Data.Acceleration = 3.9f;
			ctx.Teleports.Data.MaxCooldown = 180;
		}

		// Start intro.
		if (Phase == PhaseType.Idle && !CutsceneActive)
		{
			const float introRange = 512;
			foreach (Player player in Main.ActivePlayers)
			{
				if (player.WithinRange(ctx.Center, introRange))
				{
					Phase = PhaseType.Bladewielder;
					Cutscene = new(CutsceneType.Intro, 0, 250);
					break;
				}
			}
		}

		// Switch to second phase.
		if (!CutsceneActive && Phase == PhaseType.Bladewielder
#if !FORCE_PHASE_II
		&& NPC.life < NPC.lifeMax * SecondPhaseHealthFactor
#endif
		)
		{
			Phase = PhaseType.Abomination;
			Cutscene = new(CutsceneType.Transformation, 0, 600);
			NPC.life = (int)(NPC.lifeMax * SecondPhaseHealthFactor);
		}

		// Faint glow in the second phase.
		if (!Main.dedServ && Phase is PhaseType.Abomination)
		{
			Lighting.AddLight(ctx.Center, new Vector3(1.0f, 0.2f, 0.8f) * 0.5f);
		}
	}
	private void IdentityLogic(in Context ctx)
	{
		_ = ctx;
		NPC.dontTakeDamage = Phase == PhaseType.Idle || CutsceneActive;
		NPC.boss = Phase != PhaseType.Idle;
		Music = Phase switch
		{
			PhaseType.Bladewielder => phase1Music,
			PhaseType.Abomination => phase2Music,
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
					Identifier = $"{nameof(InfernalBoss)}_Intro",
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
				// Throw the sword.
				bladeVelocity.Pos = new Vector2(0, -21) + Main.rand.NextVector2Circular(5f, 3f);
				bladeVelocity.Rot = MathHelper.TwoPi * 20f * (Main.rand.NextBool() ? +1 : -1);

				// Kill minions.
				NPCUtil.KillAllWithType(minionTypes);

				if (!Main.dedServ)
				{
					// Spawn helmet gore.
					int helmetGore = ModContent.Find<ModGore>($"{nameof(PathOfTerraria)}/{Name}_Helmet").Type;
					var helmetPos = (Vector2)(ctx.Center + new Vector2(NPC.direction * 80, -20));
					var helmetVel = (Vector2)((NPC.velocity * 0.5f) + new Vector2(NPC.direction * 5, -8));
					Gore.NewGorePerfect(NPC.GetSource_FromThis(), helmetPos, helmetVel, helmetGore);

					// Skip music fade-in.
					Main.musicFade[phase1Music] = 0f;
					Main.musicFade[phase2Music] = 1f;

					CameraCurios.Create(new()
					{
						Identifier = $"{nameof(InfernalBoss)}_Phase2",
						Weight = 0.5f,
						LengthInSeconds = lengthInSeconds,
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
			}
			else if (justEnded)
			{
				// Grow new limbs!
				SetupLimbs(in ctx);

				// Prevent immediate teleportation & flame attacks.
				ctx.Teleports.Data.Cooldown.Set(600);
				enflameAttackCounter = 0;
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
		}
		else if (Cutscene.Type == CutsceneType.Death)
		{
			if (initialize)
			{
				// Kill minions.
				NPCUtil.KillAllWithType(minionTypes);

				// Mark all limbs as gibbable.
				foreach (ref Limb limb in limbs.AsSpan())
				{
					limb.CanBeGibbed = true;
					limb.MinPhaseToGib = 0;
				}

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
						Identifier = $"{nameof(InfernalBoss)}_Death",
						Weight = 0.5f,
						LengthInSeconds = lengthInSeconds,
						FadeOutLength = 0.2f,
						Position = ctx.Center,
						Range = new(Min: 1024, Max: 3072, Exponent: 1.5f),
						Callback = new NPCTracker(NPC).Center,
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

				// Bloody effects.
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
		}

		Cutscene.Initialized = true;
		if (justEnded) { Cutscene = default; }
		else { Cutscene.Counter++; }
	}

	// Trigger death cutscene.
	public override bool CheckDead()
	{
		if (Cutscene.Type != CutsceneType.Death || Cutscene.Counter != Cutscene.Length)
		{
			if (Cutscene.Type != CutsceneType.Death) { Cutscene = new(CutsceneType.Death, 0, 240); }
			Phase = PhaseType.Abomination;
			NPC.dontTakeDamage = true;
			NPC.life = 1;
			return false;
		}

		return true;
	}

	// Prevent despawn in idle state.
	public override bool CheckActive()
	{
		return false;
	}

	private void BodyMovement(in Context ctx)
	{
		NPC.GravityMultiplier = MultipliableFloat.One * 2.5f;
		
		int numTotalLimbs = limbs.Count(l => l.Role.HasFlag(LimbRole.Walking));
		int numAvailableLimbs = limbs.Count(l => l.Role.HasFlag(LimbRole.Walking) && !l.IsRenderedUseless);
		int numAttachedLimbs = limbs.Count(l => l.TileAttachment != null && l.Role.HasFlag(LimbRole.Walking) && !l.IsRenderedUseless);
		bool isSwimming = NPC.wet || NPC.lavaWet;
		// bool isClimbing = numAttachedLimbs >= Math.Clamp(numAvailableLimbs * 0.3f, 1, 4) || isSwimming;
		// NPC.noGravity = isClimbing;
		NPC.noGravity = false;

		bool hasTarget = NPC.HasValidTarget && !CurrentlySitting;
		NPCAimedTarget target = NPC.GetTargetData(ignorePlayerTankPets: true);
		Vector2 targetPos = target.Center;
		Vector2 walkPos = targetPos;
		bool fixedPosition = false;

		// Go to the center of the arena if lacking a target in the infernal realm.
		if (!hasTarget)
		{
			bool inNativeArena = SubworldSystem.IsActive<InfernalRealm>();
			Vector2? arenaCenter = inNativeArena ? InfernalRealm.ArenaCenter.Get()?.ToWorldCoordinates(+8, -128) : null;
			targetPos = arenaCenter ?? ctx.Center;
			walkPos = targetPos;
			fixedPosition = true;
		}

		// In phase I, when using the sword, keep some distance away from the target and also rotate around it.
		const float approachDistance = 290;
		if (Phase == PhaseType.Bladewielder && hasTarget)
		{
			float strafingAngle = globalCounter * -0.2f * MathHelper.TwoPi;
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
		}

#if FOLLOW_MOUSE
		walkPos = Main.MouseWorld;
#endif

		// Stand up, but only if that doesn't prevent the creature from getting into tight spaces.
		Vector2Int sizeInTiles = (NPC.Size * new Vector2(0.9f, 2.2f)).ToTileCoordinates();
		if (!fixedPosition && TileUtils.TryFitRectangleIntoTilemap(walkPos.ToTileCoordinates() - new Point(0, 1), sizeInTiles, out Vector2Int adjustedPoint))
		{
			walkPos = (adjustedPoint + (sizeInTiles * 0.5f)).ToWorldCoordinates();
		}

		float maxSpeed = ctx.Movement.Data.MaxSpeed;
		float acceleration = ctx.Movement.Data.Acceleration * TimeSystem.LogicDeltaTime;
		Vector2 targetDir = ctx.Center.SafeDirection(targetPos, Vector2.UnitY);
		Vector2 walkDir = ctx.Center.SafeDirection(walkPos, Vector2.UnitY);

		// Slow down during attacks.
		if (ctx.Attacking.Active && attackType is not AttackType.Enflame and not AttackType.Ravage)
		{
			float effectFactor = 1f - MathF.Pow(MathUtils.Clamp01(ctx.Attacking.Progress / (float)ctx.Attacking.Data.LengthInTicks), 3f);
			maxSpeed *= MathHelper.Lerp(1f, 0.90f, effectFactor);
			acceleration *= MathHelper.Lerp(1f, 1.00f, effectFactor);
		}
		
		// Speed up in second phase.
		if (Phase == PhaseType.Abomination)
		{
			acceleration *= 1.1f;
			maxSpeed *= 1.1f;
		}

		if (Phase == PhaseType.Idle && !CutsceneActive)
		{
			NPC.velocity = default;
			NPC.Center = walkPos + new Vector2(0, -8);
			maxSpeed = 0.0f;
			acceleration = 0f;
		}
		else if (CutsceneActive)
		{
			NPC.velocity *= 0.98f;

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

		// DebugUtils.DrawStringInWorld("walkPos", walkPos, Color.Beige);
		// DebugUtils.DrawStringInWorld($"speed: {(NPC.velocity / maxSpeed).Length():0.00}", walkPos + new Vector2(0, 32), Color.Beige);
		wishedMovement = (walkDir, maxSpeed * 0.75f);

		// Occasional "coldblooded" movement & attack cooldown periods for the second phase.
		if (Phase is PhaseType.Abomination && !CutsceneActive)
		{
			float sine1 = MathUtils.Sin01(globalCounter / 40f);
			float sine2 = MathUtils.Sin01(globalCounter / 70f);
			float sine = MathF.Max(sine1, sine2);
			const float cutoff = 0.20f;
			bool canMove = sine > cutoff;
			wishedMovement.Haste *= canMove ? 1.5f : 0.3f;
			acceleration *= canMove ? 1.0f : 0.5f;
			maxSpeed *= canMove ? 1.5f : 0.12f;

			if (!canMove)
			{
				const int CooldownPeriod = 60;
				ctx.Teleports.Data.Cooldown.SetIfGreater(CooldownPeriod);
				// ctx.Attacking.Data.Cooldown.SetIfGreater(CooldownPeriod);
			}
		}

		if (fixedPosition)
		{
			NPC.Center = walkPos;
			NPC.velocity = default;
		}
		// Pull the body towards tile-grabbing limbs.
		else if (!ctx.Attacking.Active || ctx.Attacking.Progress < ctx.Attacking.Data.Dash.Start || ctx.Attacking.Progress >= ctx.Attacking.Data.Dash.End)
		{
			float factorSum = 0f;
			foreach (ref Limb limb in limbs.AsSpan())
			{
				if (limb.TileAttachment == null) { continue; }
				if (limb.Animation.Progress < 0.8f && limb.TargetPosition.Distance(limb.IK.Target) > 12) { continue; }

				Vector2 logicalLimbPos = limb.IK.GetPosition(ctx.Center, rotation: bodyAngle.Current, xDir: NPC.spriteDirection);
				Vector2 tileTarget = limb.TileAttachment.Value.ToWorldCoordinates();
				Vector2 pullDir = logicalLimbPos.DirectionTo(tileTarget);
				float distance = logicalLimbPos.Distance(tileTarget);
				float limbLength = limb.IK.Length;
				float dotFactor = Utils.Remap(Vector2.Dot(pullDir, walkDir), -0.1f, +0.5f, +0.1f, +1.0f);
				float dstFactor = MathF.Pow(1f - MathUtils.DistancePower(distance, 10f, limbLength), 1f);
				float totalFactor = dstFactor * dotFactor;
				float limbPull = acceleration * totalFactor;

				factorSum += totalFactor;

				NPC.velocity = MovementUtils.DirAccel(NPC.velocity, pullDir, maxSpeed, limbPull);
			}

			NPC.velocity += -Vector2.UnitY * NPC.gravity * MathF.Min(1f, factorSum);
		}

		// Apply hard & soft speed capping.
		if (maxSpeed > 0f)
		{
			float hardCap = maxSpeed * 2.50f;
			float softCap = maxSpeed * 1.00f;
			float softLoss = 8 * TimeSystem.LogicDeltaTime;
			NPC.velocity = MovementUtils.ApplyHardSpeedCap(NPC.velocity, hardCap);
			NPC.velocity = MovementUtils.ApplySoftSpeedCap(NPC.velocity, softCap, softLoss);
		}

		// Do not change the direction if the target is close or if an attack is ongoing.
		if (hasTarget && !ctx.Attacking.Active && Math.Abs(targetPos.X - ctx.Center.X) > 100)
		{
			NPC.direction = targetDir.X >= 0f ? 1 : -1;
		}
		// Face left when idle.
		else if (Phase is PhaseType.Idle || Cutscene.Type is CutsceneType.Intro)
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

		if (CurrentlySitting)
		{
			bodyAngle = default;
			headAngle = default;
		}
		else
		{
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

		enflameAttackCounter++;

		if (ctx.Attacking.Active) { return; }

		// Do not start attacks when holding a player.
		if (limbs.Any(l => l.EntityAttachment.Player != null)) { return; }

		Vector2 targetCenter = ctx.Targeting.GetTargetCenter(NPC);
		float distance = targetCenter.Distance(ctx.Center);
		AttackType nextAttackType = attackType switch
		{
			AttackType.BladeSlash when Phase is PhaseType.Bladewielder => AttackType.BladeReverse,
			AttackType.BladeReverse when Phase is PhaseType.Bladewielder => AttackType.BladeStab,
			_ when Phase is PhaseType.Bladewielder => AttackType.BladeSlash,
			_ when Phase is PhaseType.Abomination => AttackType.Ravage,
			_ => AttackType.None,
		};
		float attackRange = nextAttackType switch
		{
			AttackType.BladeStab => 550,
			AttackType.BladeSlash or AttackType.BladeReverse => 350,
			AttackType.Ravage => 300,
			_ => 0,
		};

		// An attack will be triggered regardless of range if enough time passes without cathing up.
		// This is done to spread flames in order to interfere with the player's escape.
		const int voluntaryAttackRate = (int)(60 * 1.1f);
		bool inRange = distance < attackRange;
		bool voluntaryAttack = Phase is PhaseType.Bladewielder && voluntaryAttackCounter > voluntaryAttackRate;
		bool ignoreRange = voluntaryAttack || swingingDuringIntro;

		// Occassional flame bursts.
		int enflameAttackRate = (int)(60 * (Phase is PhaseType.Bladewielder ? 9 : 6));
		if (enflameAttackCounter > enflameAttackRate)
		{
			nextAttackType = AttackType.Enflame;
			ignoreRange = true;
		}

		if (nextAttackType != AttackType.None && (inRange || ignoreRange) && ctx.Attacking.TryStarting(new(NPC)))
		{
			attackType = nextAttackType;
			voluntaryAttackCounter = 0;

			// Reset dynamic data.
			ctx.Attacking.Data.Hitbox = default;
			ctx.Attacking.Data.Damage = default;
			ctx.Attacking.Data.Dash = default;
			ctx.Attacking.Data.Slash = null;
			ctx.Attacking.Data.Sounds = [];
			// Configure dynamic data.
			if (attackType is AttackType.BladeSlash or AttackType.BladeReverse or AttackType.BladeStab)
			{
				ctx.Attacking.Data.LengthInTicks = 90;
				ctx.Attacking.Data.CooldownLength = 0;
				ctx.Attacking.Data.Damage = (45, 60, DamageInstance.EnemyAttackFilterWithInfighting);
				ctx.Attacking.Data.Dash = (50, 55, new(30, 25));
				ctx.Attacking.Data.Movement = (0.0f, 0.80f, 0.95f);
				
				if (!Main.dedServ)
				{
					ctx.Attacking.Data.Sounds =
					[
						(23, new($"{nameof(PathOfTerraria)}/Assets/Sounds/Conflux/PyralisBlade", 3) { MaxInstances = 3, Volume = 2.0f, Pitch = -0.1f, PitchVariance = 0.0f }),
						(24, new($"{nameof(PathOfTerraria)}/Assets/Sounds/Conflux/PyralisBlade", 3) { MaxInstances = 3, Volume = 2.0f, Pitch = -0.1f, PitchVariance = 0.0f }),
					];
				}

				if (attackType is AttackType.BladeStab)
				{
					ctx.Attacking.Data.AimLag = ((new(0.01f), new(0.06f)), (0f, 0.50f));
					ctx.Attacking.Data.Hitbox = (new(240, 240), new(+500, +500), new(+0, +0));
				}
				else
				{
					ctx.Attacking.Data.AimLag = ((new(0.01f), new(0.15f)), (0f, 0.80f));
					ctx.Attacking.Data.Hitbox = (new(360, 360), new(+280, +280), new(+0, +0));
				}
			}
			else if (attackType is AttackType.Ravage)
			{
				ctx.Attacking.Data.LengthInTicks = 40;
				ctx.Attacking.Data.CooldownLength = 20;
				ctx.Attacking.Data.AimLag = ((new(0.0f), new(0.15f)), (0f, 0.99f));
				ctx.Attacking.Data.Hitbox = (new(340, 340), new(48, 48), new(0, 0));
				ctx.Attacking.Data.Damage = (25, 33, DamageInstance.EnemyAttackFilter);
				ctx.Attacking.Data.Dash = (5, 20, new(29, 24));
				ctx.Attacking.Data.Movement = (0.0f, 0.80f, 0.95f);

				if (!Main.dedServ)
				{
					ctx.Attacking.Data.Slash = (ModContent.Request<Texture2D>($"{nameof(PathOfTerraria)}/Assets/Misc/Slash"), null, Color.IndianRed);
					ctx.Attacking.Data.Sounds =
					[
						(15, new($"{nameof(PathOfTerraria)}/Assets/Sounds/Conflux/PyralisBlade", 3) { MaxInstances = 3, Volume = 1.5f, Pitch = +0.5f, PitchVariance = 0.2f }),
						(0, new($"{nameof(PathOfTerraria)}/Assets/Sounds/Conflux/PyralisThrow", 3) { MaxInstances = 3, Volume = 0.9f, Pitch = -0.3f, PitchVariance = 0.2f }),
					];
				}
			}
			else if (attackType is AttackType.Enflame)
			{
				enflameAttackCounter = 0;
				ctx.Attacking.Data.LengthInTicks = 120;
				ctx.Attacking.Data.CooldownLength = 30;
				ctx.Attacking.Data.Damage = (75, 75, (EntityKind)0);
				ctx.Attacking.Data.Movement = (0.0f, 0.95f, 0.95f);

				if (!Main.dedServ)
				{
					bool poweredUp = Phase is PhaseType.Abomination;
					var style = new SoundStyle($"{nameof(PathOfTerraria)}/Assets/Sounds/Conflux/PyralisEnflame", 3)
					{
						Volume = 2.00f,
						PitchVariance = 0.1f,
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
		else if (Phase is PhaseType.Bladewielder)
		{
			voluntaryAttackCounter = (ushort)Math.Min(voluntaryAttackCounter + 1, ushort.MaxValue - 2);
		}
	}

	private void ShootFireballs(in Context ctx)
	{
		if (Phase is PhaseType.Idle) { return; }
		if (CutsceneActive) { return; }
		if (!ctx.Attacking.Active) { return; }
		if (attackType != AttackType.Enflame) { return; }

		if (!Main.dedServ)
		{
			float midTick = MathHelper.Lerp(ctx.Attacking.Data.Damage.Start, ctx.Attacking.Data.Damage.End, 0.1f);
			float intensity = MathF.Pow(MathUtils.DistancePower(MathF.Abs(ctx.Attacking.Progress - midTick), 0, 60), 2f);
			Lighting.AddLight(ctx.Center, Color.Orange.ToVector3() * 3f * intensity);
		}

		if (Main.netMode == NetmodeID.MultiplayerClient) { return; }

		float progressFactor = ctx.Attacking.Progress / (float)ctx.Attacking.Data.LengthInTicks;
		var source = (IEntitySource)NPC.GetSource_FromThis();
		
		int tickBase = ctx.Attacking.Data.Damage.Start;
		bool poweredUp = Phase is PhaseType.Abomination;
		int numInstances = poweredUp ? 3 : 1;
		int numProjectiles = poweredUp ? 12 : 24;
		for (int instance = 0; instance < numInstances; instance++)
		{
			if (ctx.Attacking.Progress != tickBase + (instance * 15)) { continue; }

			float instanceFactor = instance / (float)(numInstances);
			float baseAngle = instance * MathHelper.PiOver4;
			float baseSpeed = MathHelper.Lerp(12, 18, 1f - instanceFactor);

			for (int i = 0; i < numProjectiles; i++)
			{
				float iFactor = i / (float)numProjectiles;
				int tick = (int)(ctx.Attacking.Data.LengthInTicks * iFactor);
			
				var projPos = (Vector2)ctx.Center;
				float projSpeed = (float)(baseSpeed + Main.rand.NextFloat(0, 10));
				var projVel = (Vector2)((baseAngle + (iFactor * MathHelper.TwoPi)).ToRotationVector2() * projSpeed);
				int projType = ModContent.ProjectileType<InfernalFlames>();
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

		// Find the limb to use for grabbing.
		bool willThrow = true; //Phase != PhaseType.Abomination;
		if (Array.FindIndex(limbs, l => l.Role.HasFlag(LimbRole.EntityGrabbing) && !l.EntityAttachment.IsValid) is int limbIndex && limbIndex < 0 && willThrow) { return; }
		ref Limb limb = ref (willThrow ? ref limbs[limbIndex] : ref Unsafe.NullRef<Limb>());

		int numSavages = 0, numSchemers = 0, numTyrants = 0, numShamans = 0;
		foreach (NPC npc in Main.ActiveNPCs)
		{
			if (npc.type == ModContent.NPCType<FallenSavage>()) { numSavages++; }
			else if (npc.type == ModContent.NPCType<FallenSchemer>()) { numSchemers++; }
			else if (npc.type == ModContent.NPCType<FallenTyrant>()) { numTyrants++; }
			else if (npc.type == ModContent.NPCType<FallenShaman>()) { numShamans++; }
		}

		bool phase2 = Phase is PhaseType.Abomination;
		int type = 0;
		int numSteps = phase2 ? 5 : 3;
		for (int i = 1; i <= numSteps; i++)
		{
			// In phase II, only spawn shamans and schemers.
			if (phase2)
			{
				if (numShamans < 3) { type = ModContent.NPCType<FallenShaman>(); }
				else if (numSchemers < 10) { type = ModContent.NPCType<FallenSchemer>(); }
				break;
			}

			if (numSavages < i * 1.5f) { type = ModContent.NPCType<FallenSavage>(); break; }
			else if (numSchemers < i) { type = ModContent.NPCType<FallenSchemer>(); break; }
			else if (numTyrants < i) { type = ModContent.NPCType<FallenTyrant>(); break; }
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
			if (willThrow)
			{
				limb.ResetState();
				limb.EntityAttachment = (EntityRef)npc;
				grabCooldown = 120;
			}

			bool spawnedImportantShaman = Phase is PhaseType.Abomination && type == ModContent.NPCType<FallenShaman>();
			spawnCooldown = (ushort)(spawnedImportantShaman ? 500 : 250);
		}
	}

	private void ResetOffsets(in Context ctx)
	{
		_ = ctx;

		if (CurrentlySitting)
		{
			NPC.gfxOffY = -275;
		}
		else
		{
			NPC.gfxOffY = MathF.Sin(((float)Main.timeForVisualEffects / 25f) + (movementAnimation / 40f)) * 6f;
		}

#if IK_PREVIEW
		NPC.gfxOffY = 0f;
#endif
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
				Identifier = $"{nameof(InfernalBoss)}{(fightStarted ? "" : "_Idle")}",
				Weight = fightStarted ? 0.2f : 0.3f,
				Zoom = fightStarted ? 0f : 0.5f,
				LengthInSeconds = 1f,
				Position = ctx.Center + (!introStarted ? new Vector2(0, +128) : default),
				Range = fightStarted ? new(Min: 1000, Max: 2000, Exponent: 2.0f) : new(Min: 300, Max: 1000, Exponent: 1.5f),
				Callback = fightStarted ? new NPCTracker(NPC).Center : null,
			});
		}

		// Movement sound.
		{
			bool quiet = Cutscene.Type == CutsceneType.Death || CurrentlySitting;
			var loopSound = new SoundStyle($"{nameof(PathOfTerraria)}/Assets/Sounds/Gore/FleshLoopChaotic") { Volume = 0.1f, IsLooped = true, PauseBehavior = PauseBehavior.PauseWithGame };
			float halfStep = 1f * TimeSystem.LogicDeltaTime;
			float target = quiet ? 0 : MathUtils.Clamp01(NPC.velocity.Length() * 0.1f);
			float intensity = movementSound.Intensity = MathUtils.StepTowards(MathHelper.Lerp(movementSound.Intensity, target, halfStep), target, halfStep);
			float volume = MathHelper.Lerp(quiet ? 0 : 0.75f, 1.00f, intensity);
			float pitch = MathHelper.Lerp(-0.9f, +0.2f, intensity);
			SoundUtils.UpdateLoopingSound(ref movementSound.Handle, ctx.Center, volume, pitch, loopSound, _ => Main.npc[NPC.whoAmI] is { active: true } n && n == NPC);
		}
	}

	private void BladeEffects(in Context ctx)
	{
		if (Main.dedServ) { return; }
#if HIDE_SWORD
		return;
#endif

		bool isSitting = CurrentlySitting;

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
					Lighting.AddLight(point, Color.Orange.ToVector3() * 3f);
					continue;
				}

				if (isSitting) { continue; }
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

			var loopSound = new SoundStyle($"{nameof(PathOfTerraria)}/Assets/Sounds/Conflux/PyralisBladeBurn") { Volume = 0.60f, IsLooped = true, PauseBehavior = PauseBehavior.PauseWithGame };
			float halfStep = 8 * TimeSystem.LogicDeltaTime;
			float distanceTarget = MathF.Pow(MathUtils.DistancePower(minDistance, 64, 1024), 2.3f);
			float slicingTarget = MathUtils.Clamp01(numLevelsSliced / 3f) * 0.5f;
			float target = MathF.Max(distanceTarget, slicingTarget);
			float intensity = bladeBurnSound.Intensity = MathUtils.StepTowards(MathHelper.Lerp(bladeBurnSound.Intensity, target, halfStep), target, halfStep);
			float volume = MathHelper.Lerp(0.01f, 1.0f, intensity);
			float pitch = sufficientlyDeepInGround ? +0.5f : +0.0f;
			SoundUtils.UpdateLoopingSound(ref bladeBurnSound.Handle, ctx.Center, volume, pitch, loopSound, _ => Main.npc[NPC.whoAmI] is { active: true } n && n == NPC);
		}
	}

	private void SpreadTileFlames(in Context ctx)
	{
#if NO_FLAMES
		return;
#endif

		if (Main.netMode == NetmodeID.MultiplayerClient) { return; }
		if (Phase is PhaseType.Idle || CutsceneActive) { return; }
		if (Phase is PhaseType.Abomination) { return; }

		if (ctx.Attacking.DealingDamage == (Phase is PhaseType.Abomination))
		{
			flamePoints.Clear();
			return;
		}

		(Vector2 origin, float angle) = Blade;

		if (Phase is PhaseType.Abomination)
		{
			origin = ctx.Center;
			angle = ctx.Attacking.Data.Angle;
		}

		Vector2 forward = angle.ToRotationVector2();
		Vector2 sideways = forward.RotatedBy(MathHelper.PiOver2);
		(float Start, float End, float Step) extents = attackType != AttackType.BladeStab ? (300, 900, 32) : (200, 1100, 32);
		(float Start, float End, float Step) offsets = attackType != AttackType.BladeStab ? (-32, +32, 32) : (-64, +64, 32);
		if (Phase is PhaseType.Abomination)
		{
			extents = (300, 500, 32);
			offsets = (0, 1, 1);
		}

		for (float extent = extents.Start; extent < extents.End; extent += extents.Step)
		{
			for (float offset = offsets.Start; offset <= offsets.End; offset += offsets.Step)
			{
				Vector2 point = origin + (forward * extent) + (sideways * offset);
				Point16 tilePoint = point.ToTileCoordinates16();
				(int x, int y) = (tilePoint.X, tilePoint.Y);

				if (x < 1 || y < 1 || x >= Main.maxTilesX - 1 || y >= Main.maxTilesY - 1) { continue; }
				if (!WorldUtilities.SolidTile(x, y)) { continue; }
				if (!IsTileSuitableForFlames(x, y)) { continue; }
				if (!flamePoints.Add(tilePoint)) { continue; }

				point = tilePoint.ToWorldCoordinates(8, 0);

				int projType = ModContent.ProjectileType<InfernalFlames>();
				Vector2 projPos = origin;
				Vector2 projVel = default;
				float ai0 = Main.rand.NextFloat(0.00f, 0.05f);
				var proj = Projectile.NewProjectileDirect(null, projPos, projVel, projType, 150, 0f, ai0: ai0, ai1: point.X, ai2: point.Y);
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
		var gibbableFraming = new SpriteFrame(4, 2) { PaddingX = 0, PaddingY = 0 };

		// When facing right...
		limbs =
		[
			// Front right limb.
			new Limb()
			{
				MovementGroup = 1,
				Role = LimbRole.Walking,
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
				MovementGroup = 1,
				Role = LimbRole.Walking | LimbRole.EntityGrabbing,
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
				MovementGroup = 2,
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
				MovementGroup = 2,
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
				MovementGroup = 3,
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
				MovementGroup = 4,
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
				MovementGroup = 4,
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
				Role = LimbRole.Wielding,
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
			List<Limb> list = [];
			for (int i = 0; i < limbs.Length * 3; i++)
			{
				int pass = i / limbs.Length;
				int baseIndex = i & limbs.Length;
				ref readonly Limb oldLimb = ref limbs[i % limbs.Length];
				Limb newLimb = oldLimb.Copy();

				if (pass != 0 && oldLimb.Role.HasFlag(LimbRole.Wielding)) { continue; }
				if (oldLimb.MovementGroup is 3 or 4) { continue; }

				if (pass == 1)
				{
					newLimb.IK.Offset.X *= 1.2f;
					newLimb.IK.Offset.X += -8;
					newLimb.IK.Offset.Y *= 0.8f;
					newLimb.IK.Offset.Y += +16;
					newLimb.IK.IsFlipped ^= true;
				}
				else if (pass == 2)
				{
					newLimb.IK.Offset.X *= 1.2f;
					newLimb.IK.Offset.X += -8;
					newLimb.IK.Offset.Y *= 0.8f;
					newLimb.IK.Offset.Y += +16;
				}

				newLimb.MovementGroup += (byte)(5 * pass);

				// newLimb.IK.RestingAngle = MathF.Abs(newLimb.IK.RestingAngle - MathHelper.Pi);
				newLimb.CanBeGibbed = true;

				newLimb.Role &= ~LimbRole.EntityGrabbing;
				bool useForStabbing = !oldLimb.Role.HasFlag(LimbRole.Wielding);
				if (oldLimb.MovementGroup == 4)
				{
					newLimb.Role &= ~LimbRole.Stabbing;
				}
				else if (useForStabbing)
				{
					newLimb.Role |= LimbRole.Stabbing;
					newLimb.IK.Segments = newLimb.IK.Segments.Take(newLimb.IK.Segments.Length - 1).ToArray();
					newLimb.SetGibbed(1);
				}
				else
				{
					newLimb.IK.Segments = newLimb.IK.Segments.Take(newLimb.IK.Segments.Length - 1).ToArray();
					newLimb.SetGibbed(0);
					// newLimb.Role &= ~LimbRole.Wielding;
					newLimb.Role |= LimbRole.EntityGrabbing;
				}

				list.Add(newLimb);
			}

			limbs = list.ToArray();
		}

		// Default and calculated data.
		foreach (ref Limb limb in limbs.AsSpan())
		{
			limb.IK.Target = NPC.Center;
			limb.TargetPosition = NPC.Center;
			limb.StartPosition = NPC.Center;
		}

		// Prepare limb groups.
		Array.Resize(ref limbGroups, limbs.Max(l => l.MovementGroup) + 1);
		for (int group = 0; group < limbGroups.Length; group++)
		{
			Func<Limb[], Predicate<Limb>, int> search = group % 2 == 0 ? Array.FindIndex<Limb> : Array.FindLastIndex<Limb>;
			limbGroups[group].NextIndex = Math.Max(0, search(limbs, l => l.MovementGroup == group && l.Role.HasFlag(LimbRole.Walking)));
			limbGroups[group].Cooldown = group / (float)limbGroups.Length;
		}
	}

	private void LimbMovement(in Context ctx)
	{
		_ = ctx;
		Vector2 logicalBodyCenter = ctx.Center;
		Vector2 visualBodyCenter = logicalBodyCenter + new Vector2(0f, NPC.gfxOffY);

		// Cooldown is reduced by the haste value.
		int totalMovementLimbs = limbs.Count(l => !l.IsRenderedUseless && l.Role.HasFlag(LimbRole.Walking));
		for (int groupIdx = 0; groupIdx < limbGroups.Length; groupIdx++)
		{
			ref LimbGroup group = ref limbGroups[groupIdx];
			int groupMovementLimbs = limbs.Count(l => l.MovementGroup == groupIdx && !l.IsRenderedUseless && l.Role.HasFlag(LimbRole.Walking));
			float coolSpeed = wishedMovement.Haste * groupMovementLimbs * 0.15f;
			group.Cooldown = MathUtils.StepTowards(group.Cooldown, 0f, coolSpeed * TimeSystem.LogicDeltaTime);
			group.NextIndex = Math.Clamp(group.NextIndex, 0, limbs.Length - 1);
		}

		static bool IsLimbAvailable(in Limb limb)
		{
			return !limb.IsRenderedUseless && !limb.EntityAttachment.IsValid && limb.BladeAttachment == null;
		}

		int limbIndex = -1;
		bool placeAll = limbs.All(l => l.TileAttachment == null);
		bool playedSounds = false;

		using var groupsToAdvance = new RentedArray<bool>(limbGroups.Length);
		Array.Clear(groupsToAdvance);

		foreach (ref Limb limb in limbs.AsSpan())
		{
			limbIndex++;

			if (!limb.Role.HasFlag(LimbRole.Walking)) { continue; }

			ref LimbGroup group = ref limbGroups[limb.MovementGroup];
			float releaseReach = limb.IK.Length + 32;
			bool upNext = group.Cooldown <= 0f && limbIndex == group.NextIndex;
			bool grabTerrain = placeAll || upNext;
			bool isBusy = !IsLimbAvailable(in limb);
			Vector2 visualLimbPos = limb.IK.GetPosition(visualBodyCenter, rotation: bodyAngle.Current, xDir: NPC.spriteDirection);
			Vector2 logicalLimbPos = limb.IK.GetPosition(logicalBodyCenter, rotation: bodyAngle.Current, xDir: NPC.spriteDirection);

			// If already attached (to a tile), advance order if needed.
			if (!isBusy && !upNext
			&& limb.TileAttachment?.ToWorldCoordinates() is { } attach
			&& attach.WithinRange(logicalLimbPos, releaseReach))
			{
				limb.TargetPosition = attach;
			}
			// Otherwise, if walking, find a terrain point to grab.
			else if (!isBusy && grabTerrain && PickTileTarget(in ctx, ref limb, out Point16 tilePos))
			{
				groupsToAdvance[limb.MovementGroup] = true;
				group.Cooldown = 1f;
				limb.ResetState();
				limb.LiftInAnimation = limb.TileAttachment != null;
				limb.TileAttachment = tilePos;
				limb.TargetPosition = ((Point16)tilePos).ToWorldCoordinates();

#if DEBUG && DEBUG_GIZMOS
				DebugUtils.DrawStringInWorld("X", tilePos.ToWorldCoordinates(), Color.Red);
#endif
			}
			// Release attachment if all else fails.
			else
			{
				if (!isBusy) { limb.ResetState(); }
				else { limb.TileAttachment = null; }
				continue;
			}

			float animSpeed = MathHelper.Lerp(MathUtils.Clamp01(limb.StartPosition.Distance(limb.TargetPosition) / 400), 1.5f, 1.4f);
			limb.Animation.Progress = MathUtils.StepTowards(limb.Animation.Progress, 1f, animSpeed * TimeSystem.LogicDeltaTime);

			const float soundRange = 16;
			if (!Main.dedServ && limb.TileAttachment != null && !playedSounds && !limb.PlayedSound
			&& Phase is not PhaseType.Idle
			&& limb.IK.Target.Distance(limb.TargetPosition) <= soundRange
			&& limb.StartPosition.Distance(limb.TargetPosition) > soundRange)
			{
				(playedSounds, limb.PlayedSound) = (true, true);

				Main.instance.CameraModifiers.Add(new PunchCameraModifier(limb.TargetPosition, new Vector2(0, -2), 1f, 4f, 15, 1000f, $"Footstep{limbIndex}"));

				if (limb.IsSharpened)
				{
					SoundEngine.PlaySound(position: limb.TargetPosition, style: new($"{nameof(PathOfTerraria)}/Assets/Sounds/Conflux/PyralisCrawl", 9)
					{
						MaxInstances = 30,
						Volume = Main.rand.NextFloat(0.10f, 0.25f),
						Pitch = -0.1f,
						PitchVariance = 0.2f,
						SoundLimitBehavior = SoundLimitBehavior.ReplaceOldest,
					});
				}
				else
				{
					SoundEngine.PlaySound(position: limb.TargetPosition, style: new($"{nameof(PathOfTerraria)}/Assets/Sounds/Footsteps/FleshyStomp", 3)
					{
						MaxInstances = 40,
						Volume = 0.25f,
						Pitch = -0.1f,
						PitchVariance = 0.2f,
					});
				}
			}
		}

		// Enqueue the next walking limbs.
		if (totalMovementLimbs > 0)
		{
			for (int groupIdx = 0; groupIdx < groupsToAdvance.Length; groupIdx++)
			{
				if (!groupsToAdvance[groupIdx]) { continue; }

				ref int idx = ref limbGroups[groupIdx].NextIndex;
				int startIndex = idx;
				do { idx = MathUtils.Modulo(idx + NPC.spriteDirection, limbs.Length); }
				while (idx != startIndex && (limbs[idx].MovementGroup != groupIdx || !limbs[idx].Role.HasFlag(LimbRole.Walking) || !IsLimbAvailable(in limbs[idx])));
			}
		}
	}
	private void LimbWielding(in Context ctx)
	{
		foreach (ref Limb limb in limbs.AsSpan())
		{
			if (!limb.Role.HasFlag(LimbRole.Wielding)) { continue; }

			// Reset blade hold points.
			bool wasWielding = limb.BladeAttachment != null;
			limb.BladeAttachment = null;

			if (limb.EntityAttachment.IsValid) { goto Reset; }
			if (Phase > PhaseType.Bladewielder) { goto Reset; }
			// If wielding, find a blade point to grab.
			if (!PickBladePoint(in ctx, ref limb, out (Vector2 Local, Vector2 Global) bladePoint)) { goto Reset; }

			limb.LiftInAnimation = false;
			limb.TileAttachment = null;
			limb.BladeAttachment = bladePoint.Local;
			limb.Animation.Progress = MathUtils.StepTowards(limb.Animation.Progress, 1f, 3f * TimeSystem.LogicDeltaTime);
			limb.TargetPosition = bladePoint.Global;
			continue;

			Reset:;
			if (wasWielding)
			{
				limb.ResetState();
			}
		}
	}
	private void LimbGrabbing(in Context ctx)
	{
		_ = ctx;

		if (grabCooldown > 0)
		{
			grabCooldown--;
		}

		(bool ForPlayers, bool ForMinions) CanTriggerGrab(in Context ctx)
		{
			// Not during cutscenes.
			if (Phase is PhaseType.Idle || CutsceneActive) { return (false, false); }

			// Not if on cooldown.
			if (grabCooldown > 0) { return (false, false); }

			// Not during teleports.
			if (ctx.Teleports.Active) { return (false, false); }

			if (Phase is PhaseType.Abomination)
			{
				return (ForPlayers: false, ForMinions: true);
			}

			// Grabs can be triggered slightly before the attack is considered over.
			// Minions can be grabbed even while attacking.
			const int postDmgWindow = 15;
			if (ctx.Attacking.Active && ctx.Attacking.Data.Progress <= ctx.Attacking.Data.Damage.End + postDmgWindow)
			{
				return (ForPlayers: false, ForMinions: true);
			}

			return (true, true);
		}

		(bool canGrabPlayers, bool canGrabMinions) = CanTriggerGrab(in ctx);
		bool canGrabAnyone = canGrabPlayers | canGrabMinions;
		Vector2 logicalBodyCenter = ctx.Center;

#if FRIENDLY
		canGrabAnyone = false;
#endif

		int limbIndex = -1;
		foreach (ref Limb limb in limbs.AsSpan())
		{
			limbIndex++;
			if (!limb.Role.HasFlag(LimbRole.EntityGrabbing)) { continue; }

			const float postInitiationRangeBonus = 16;
			float npcGrabRange = limb.IK.Length * 1.05f;
			float playerGrabRange = limb.IK.Length * (Phase is PhaseType.Abomination ? 1.00f : 0.85f);
			float sqrNpcGrabRange = npcGrabRange * npcGrabRange;
			float sqrPlayerGrabRange = playerGrabRange * playerGrabRange;
			Vector2 logicalLimbPos = limb.IK.GetPosition(logicalBodyCenter, rotation: bodyAngle.Current, xDir: NPC.spriteDirection);
			bool limbAvailable = !limb.IsGibbedAtAll || Phase == PhaseType.Abomination;

			// If not already holding something - try grabbing something.
			if (limb.EntityAttachment.Entity is not { } grabEntity)
			{
				// Unless we cannot!
				if (!canGrabAnyone || !limbAvailable)
				{
					continue;
				}

				// Find closest entity to grab.

				if (canGrabPlayers)
				{
					foreach (Player player in Main.ActivePlayers)
					{
						// continue;
						if (player.DistanceSQ(logicalLimbPos) > sqrPlayerGrabRange) { continue; }
						if (limbs.Any(l => l.EntityAttachment.Player == player)) { continue; }

						limb.ResetState();
						limb.EntityAttachment = (EntityRef)player;

						break;
					}
				}
#if true
				if (canGrabMinions)
				{
					foreach (NPC npc in Main.ActiveNPCs)
					{
						if (npc == NPC) { continue; }
						if (npc.immortal || NPCID.Sets.ImmuneToAllBuffs[npc.type]) { continue; }
						if (npc.DistanceSQ(logicalLimbPos) > sqrNpcGrabRange) { continue; }
						if (limbs.Any(l => l.EntityAttachment.NPC == npc)) { continue; }

						// In phase II, only grab shamans.
						if (Phase is PhaseType.Abomination && npc.type != ModContent.NPCType<FallenShaman>()) { continue; }

						// Don't grab teleporting enemies.
						if (npc.TryGetGlobalNPC(out NPCTeleports teleports) && teleports.Enabled && teleports.Active) { continue; }

						limb.ResetState();
						limb.EntityAttachment = (EntityRef)npc;

						break;
					}
				}
#endif

				if (!limb.EntityAttachment.IsValid) { continue; }

				grabEntity = limb.EntityAttachment.Entity!;
				limb.TargetPosition = grabEntity.Center;
				grabCooldown = (ushort)Math.Max(grabCooldown, Phase is PhaseType.Abomination ? 120 : 120);

				if (!Main.dedServ)
				{
					SoundEngine.PlaySound(position: ctx.Center, style: new($"{nameof(PathOfTerraria)}/Assets/Sounds/Conflux/PyralisGrab", 2)
					{
						MaxInstances = 3,
						PitchVariance = 0.125f,
						Volume = 1.0f,
					});
				}

				// Play a special cue when grabbing the local player.
				if (!Main.dedServ && grabEntity == Main.LocalPlayer)
				{
					SoundEngine.PlaySound(position: ctx.Center, style: new($"{nameof(PathOfTerraria)}/Assets/Sounds/Conflux/PyralisGrabCue", 3)
					{
						MaxInstances = 3,
						Pitch = Phase is PhaseType.Abomination ? -0.7f : +0,
						PitchVariance = 0.05f,
						// Volume = 0.6f,
						Volume = Phase is PhaseType.Abomination ? 0.0f : 0.6f,
					}, updateCallback: new NPCTracker(NPC).AudioCallback);
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

			// If the grabbed entity is not the target - throw at the target.
			// Else throw somewhere that our velocity dictates.
			bool isHoldingTarget = NPC.GetTargetEntity() != grabEntity;
			Vector2 throwBase = isHoldingTarget ? (ctx.TargetCenter - grabEntity.Center) : (NPC.velocity + new Vector2(0, -3));
			Vector2 throwDirection = throwBase.SafeNormalize(Vector2.UnitX * NPC.direction);

			// Stage 0 - Grab.
			if (limb.Animation.Stage == 0)
			{
				float speed = Phase is PhaseType.Abomination ? 4f : 2f;
				limb.TargetPosition = grabEntity.Center;
				limb.Animation.Progress = MathUtils.StepTowards(limb.Animation.Progress, 1f, speed * TimeSystem.LogicDeltaTime);
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

				float progressSpeed = 1.5f;
				// In phase II, keep shamans in hand for quite some time, in order to use them as turrets.
				if (Phase is PhaseType.Abomination && grabEntity is NPC n && n.type == ModContent.NPCType<FallenShaman>())
				{
					limb.TargetPosition += new Vector2(((limbIndex % 3) - 1) * 32, -144);
					if (limb.Animation.Progress > 0.90f)
					{
						progressSpeed = 0.005f;
					}
				}

				limb.Animation.Progress = MathUtils.StepTowards(limb.Animation.Progress, 1f, progressSpeed * TimeSystem.LogicDeltaTime);
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
				if (!TileUtils.TryFitRectangleIntoTilemap(basePoint, sizeInTiles, out Vector2Int adjustedPoint))
				{
					limb.TargetPosition = ctx.Center;
				}
				else if ((Point)(adjustedPoint + new Vector2Int(1, 1)) != basePoint)
				{
					limb.TargetPosition = ((Point)adjustedPoint).ToWorldCoordinates(0, 0) + (size * 0.5f);
				}
			}

			// Release.
			if (limb.Animation is { Stage: 2, Progress: >= 0.25f } or { Stage: >= 3 })
			{
				limb.ResetState();
				grabEntity.velocity = throwDirection * 30f;

				grabCooldown = Math.Max(grabCooldown, (ushort)120);

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
				grabEntity.Center = limb.TargetPosition + new Vector2(grabEntity.width * 0.5f * NPC.direction, -grabEntity.height * 0.25f);

				// Prevent held NPCs from teleporting.
				if (grabEntity is NPC npc && npc.TryGetGlobalNPC(out NPCTeleports ntp) && ntp.Enabled)
				{
					ntp.Data.Cooldown.SetIfGreater(60);
				}
			}
		}
	}
	private void LimbFlailing(in Context ctx)
	{
		_ = ctx;
		Vector2 logicalBodyCenter = NPC.Center;
		Vector2 visualBodyCenter = logicalBodyCenter + new Vector2(0f, NPC.gfxOffY);

		Vector2 perceivedVelocity = NPC.position - NPC.oldPosition;
		bool teleportAnim = ctx.Teleports.Active && ctx.Teleports.Data.Progress < ctx.Teleports.Data.Reappear.Start - 10;

		foreach (ref Limb limb in limbs.AsSpan())
		{
			if (!limb.IsTotallyGone)
			{
				if (limb.TileAttachment != null && !teleportAnim) { continue; }
				if (limb.BladeAttachment != null) { continue; }
				if (limb.EntityAttachment.IsValid) { continue; }
			}

			limb.TileAttachment = null;

			Vector2 visualLimbPos = limb.IK.GetPosition(visualBodyCenter, rotation: bodyAngle.Current, xDir: NPC.spriteDirection);
			Vector2 flailOffset = new Vector2(NPC.direction * 16, 32) + (perceivedVelocity.SafeNormalize(Vector2.UnitY) * 120);

			if (Phase is PhaseType.Abomination && limb.Role.HasFlag(LimbRole.Wielding))
			{
				flailOffset = new Vector2(
					MathF.Sin(movementAnimation / 100) * 24,
					-128 + MathF.Sin(movementAnimation / 100) * 8
				);
			}

			var flailTarget = Vector2.Lerp(visualLimbPos + flailOffset, NPC.GetTargetData().Center, 0.1f);
			limb.Animation.Progress = MathUtils.StepTowards(limb.Animation.Progress, 1f, 0.8f * TimeSystem.LogicDeltaTime);
			limb.TargetPosition = flailTarget;
		}
	}

	private void UpdateLimbs(in Context ctx)
	{
#if DEBUG && DEBUG_KEYS
		if (Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.N)) { SetupLimbs(in ctx); }
#endif

		Vector2 logicalBodyCenter = NPC.Center;
		Vector2 visualBodyCenter = logicalBodyCenter + new Vector2(0f, NPC.gfxOffY);

		int limbIndex = -1;
		foreach (ref Limb limb in limbs.AsSpan())
		{
			limbIndex++;
			ref LimbGroup group = ref limbGroups[limb.MovementGroup];
			Vector2 visualLimbPos = limb.IK.GetPosition(visualBodyCenter, rotation: bodyAngle.Current, xDir: NPC.spriteDirection);

#if DEBUG && IK_PREVIEW
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
			Vector2 usedStart = limb.StartPosition;
			Vector2 usedTarget = limb.TargetPosition;

			// Lift up for moving between tiles.
			float liftAnim = limb.LiftInAnimation ? MathUtils.Clamp01(anim * 8) : 0f;
			float liftEffect = LiftEasing(MathUtils.Clamp01(liftAnim <= 0.5f ? (liftAnim * 2f) : ((1f - liftAnim) * 2f)));
			var liftPos = new Vector2(limb.TargetPosition.X, visualLimbPos.Y);
			usedTarget = Vector2.Lerp(usedTarget, visualLimbPos, liftEffect * 0.2f);

			limb.IK.Target = Vector2.Lerp(usedStart, usedTarget, MoveEasing(anim));

			static float MoveEasing(float x)
			{
				return x >= 1f ? 1 : (1 - MathF.Pow(2, -10 * x));
			}
			static float LiftEasing(float x)
			{
				return 1f - MathF.Pow(1f - x, 3f);
			}

#if DEBUG && DEBUG_GIZMOS
			var sb = new System.Text.StringBuilder();
			sb.AppendLine($"L{limbIndex}");
			sb.AppendLine($"{(limb.TileAttachment != null ? "T" : null)}");
			sb.AppendLine($"{(limb.EntityAttachment.Entity != null ? "E" : null)}");
			DebugUtils.DrawStringInWorld(sb.ToString(), usedTarget, Color.White);
#endif
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
			return !limb.IsTotallyGone && limb.CanBeGibbed && (int)Phase >= (int)limb.MinPhaseToGib && limb.IK.Segments[0].Frame.RowCount > 1;
		}

		int divisions = 40 * (Phase == PhaseType.Abomination ? 1 : 1);
		bool gibFromDamage = lifeNew < lifeOld && (int)MathF.Ceiling(factorNew * divisions) < (int)MathF.Ceiling(factorOld * divisions);
		bool gibDuringDeath = Cutscene.Type == CutsceneType.Death && (Cutscene.Counter % 15 == 0 || Cutscene.Counter % 21 == 0);
		float volumeFactor = gibDuringDeath ? 0.5f : 1.0f;

		if ((gibFromDamage || gibDuringDeath) && Enumerable.Range(0, limbs.Length).Where(CanGibLimb).ToArray() is { Length: > 0 } gibbables)
		{
			int gibIndex = gibbables[Main.rand.Next(gibbables.Length)];
			ref Limb limb = ref limbs[gibIndex];
			Vector2 logicalBodyCenter = ctx.Center;
			Vector2 limbPos = limb.IK.GetPosition(logicalBodyCenter, rotation: bodyAngle.Current, xDir: NPC.spriteDirection);

			limb.ResetState();
			limb.SetGibbed(limb.GibLevel + 1);

			if (!Main.dedServ)
			{
				if (Main.rand.NextBool(3))
				{
					SoundEngine.PlaySound(position: ctx.Center, style: new($"{nameof(PathOfTerraria)}/Assets/Sounds/Gore/SuperSplatter1")
					{
						Volume = 0.62f * volumeFactor,
						MaxInstances = 5,
						PitchVariance = 0.5f,
					});
				}
				else
				{
					SoundEngine.PlaySound(position: ctx.Center, style: new($"{nameof(PathOfTerraria)}/Assets/Sounds/Gore/MediumSplatter", 2)
					{
						Volume = 0.85f * volumeFactor,
						MaxInstances = 5,
						Pitch = -0.2f,
						PitchVariance = 0.4f,
					});
				}

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
		Vector2 movDir = wishedMovement.Direction != default ? wishedMovement.Direction : NPC.velocity.SafeNormalize(Vector2.UnitY);
		movDir = ((movDir * new Vector2(1.0f, 1.0f)) + new Vector2(0.0f, 0.2f)).SafeNormalize(Vector2.UnitY);
		//Vector2 movDir = (NPC.velocity * new Vector2(1.5f, 1.0f) + new Vector2(0f, 3.5f)).SafeNormalize(Vector2.UnitY);

		Vector2 startPos = limb.IK.GetPosition(NPC.Center, rotation: bodyAngle.Current, xDir: NPC.spriteDirection);
		Vector2Int startTile = startPos.ToTileCoordinates();
		float placeRange = limb.IK.Length + 16;
		int tileRange = (int)MathF.Floor(placeRange / TileUtils.TileSizeInPixels);

#if DEBUG && DEBUG_GIZMOS
		DebugUtils.DrawStringInWorld("C", ctx.Center, Color.White);
		DebugUtils.DrawStringInWorld("D", ctx.Center + movDir * 64, Color.White);
#endif

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

					// Penalize using a surrounded tile.
					Tile uTile = Main.tile[x + 0, y - 1];
					Tile dTile = Main.tile[x + 0, y + 1];
					Tile lTile = Main.tile[x - 1, y + 0];
					Tile rTile = Main.tile[x + 1, y + 0];
					bool uFull = (WorldUtilities.SolidTile(uTile) | uTile.LiquidAmount != 0);
					bool dFull = (WorldUtilities.SolidTile(dTile) | dTile.LiquidAmount != 0);
					bool lFull = (WorldUtilities.SolidTile(lTile) | lTile.LiquidAmount != 0);
					bool rFull = (WorldUtilities.SolidTile(rTile) | rTile.LiquidAmount != 0);
					bool tileSurrounded = uFull && dFull && lFull && rFull;
					float surroundPenalty = tileSurrounded ? 0.01f : 1f;
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
#if DEBUG && DEBUG_GIZMOS
			DebugUtils.DrawStringInWorld("X", bestTile.ToWorldCoordinates(), Color.Green);
#endif
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

		if (CurrentlySitting)
		{
			Blade.Position = ctx.Center + new Vector2(-150, -202);
			Blade.Rotation = MathHelper.ToRadians(108.5f);
			return;
		}

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

		if (ctx.Attacking.Active && attackType is AttackType.BladeSlash or AttackType.BladeReverse or AttackType.BladeStab)
		{
			// Attack animations.
			Gradient<BladeAnimKey> animation = attackType switch
			{
				AttackType.BladeStab => stabAnimation,
				AttackType.BladeReverse => reverseAnimation,
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
		}
		else
		{
			// Idle animations.
			targetPosition = ctx.Center + new Vector2(0, -64);
			posChange = 0.08f;

			targetRotation = (MathHelper.PiOver2 + (MathHelper.TwoPi * 0.13f * NPC.spriteDirection));
			rotChange = 0.09f;
			maxDistance = 180;
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
	
	private static readonly Vector2[] bladeGrabOffsets =
	[
		new(+0, +0),
		new(-48, +0),
		new(+48, +0),
	];
	/// <summary> Finds the closest free and reachable blade grab point. Object-relative. </summary>
	private bool PickBladePoint(in Context ctx, ref Limb limb, [MaybeNullWhen(false)] out (Vector2 Local, Vector2 Global) result)
	{
		_ = ctx;
		Vector2 limbOrigin = limb.IK.GetPosition(NPC.Center, rotation: bodyAngle.Current, xDir: NPC.spriteDirection);
		float reachRange = limb.IK.Length * 0.97f;
		float reachRangeSqr = reachRange * reachRange;

		(float SqrDistance, Vector2 Local, Vector2 Global) bestResult = (float.PositiveInfinity, default, default);

		(Vector2 hiltPos, float hiltAngle) = (Blade.Position, Blade.Rotation);

		if (Phase is PhaseType.Abomination)
		{
			hiltPos = ctx.Center + new Vector2(+0, -128);
			hiltAngle = 0f;
		}

		foreach (Vector2 local in bladeGrabOffsets)
		{
			Vector2 global = hiltPos + (local.RotatedBy(hiltAngle));
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
		if (!NPC.active) { return; }

		Context ctx = new(NPC);
		ctx.Animations.Advance();
		ctx.Animations.Set(PickAnimation(in ctx));
	}

	public override bool PreDraw(SpriteBatch sb, Vector2 screenPos, Color drawColor)
	{
		Context ctx = new(NPC);

		if (CurrentlySitting || NPC.IsABestiaryIconDummy) // && limbs.Length == 0)
		{
			// SetupLimbs(in ctx);
			RenderBlade(sb, screenPos);
			return true;
		}

		helmetTexture ??= ModContent.Request<Texture2D>($"{Texture}_Helmet", AssetRequestMode.ImmediateLoad);
		bodyTexture ??= ModContent.Request<Texture2D>($"{Texture}_Body", AssetRequestMode.ImmediateLoad);
		robeTexture ??= ModContent.Request<Texture2D>($"{Texture}_Robe", AssetRequestMode.ImmediateLoad);

		SpriteBatchArgs sbArgs = sb.GetArguments();
		// sb.End();
		// sb.Begin(sbArgs with { Effect = LitShaders.LitSprite(sbArgs.Matrix) });

		// Draw the back arms.
		RenderArms(sb, layerSign: -1, screenPos, drawColor);

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
		RenderArms(sb, layerSign: +1, screenPos, drawColor);

		// sb.End();
		// sb.Begin(sbArgs);

		return false;
	}

	private void RenderBlade(SpriteBatch sb, Vector2 screenPos)
	{
#if HIDE_SWORD
		return;
#endif

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
				RenderUtils.DrawUserQuad(gfx, vertices.Array, vertices.Length);
			}
		}
	}

	private void RenderArms(SpriteBatch sb, int? layerSign, Vector2 screenPos, Color drawColor)
	{
		foreach (int limbIndex in Enumerable.Range(0, limbs.Length).OrderBy(i => limbs[i].Layer))
		{
			ref Limb limb = ref limbs[limbIndex];
			if (layerSign != null && (limb.Layer >= 0f ? +1 : -1) != layerSign.Value) { continue; }

			limb.TextureCache ??= ModContent.Request<Texture2D>(limb.TexturePath);

			if (limb.TextureCache is not { IsLoaded: true, Value: { } limbTexture }) { continue; }

			int numDrawnSegments = Math.Clamp(limb.IK.Segments.Length - limb.GibLevel + 1, 0, limb.IK.Segments.Length);
			for (int j = 0; j < numDrawnSegments; j++)
			{
				Debug.Assert(limb.IK.Segments[j].Frame.CurrentRow != byte.MaxValue);

				DrawData drawData = limb.IK.GetDrawParams(limbTexture, j, screenPos, NPC.spriteDirection);
				drawData.color = drawData.color.MultiplyRGBA(drawColor);
				drawData.Draw(sb);
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


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

using System.IO;
using System.Linq;
using MonoMod.Cil;
using NPCUtils;
using PathOfTerraria.Common.AI;
using PathOfTerraria.Common.NPCs;
using PathOfTerraria.Common.NPCs.Components;
using PathOfTerraria.Common.NPCs.Effects;
using PathOfTerraria.Common.Utilities;
using PathOfTerraria.Common.Utilities.Extensions;
using PathOfTerraria.Common.World.Utilities;
using PathOfTerraria.Content.Gores;
using PathOfTerraria.Content.Items.Gear.Armor.Chestplate;
using PathOfTerraria.Content.Items.Gear.Armor.Helmet;
using PathOfTerraria.Content.Items.Gear.Armor.Leggings;
using PathOfTerraria.Content.Items.Gear.Rings;
using PathOfTerraria.Content.Items.Gear.Weapons.Sword;
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
using Terraria.Localization;

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
		private const int SaturationDuration = 180;
		private const int SaturationCooldown = 360;
		private const int SegmentsPerBand = 4;

		private Vector2? lastSegmentVelocity;
		private int ridingBoss = -1;
		private int ridingBand = -1;
		private int saturation;
		private int cooldown;
		private bool touchedSegment;

		public override void PreUpdateMovement()
		{
			touchedSegment = false;
			ApplyEntityCollision<Player>(Player, ref lastSegmentVelocity);
			UpdateRimeSaturation();
		}

		internal void RecordRidingSegment(GlacialBoss boss, int segmentIndex)
		{
			int band = segmentIndex / SegmentsPerBand;
			touchedSegment = true;

			if (ridingBoss != boss.NPC.whoAmI || ridingBand != band)
			{
				ridingBoss = boss.NPC.whoAmI;
				ridingBand = band;
				saturation = Math.Max(0, saturation - 45);
			}
		}

		private void UpdateRimeSaturation()
		{
			if (cooldown > 0) { cooldown--; }

			if (!touchedSegment || ridingBoss < 0 || ridingBoss >= Main.maxNPCs
				|| Main.npc[ridingBoss] is not { active: true, ModNPC: GlacialBoss boss }
				|| boss.Phase == GlacialBoss.PhaseType.Idle || boss.CutsceneActive)
			{
				saturation = Math.Max(0, saturation - 3);
				if (saturation == 0)
				{
					ridingBoss = -1;
					ridingBand = -1;
				}
				return;
			}

			if (cooldown > 0) { return; }

			saturation = Math.Min(SaturationDuration, saturation + 1);
			if (!Main.dedServ && Player.whoAmI == Main.myPlayer)
			{
				boss.EmitLocalRimeWarning(ridingBand, saturation / (float)SaturationDuration);
				if (saturation == 90)
				{
					CombatText.NewText(Player.Hitbox, Color.LightCyan,
						Language.GetTextValue("Mods.PathOfTerraria.Misc.Yryoth.RimeGathering"));
					SoundEngine.PlaySound(SoundID.Item30 with { Volume = 0.5f, Pitch = -0.4f }, Player.Center);
				}
				else if (saturation == 144)
				{
					CombatText.NewText(Player.Hitbox, Color.Cyan,
						Language.GetTextValue("Mods.PathOfTerraria.Misc.Yryoth.RimeEruption"), dramatic: true);
					SoundEngine.PlaySound(SoundID.Item30 with { Volume = 0.8f, Pitch = 0.1f }, Player.Center);
				}
			}

			if (saturation < SaturationDuration) { return; }

			if (Main.netMode != NetmodeID.MultiplayerClient && !boss.TryArmRimeEruption(Player, ridingBand))
			{
				saturation = SaturationDuration - 1;
				return;
			}
			saturation = 0;
			cooldown = SaturationCooldown;
		}
	}
	
	public static void ApplyEntityCollision<T>(Entity entity, ref Vector2? lastSegmentVelocity) where T : Entity
	{
		bool ridingSegment = false;
		Player player = typeof(T) == typeof(Player) ? (Player)entity : null!;
	
		if (entity.velocity.Y < 0 || (typeof(T) == typeof(Player) && player.controlDown))
		{
			goto SkipCollision;
		}

		const float DepthCutoff = -0.05f;

		// Allow players to walk on this boss.
		foreach (NPC npc in Main.ActiveNPCs)
		{
			if (npc.ModNPC is not GlacialBoss boss) { continue; }
		
			const float lowFactor = 0.45f;
			Rectangle npcRect = npc.getRect();
			Rectangle entityRect = new((int)entity.position.X, (int)entity.position.Y, entity.width, entity.height);
			Rectangle lowRect = entityRect with
			{
				Height = (int)(entity.height * lowFactor),
				Y = (int)(entity.position.Y + entity.height * (1f - lowFactor)),
			};

			if (!lowRect.Intersects(npcRect)) { continue; }

			const int numInterSteps = 4;
			// foreach (ref readonly GlacialBoss.Segment segment in boss.segments.AsSpan())
			// foreach (GlacialBoss.Segment segment in boss.segments.OrderByDescending(s => s.Position.Y))
			for (int pointIdx = 0; pointIdx < boss.segments.Length * numInterSteps; pointIdx++)
			{
				int segmentIdx = pointIdx / numInterSteps;
				GlacialBoss.Segment segment = boss.segments[segmentIdx];
				GlacialBoss.Segment nextSegment = segmentIdx < boss.segments.Length - 1 ? boss.segments[segmentIdx + 1] : segment;
				float interStep = (pointIdx % numInterSteps) / (float)numInterSteps;

				if (segmentIdx == 0) { interStep = 0; }

				Rectangle aabbA = segment.GetSurfaceAabb();
				Rectangle aabbB = nextSegment.GetSurfaceAabb();
				Rectangle segAabb = aabbA;
				segAabb.X = (int)MathHelper.Lerp(aabbA.X, aabbB.X, interStep);
				segAabb.Y = (int)MathHelper.Lerp(aabbA.Y, aabbB.Y, interStep);
				float depth = MathHelper.Lerp(segment.Depth, nextSegment.Depth, interStep);
				float contactDamageFactor = MathHelper.Lerp(segment.ContactDamageFactor, nextSegment.ContactDamageFactor, interStep);
				
#if DEBUG && DEBUG_GIZMOS
				if (entity == Main.LocalPlayer)
				{
					DebugUtils.DrawRectInWorld(segAabb, Color.Red);
				}
#endif
				if (depth < DepthCutoff) { continue; }

				if (contactDamageFactor > 0f && depth > 0.15f)
				{
					Rectangle damageAabb = Rectangle.Union(segment.GetHitbox(), nextSegment.GetHitbox());
					if (entityRect.Intersects(damageAabb))
					{
						if (typeof(T) == typeof(Player)
							&& player.whoAmI == Main.myPlayer
							&& player.hurtCooldowns[ImmunityCooldownID.TileContactDamage] <= 0)
						{
							int damage = (int)(npc.damage * boss.GetSegmentContactDamageFactor(contactDamageFactor));
							player.Hurt(PlayerDeathReason.ByNPC(npc.whoAmI), damage, Math.Sign(player.Center.X - boss.Center.X),
								cooldownCounter: ImmunityCooldownID.TileContactDamage);
						}
						continue;
					}
				}

				
				if (!lowRect.Intersects(segAabb)) { continue; }

				int standY = (segAabb.Top - entity.height) + 16;
				Vector2 oldPos = entity.position;
				Vector2 newPos = (oldPos with { Y = Math.Min(entity.position.Y, standY) }) + segment.Velocity;
				float yDelta = newPos.Y - oldPos.Y;

				if (yDelta < -30) { continue; }
			
				if (!Collision.SolidCollision(newPos, entity.width, entity.height))
				{
					entity.position = newPos;
					entity.velocity.Y = 0;
					
					lastSegmentVelocity = segment.Velocity;
					ridingSegment = true;

					if (typeof(T) == typeof(Player))
					{
						player.gfxOffY = Math.Max(player.gfxOffY, oldPos.Y - newPos.Y);
						player.GetModPlayer<GlacialBossPlayer>().RecordRidingSegment(boss, segmentIdx);
						goto SkipCollision;
					}
				}
			}
		}

		SkipCollision:;
		if (!ridingSegment && lastSegmentVelocity is Vector2 vel && vel.Floor() != default)
		{
			const float maxSpeed = 10;
			if (vel.Length() > maxSpeed)
			{
				vel = Vector2.Normalize(vel) * maxSpeed;
			}

			var direction = Vector2.Normalize(vel);
			Vector2 oldVel = entity.velocity;
			entity.velocity = MovementUtils.DirAccel(entity.velocity, direction, vel.Length(), vel.Length());
			entity.velocity.Y = MathF.Min(entity.velocity.Y, oldVel.Y);
		
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
		public float ContactDamageFactor;
		public bool WasSubmerged;

		public readonly Rectangle GetSurfaceAabb()
		{
			float full = Length * 1.0f;
			float half = full * 0.5f;
			float vert = full * 0.5f;
			return new Rectangle((int)(Position.X - half), (int)(Position.Y - half), (int)full, (int)vert);
		}

		public readonly Rectangle GetHitbox(float scale = 1f)
		{
			float size = Length * scale;
			float half = size * 0.5f;
			return new Rectangle((int)(Position.X - half), (int)(Position.Y - half), (int)size, (int)size);
		}
	}

	internal enum AttackType : byte
	{
		None,
		Bite,
		FrostRing,
		BreachSweep,
		RimeWall,
	}
	private readonly struct Context(NPC npc)
	{
		public Vector2 Center { get; } = npc.Center;
		public Vector2 TargetCenter { get; } = npc.GetGlobalNPC<NPCTargeting>().GetTargetCenter(npc);
		public NPCMovement Movement { get; } = npc.GetGlobalNPC<NPCMovement>();
		public NPCTargeting Targeting { get; } = npc.GetGlobalNPC<NPCTargeting>();
		public NPCAnimations Animations { get; } = npc.GetGlobalNPC<NPCAnimations>();
		public NPCFootsteps Footsteps { get; } = npc.GetGlobalNPC<NPCFootsteps>();
		public NPCVoice Voice { get; } = npc.GetGlobalNPC<NPCVoice>();
	}

	public const float SecondPhaseHealthFactor = 0.6f;

	private static readonly SpriteAnimation animIdle = new() { Id = "idle", Frames = [0], Speed = 3f };
	private static readonly int phase1Music = MusicID.OtherworldlyBoss2;
	private static readonly int phase2Music = MusicID.OtherworldlyLunarBoss;
	private static int[] minionTypes = [];
	private static Asset<Texture2D>? hornsTexture;
	private const int SegmentsPerRimeBand = 4;
	private const ushort RimeEruptionDelay = 45;
	private const ushort RimeEruptionLifetime = 65;

	// Synchronized state:
	public PhaseType Phase = PhaseType.Idle;
	private CutsceneData Cutscene;
	private uint globalCounter;
	private ushort spawnCooldown;
	private ushort attackTimer;
	private ushort attackDuration;
	private ushort attackCooldown;
	private byte attackSequence;
	private byte attackTarget = byte.MaxValue;
	private AttackType attackType;
	private byte rimeEruptionBand = byte.MaxValue;
	private ushort rimeEruptionTimer;
	private byte rimeEruptionTarget = byte.MaxValue;
	private Vector2 headPosition;
	private Vector2 headVelocity;
	private Vector2 spawnPosition;
	private Vector2 networkHeadPosition;
	private Vector2 networkHeadVelocity;
	private bool receivedHeadState;
	private Vector2 attackAim;
	private Vector2 attackHeading;
	internal Segment[] segments = [];

	// Non-synchronized state:
	private (SlotId Handle, float Intensity) movementSound;
	private bool rimeWarningSoundPlayed;
	private bool rimeBurstSoundPlayed;

	public bool CutsceneActive => Cutscene.Type != 0;
	public Vector2 Center => segments.Length > 0 ? segments[0].Position : NPC.Center;
	internal AttackType CurrentAttack => attackType;
	internal ushort AttackTimer => attackTimer;
	internal ushort AttackDuration => attackDuration;

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
		NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, new() { CustomTexturePath = $"{Texture}_Bestiary" });
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
		NPC.noGravity = true;
		NPC.noTileCollide = true;
		NPC.netAlways = true;

		NPC.HitSound = new($"{nameof(PathOfTerraria)}/Assets/Sounds/HitEffects/BoneHit", 3) { Volume = 0.9f, Pitch = -0.35f, PitchVariance = 0.2f, MaxInstances = 5 };
		NPC.DeathSound = SoundID.NPCDeath51 with { Volume = 1f, Pitch = -0.6f, PitchVariance = 0.1f };

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
		Context ctx = new(NPC);
		SetupSegments(in ctx);
		headPosition = segments[0].Position;

		// Initial cooldowns.
		spawnCooldown = 60 * 3;
		attackCooldown = 60 * 2;
	}

	public override void OnSpawn(IEntitySource source)
	{
		Context ctx = new(NPC);
		SetupSegments(in ctx);
		headPosition = NPC.Center;
		spawnPosition = headPosition;
		headVelocity = Vector2.Zero;
		networkHeadPosition = headPosition;
		networkHeadVelocity = Vector2.Zero;
		receivedHeadState = Main.netMode != NetmodeID.MultiplayerClient;
	}

	public override void ModifyNPCLoot(NPCLoot npcLoot)
	{
		int chanceCommon = (int)MathF.Ceiling(100f / 15f);
		int chanceUncommon = (int)MathF.Ceiling(100f / 5f);
		int chanceRare = (int)MathF.Ceiling(100f / 2f);

		npcLoot.AddCommon<YryothsCrown>(chanceCommon);
		npcLoot.AddCommon<BetrayersCarapace>(chanceCommon);
		npcLoot.AddCommon<RimeboundTreads>(chanceUncommon);
		npcLoot.AddCommon<FrozenOrbit>(chanceRare);
		npcLoot.AddCommon<Cryobrand>(chanceRare);
	}

	public override void SendExtraAI(BinaryWriter writer)
	{
		writer.Write((byte)Phase);
		writer.Write(globalCounter);
		writer.Write(spawnCooldown);
		writer.Write((byte)attackType);
		writer.Write(attackTimer);
		writer.Write(attackDuration);
		writer.Write(attackCooldown);
		writer.Write(attackSequence);
		writer.Write(attackTarget);
		writer.Write(rimeEruptionBand);
		writer.Write(rimeEruptionTimer);
		writer.Write(rimeEruptionTarget);
		writer.Write(headPosition.X);
		writer.Write(headPosition.Y);
		writer.Write(headVelocity.X);
		writer.Write(headVelocity.Y);
		writer.Write(attackAim.X);
		writer.Write(attackAim.Y);
		writer.Write(attackHeading.X);
		writer.Write(attackHeading.Y);
		writer.Write((byte)Cutscene.Type);
		writer.Write(Cutscene.Counter);
		writer.Write(Cutscene.Length);
	}
	public override void ReceiveExtraAI(BinaryReader reader)
	{
		Phase = (PhaseType)reader.ReadByte();
		globalCounter = reader.ReadUInt32();
		spawnCooldown = reader.ReadUInt16();
		attackType = (AttackType)reader.ReadByte();
		attackTimer = reader.ReadUInt16();
		attackDuration = reader.ReadUInt16();
		attackCooldown = reader.ReadUInt16();
		attackSequence = reader.ReadByte();
		attackTarget = reader.ReadByte();
		rimeEruptionBand = reader.ReadByte();
		rimeEruptionTimer = reader.ReadUInt16();
		rimeEruptionTarget = reader.ReadByte();
		networkHeadPosition = new(reader.ReadSingle(), reader.ReadSingle());
		networkHeadVelocity = new(reader.ReadSingle(), reader.ReadSingle());
		if (!receivedHeadState)
		{
			headPosition = networkHeadPosition;
			headVelocity = networkHeadVelocity;
			receivedHeadState = true;
		}
		attackAim = new(reader.ReadSingle(), reader.ReadSingle());
		attackHeading = new(reader.ReadSingle(), reader.ReadSingle());

		var type = (CutsceneType)reader.ReadByte();
		ushort counter = reader.ReadUInt16();
		ushort length = reader.ReadUInt16();
		bool initialized = type == Cutscene.Type && Cutscene.Initialized;
		Cutscene = new(type, counter, length) { Initialized = initialized };
	}

	public override void AI()
	{
		Context ctx = new(NPC);

#if IK_PREVIEW
		NPC.Center = Main.MouseWorld;
#endif

		NPC.scale = 1.0f; //1.5f;

		// Reset overrides.
		ctx.Movement.Data.TargetOverride = null;

		globalCounter++;
		if (Main.netMode != NetmodeID.MultiplayerClient && globalCounter % 30 == 0)
		{
			NPC.netUpdate = true;
		}

		// Invoke behaviors.
		ctx.Targeting.ManualUpdate(new(NPC));
		PhaseLogic(in ctx);
		CutsceneLogic(in ctx);

		// Short-circuit if the death cutscene just ended.
		if (!NPC.active) { return; }

		IdentityLogic(in ctx);
		ResetOffsets(in ctx);
		SpawnMinions(in ctx);
		UpdateAttacks(in ctx);
		UpdateSegments(in ctx);
		UpdateRimeEruption();
		UpdateCollision(in ctx);

		UpdateEffects(in ctx);
		ResetOffsets(in ctx);

#if IK_PREVIEW
		NPC.rotation = 0f;
		NPC.velocity = default;
#endif
	}

	private void PhaseLogic(in Context ctx)
	{
		if (Main.netMode != NetmodeID.MultiplayerClient && Phase == PhaseType.Idle && !CutsceneActive)
		{
			const float introRange = 512;
			foreach (Player player in Main.ActivePlayers)
			{
				if (player.WithinRange(Center, introRange))
				{
					Phase = PhaseType.First;
					Cutscene = new(CutsceneType.Intro, 0, 250);
					NPC.netUpdate = true;
					break;
				}
			}
		}

		// Switch to second phase.
		if (Main.netMode != NetmodeID.MultiplayerClient && !CutsceneActive && Phase == PhaseType.First
#if !FORCE_PHASE_II
		&& NPC.life <= NPC.lifeMax * SecondPhaseHealthFactor
#endif
		)
		{
			Phase = PhaseType.Second;
			Cutscene = new(CutsceneType.Transformation, 0, 300);
			NPC.life = (int)(NPC.lifeMax * SecondPhaseHealthFactor);
			ResetAttack(90);
			NPC.netUpdate = true;
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

				if (!Main.dedServ) CameraCurios.Create(new()
				{
					Identifier = $"{nameof(GlacialBoss)}_Intro",
					Weight = 0.5f,
					LengthInSeconds = lengthInSeconds,
					FadeOutLength = 0.2f,
					Position = ctx.Center + new Vector2(0, 128),
					Range = new(Min: 1024, Max: 3072, Exponent: 1.5f),
					Zoom = +0.5f,
				});
				if (!Main.dedServ) OverlayText.Create(new OverlayTextLine
				{
					AnimationLength = lengthInSeconds,
					Position = (new Vector2(0.5f, 0.25f), new Vector2(0f, -30f)),
					Text = this.GetLocalizedValue("OverlayTitle1"),
					Scale = Vector2.One * 1.10f,
					FontOverride = FontAssets.DeathText,
					PrimaryColor = Color.White,
					OutlineColor = Color.DarkSlateBlue,
					FadeInEffect = (0.00f, 0.20f),
					FadeOutEffect = (0.85f, 1.00f),
					ShakeEffect = (15f, Vector2.One * 3f),
				});
				if (!Main.dedServ) OverlayText.Create(new OverlayTextLine
				{
					AnimationLength = lengthInSeconds,
					Position = (new Vector2(0.5f, 0.25f), new Vector2(0f, 20f)),
					Text = this.GetLocalizedValue("OverlaySubTitle1"),
					Scale = Vector2.One * 0.75f,
					FontOverride = FontAssets.DeathText,
					PrimaryColor = Color.White,
					OutlineColor = Color.DarkSlateBlue,
					FadeInEffect = (0.30f, 0.50f),
					FadeOutEffect = (0.85f, 1.00f),
					ShakeEffect = (15f, Vector2.One * 3f),
				});
				if (!Main.dedServ) SoundEngine.PlaySound(style: new($"{nameof(PathOfTerraria)}/Assets/Sounds/Conflux/CryoStalkerCast")
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
				if (Main.netMode != NetmodeID.MultiplayerClient) { NPCUtil.KillAllWithType(minionTypes); }

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
					SoundEngine.PlaySound(style: new($"{nameof(PathOfTerraria)}/Assets/Sounds/Conflux/CryoStalkerCast")
					{
						Volume = 0.85f,
					});
				}
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
					OutlineColor = Color.DarkSlateBlue,
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
					OutlineColor = Color.DarkSlateBlue,
					FadeInEffect = (0.30f, 0.50f),
					FadeOutEffect = (0.85f, 1.00f),
					ShakeEffect = (15f, Vector2.One * 3f),
				});

				SoundEngine.PlaySound(position: ctx.Center, style: new($"{nameof(PathOfTerraria)}/Assets/Sounds/HitEffects/FrostMagic")
				{
					Volume = 0.9f,
					MaxInstances = 3,
					PitchVariance = 0.2f,
				});

				foreach (ref Segment segment in segments.AsSpan())
				{
					for (int i = 0; i < 16; i++)
					{
						Vector2 pos = segment.Position + Main.rand.NextVector2Circular(32, 32);
						Vector2 vel = Main.rand.NextVector2Circular(12, 12);
						float scale = 1f + (Main.rand.NextFloat() * Main.rand.NextFloat());
						Dust.NewDustPerfect(pos, Main.rand.NextBool() ? DustID.Ice : DustID.Snow, vel, Scale: scale);
					}
				}
			}
		}
		else if (Cutscene.Type == CutsceneType.Death)
		{
			if (initialize)
			{
				// Kill minions.
				if (Main.netMode != NetmodeID.MultiplayerClient) { NPCUtil.KillAllWithType(minionTypes); }

				if (!Main.dedServ)
				{
					// Stop music immediately.
					Main.musicFade[Main.curMusic] = 0f;

					// Play death audio.
					SoundEngine.PlaySound(position: ctx.Center, style: SoundID.NPCDeath51 with
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
			else if (Cutscene.Counter == Cutscene.Length - 10 && !Main.dedServ)
			{
				SoundEngine.PlaySound(style: new($"{nameof(PathOfTerraria)}/Assets/Sounds/HitEffects/FrostMagic")
				{
					Volume = 0.85f,
					PauseBehavior = PauseBehavior.PauseWithGame,
				});
			}
			else if (justEnded)
			{
				if (Main.netMode != NetmodeID.MultiplayerClient)
				{
					if (SubworldSystem.Current is GlacialRealm realm)
					{
						realm.SignalBossDefeated();
					}
					NPC.dontTakeDamage = false;
					NPC.StrikeInstantKill();
					NPC.netUpdate = true;
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
					OutlineColor = Color.DarkSlateBlue,

					FadeInEffect = (0.00f, 0.20f),
					FadeOutEffect = defeatedFadeIn,
				});
				OverlayText.Create(new OverlayTextLine
				{
					AnimationLength = OverlayLength,
					Position = (new(0.5f, 0.25f), new(0f, 0f)),
					Text = this.GetLocalizedValue("OverlayDefeatedTitle"),
					FontOverride = FontAssets.DeathText,
					PrimaryColor = ColorUtils.FromHexRgb(0x7ce8ff),
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
					PrimaryColor = ColorUtils.FromHexRgb(0x7ce8ff),
					OutlineColor = Color.Black,

					FadeInEffect = defeatedFadeIn,
					FadeOutEffect = (0.60f, 1.00f),
					ShakeEffect = (10f, Vector2.One * 3f),
				});

				SoundEngine.PlaySound(position: ctx.Center, style: new($"{nameof(PathOfTerraria)}/Assets/Sounds/HitEffects/FrostMagic")
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
		Rectangle meleeHitbox = player.Hitbox;
		meleeHitbox.Inflate(Math.Max(item.width, 40), Math.Max(item.height, 40));
		if (!meleeHitbox.Intersects(NPC.getRect())) { return false; }

		foreach (ref readonly Segment segment in segments.AsSpan())
		{
			if (segment.Depth >= -0.05f && meleeHitbox.Intersects(segment.GetHitbox(0.9f)))
			{
				return true;
			}
		}

		return false;
	}
	public override bool? CanBeHitByProjectile(Projectile projectile)
	{
		Rectangle npcAabb = NPC.getRect();
		Rectangle projAabb = projectile.getRect();

		if (!projAabb.Intersects(npcAabb)) { return false; }
		if (!projectile.friendly) { return false; }

		foreach (ref readonly Segment segment in segments.AsSpan())
		{
			if (segment.Depth < -0.05f) { continue; }

			Rectangle segmentHitbox = segment.GetHitbox(0.9f);
			if (projectile.Colliding(projAabb, segmentHitbox))
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
			if (Cutscene.Type != CutsceneType.Death)
			{
				Cutscene = new(CutsceneType.Death, 0, 240);
				ResetAttack(ushort.MaxValue);
				NPC.netUpdate = true;
			}
			Phase = PhaseType.Second;
			NPC.dontTakeDamage = true;
			NPC.life = 1;
			return false;
		}

		return true;
	}

	public override bool CheckActive() => false;

#if DEBUG
	internal void DebugSetPhase(PhaseType phase)
	{
		Phase = phase;
		Cutscene = default;
		NPC.life = phase == PhaseType.Second
			? (int)(NPC.lifeMax * SecondPhaseHealthFactor)
			: NPC.lifeMax;
		NPC.dontTakeDamage = false;
		ResetAttack(30);
		NPC.netUpdate = true;
	}

	internal void DebugBeginDeath()
	{
		Cutscene = new(CutsceneType.Death, 0, 120);
		Phase = PhaseType.Second;
		NPC.life = 1;
		NPC.dontTakeDamage = true;
		ResetAttack(ushort.MaxValue);
		NPC.netUpdate = true;
	}
#endif

	internal float GetSegmentContactDamageFactor(float segmentFactor)
	{
		if (Phase == PhaseType.Idle || CutsceneActive) { return 0f; }
		return segmentFactor * (Phase == PhaseType.Second ? 1f : 0.7f);
	}

	private void ResetAttack(ushort cooldown)
	{
		attackType = AttackType.None;
		attackTimer = 0;
		attackDuration = 0;
		attackCooldown = cooldown;
		attackTarget = byte.MaxValue;
	}

	private void StartAttack(AttackType type, ushort duration, Player target)
	{
		attackType = type;
		attackTimer = 0;
		attackDuration = duration;
		attackTarget = (byte)target.whoAmI;
		attackAim = target.Center;
		attackHeading = Center.SafeDirection(target.Center, Vector2.UnitX);
		NPC.netUpdate = true;
	}

	private void UpdateAttacks(in Context ctx)
	{
#if FRIENDLY
		return;
#endif
		if (Phase == PhaseType.Idle || CutsceneActive || !NPC.HasValidTarget)
		{
			if (Main.netMode != NetmodeID.MultiplayerClient && attackType != AttackType.None)
			{
				ResetAttack(90);
				NPC.netUpdate = true;
			}
			return;
		}

		if (!Main.dedServ && attackType != AttackType.None)
		{
			Lighting.AddLight(ctx.Center, ColorUtils.FromHexRgb(0x7ce8ff).ToVector3() * 1.25f);
			UpdateAttackTelegraph();
		}

		if (attackType != AttackType.None)
		{
			attackTimer++;
			ExecuteAttack(in ctx);

			if (Main.netMode != NetmodeID.MultiplayerClient && attackTimer >= attackDuration)
			{
				ushort cooldown = (ushort)(Phase == PhaseType.Second ? 70 : 95);
				ResetAttack(cooldown);
				NPC.netUpdate = true;
			}
			return;
		}

		if (attackCooldown > 0)
		{
			attackCooldown--;
			return;
		}

		if (Main.netMode == NetmodeID.MultiplayerClient) { return; }

		Player target = Main.player[NPC.target];
		AttackType nextAttack = SelectNextAttack(target);
		ushort duration = nextAttack switch
		{
			AttackType.Bite => 72,
			AttackType.FrostRing => (ushort)(Phase == PhaseType.Second ? 105 : 70),
			AttackType.BreachSweep => 150,
			AttackType.RimeWall => 110,
			_ => 60,
		};
		StartAttack(nextAttack, duration, target);
	}

	private AttackType SelectNextAttack(Player target)
	{
		AttackType[] sequence = Phase == PhaseType.Second
			? [AttackType.FrostRing, AttackType.BreachSweep, AttackType.RimeWall, AttackType.Bite]
			: [AttackType.FrostRing, AttackType.BreachSweep, AttackType.Bite];

		AttackType selected = sequence[attackSequence++ % sequence.Length];
		if (selected == AttackType.Bite && target.Distance(Center) > 520f)
		{
			selected = AttackType.FrostRing;
		}
		return selected;
	}

	private void ExecuteAttack(in Context ctx)
	{
		if (attackTarget < Main.maxPlayers && Main.player[attackTarget] is { active: true, dead: false } target)
		{
			if (attackType is AttackType.Bite or AttackType.RimeWall)
			{
				attackAim = target.Center;
			}
		}

		if (!Main.dedServ && attackTimer == 1)
		{
			SoundEngine.PlaySound(new SoundStyle($"{nameof(PathOfTerraria)}/Assets/Sounds/Conflux/CryoStalkerCast")
			{
				Volume = 1.1f,
				Pitch = attackType == AttackType.Bite ? -0.75f : -0.25f,
				PitchVariance = 0.1f,
			}, ctx.Center);
		}

		if (Main.netMode == NetmodeID.MultiplayerClient) { return; }

		switch (attackType)
		{
			case AttackType.Bite:
				if (attackTimer == 42)
				{
					Vector2 direction = Center.SafeDirection(attackAim, Vector2.UnitY);
					Projectile.NewProjectile(NPC.GetSource_FromThis(), Center + direction * 90f, direction * 5f,
						ModContent.ProjectileType<YryothBite>(), ModeUtils.ProjectileDamage(300), 5f, Main.myPlayer, NPC.whoAmI);
				}
				break;

			case AttackType.FrostRing:
				int rings = Phase == PhaseType.Second ? 3 : 1;
				for (int ring = 0; ring < rings; ring++)
				{
					if (attackTimer == 35 + ring * 22) { SpawnFrostRing(ring); }
				}
				break;

			case AttackType.BreachSweep:
				if (attackTimer >= 35 && attackTimer <= 125 && attackTimer % 10 == 0)
				{
					SpawnBreachVolley();
				}
				break;

			case AttackType.RimeWall:
				if (attackTimer is 45 or 65) { SpawnRimeWall(attackTimer == 65); }
				break;
		}
	}

	private void UpdateAttackTelegraph()
	{
		if (attackTimer % 3 != 0) { return; }

		Color color = ColorUtils.FromHexRgb(0x7ce8ff);
		switch (attackType)
		{
			case AttackType.Bite when attackTimer < 42:
			{
				Vector2 direction = Center.SafeDirection(attackAim, Vector2.UnitY);
				Vector2 jawCenter = Center + direction * 115f;
				for (int i = 0; i < 4; i++)
				{
					float angle = Main.rand.NextFloat(MathHelper.TwoPi);
					Dust dust = Dust.NewDustPerfect(jawCenter + angle.ToRotationVector2() * 95f,
						DustID.IceTorch, -direction * 1.5f, Scale: 1.15f);
					dust.noGravity = true;
					dust.color = color;
				}
				break;
			}

			case AttackType.FrostRing when attackTimer < 35:
			{
				float radius = MathHelper.Lerp(70f, 175f, attackTimer / 35f);
				float safeDirection = Center.AngleTo(attackAim);
				for (int i = 0; i < 8; i++)
				{
					float angle = MathHelper.TwoPi * i / 8f;
					if (MathF.Abs(MathHelper.WrapAngle(angle - safeDirection)) < 0.3f) { continue; }
					Dust.NewDustPerfect(Center + angle.ToRotationVector2() * radius,
						DustID.IceTorch, Vector2.Zero, Scale: 1.1f).noGravity = true;
				}
				break;
			}

			case AttackType.BreachSweep when attackTimer < 35:
			{
				Vector2 direction = Center.SafeDirection(attackAim, Vector2.UnitY);
				for (int i = 1; i <= 5; i++)
				{
					Dust.NewDustPerfect(Center + direction * i * 55f, DustID.IceTorch,
						direction * 2f, Scale: 1.1f).noGravity = true;
				}
				break;
			}

			case AttackType.RimeWall when attackTimer < 45:
			{
				for (int row = -5; row <= 5; row++)
				{
					if (Math.Abs(row) <= 1) { continue; }
					float side = Main.rand.NextBool() ? -1f : 1f;
					Vector2 position = attackAim + new Vector2(side * 700f, row * 80f);
					Dust.NewDustPerfect(position, DustID.IceTorch, -Vector2.UnitX * side * 2f,
						Scale: 1.2f).noGravity = true;
				}
				break;
			}
		}
	}

	private void SpawnFrostRing(int ring)
	{
		const int ProjectileCount = 18;
		float safeDirection = Center.AngleTo(attackAim);
		float rotationOffset = ring * 0.17f;
		for (int i = 0; i < ProjectileCount; i++)
		{
			float angle = rotationOffset + MathHelper.TwoPi * i / ProjectileCount;
			if (MathF.Abs(MathHelper.WrapAngle(angle - safeDirection)) < 0.24f) { continue; }
			Vector2 velocity = angle.ToRotationVector2() * (5.5f + ring * 0.75f);
			SpawnIcicle(Center, velocity);
		}
	}

	private void SpawnBreachVolley()
	{
		Vector2 origin = Center;
		Vector2 direction = origin.SafeDirection(attackAim, Vector2.UnitY);
		float spread = Phase == PhaseType.Second ? 0.42f : 0.28f;
		for (int i = -1; i <= 1; i++)
		{
			SpawnIcicle(origin, direction.RotatedBy(spread * i) * (7.5f + MathF.Abs(i)));
		}
	}

	private void SpawnRimeWall(bool fromRight)
	{
		const int Rows = 11;
		float direction = fromRight ? -1f : 1f;
		float x = attackAim.X - direction * 900f;
		int safeRow = Rows / 2;
		for (int row = 0; row < Rows; row++)
		{
			if (Math.Abs(row - safeRow) <= 1) { continue; }
			Vector2 position = new(x, attackAim.Y + (row - Rows / 2f) * 80f);
			SpawnIcicle(position, Vector2.UnitX * direction * 9f);
		}
	}

	private void SpawnIcicle(Vector2 position, Vector2 velocity)
	{
		Projectile.NewProjectile(NPC.GetSource_FromThis(), position, velocity,
			ModContent.ProjectileType<YryothIcicle>(), ModeUtils.ProjectileDamage(300), 2f, Main.myPlayer);
	}

	internal bool TryArmRimeEruption(Player player, int band)
	{
		if (rimeEruptionTimer != 0 || band < 0 || band * SegmentsPerRimeBand >= segments.Length)
		{
			return false;
		}

		rimeEruptionBand = (byte)band;
		rimeEruptionTarget = (byte)player.whoAmI;
		rimeEruptionTimer = 1;
		NPC.netUpdate = true;
		return true;
	}

	internal void EmitLocalRimeWarning(int band, float progress)
	{
		if (Main.dedServ || band < 0 || progress < 0.2f) { return; }

		int start = band * SegmentsPerRimeBand;
		int end = Math.Min(start + SegmentsPerRimeBand, segments.Length);
		if (start < 0 || start >= end) { return; }

		Vector3 light = Color.Cyan.ToVector3() * MathHelper.Lerp(0.15f, 0.75f, progress);
		for (int i = start; i < end; i++)
		{
			Lighting.AddLight(segments[i].Position, light);
		}

		int dustFrequency = progress >= 0.8f ? 2 : progress >= 0.5f ? 4 : 7;
		if (Main.GameUpdateCount % (ulong)dustFrequency != 0) { return; }

		ref Segment segment = ref segments[Main.rand.Next(start, end)];
		Dust dust = Dust.NewDustPerfect(segment.Position + Main.rand.NextVector2Circular(segment.Length * 0.35f, 18f),
			DustID.IceTorch, -Vector2.UnitY * MathHelper.Lerp(0.5f, 2.5f, progress), Scale: 0.9f + progress * 0.7f);
		dust.noGravity = true;
	}

	private void UpdateRimeEruption()
	{
		if (rimeEruptionTimer == 0 || rimeEruptionBand == byte.MaxValue)
		{
			rimeWarningSoundPlayed = false;
			rimeBurstSoundPlayed = false;
			return;
		}

		if (!Main.dedServ)
		{
			EmitRimeEruptionEffects();
			if (!rimeWarningSoundPlayed)
			{
				SoundEngine.PlaySound(SoundID.Item30 with { Volume = 0.75f, Pitch = -0.15f }, GetRimeBandCenter());
				rimeWarningSoundPlayed = true;
			}
			if (rimeEruptionTimer >= RimeEruptionDelay && !rimeBurstSoundPlayed)
			{
				SoundEngine.PlaySound(SoundID.Item27 with { Volume = 1f, Pitch = 0.05f }, GetRimeBandCenter());
				rimeBurstSoundPlayed = true;
			}
		}

		if (rimeEruptionTimer == RimeEruptionDelay && Main.netMode != NetmodeID.MultiplayerClient)
		{
			SpawnRimeEruption();
		}

		rimeEruptionTimer++;
		if (rimeEruptionTimer <= RimeEruptionLifetime) { return; }

		rimeEruptionTimer = 0;
		rimeEruptionBand = byte.MaxValue;
		rimeEruptionTarget = byte.MaxValue;
		if (Main.netMode != NetmodeID.MultiplayerClient) { NPC.netUpdate = true; }
	}

	private void EmitRimeEruptionEffects()
	{
		float strength = rimeEruptionTimer <= RimeEruptionDelay
			? rimeEruptionTimer / (float)RimeEruptionDelay
			: 1f - (rimeEruptionTimer - RimeEruptionDelay) / (float)(RimeEruptionLifetime - RimeEruptionDelay);
		strength = MathUtils.Clamp01(strength);

		int start = rimeEruptionBand * SegmentsPerRimeBand;
		int end = Math.Min(start + SegmentsPerRimeBand, segments.Length);
		for (int i = start; i < end; i++)
		{
			ref Segment segment = ref segments[i];
			Lighting.AddLight(segment.Position, Color.Cyan.ToVector3() * (0.35f + strength));
			if (Main.rand.NextFloat() < 0.15f + strength * 0.45f)
			{
				Dust dust = Dust.NewDustPerfect(segment.Position + Main.rand.NextVector2Circular(segment.Length * 0.4f, 22f),
					DustID.IceTorch, Main.rand.NextVector2Circular(1.5f, 1.5f) - Vector2.UnitY * strength * 2f,
					Scale: 1f + strength * 0.9f);
				dust.noGravity = true;
			}
		}
	}

	private Vector2 GetRimeBandCenter()
	{
		int start = rimeEruptionBand * SegmentsPerRimeBand;
		int end = Math.Min(start + SegmentsPerRimeBand, segments.Length);
		Vector2 center = Vector2.Zero;
		for (int i = start; i < end; i++) { center += segments[i].Position; }
		return start < end ? center / (end - start) : Center;
	}

	private void SpawnRimeEruption()
	{
		Player? target = rimeEruptionTarget < Main.maxPlayers
			&& Main.player[rimeEruptionTarget] is { active: true, dead: false } activeTarget
			? activeTarget
			: null;
		int start = rimeEruptionBand * SegmentsPerRimeBand;
		int end = Math.Min(start + SegmentsPerRimeBand, segments.Length);

		for (int i = start; i < end; i++)
		{
			ref Segment segment = ref segments[i];
			Vector2 direction = target is null
				? -Vector2.UnitY
				: segment.Position.SafeDirection(target.Center, -Vector2.UnitY);
			float spread = (i - (start + end - 1) * 0.5f) * 0.08f;
			SpawnIcicle(segment.Position, direction.RotatedBy(spread) * 10.5f);
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
		int activePlayers = 0;
		foreach (Player player in Main.ActivePlayers)
		{
			if (!player.dead) { activePlayers++; }
		}
		activePlayers = Math.Max(1, activePlayers);
		int playerScale = 1 + (activePlayers - 1) / 2;
		int numSteps = (Phase is PhaseType.Second ? 2 : 1) * playerScale;
		for (int i = 1; i <= numSteps; i++)
		{
			if (numShamblings < i * 1.5f) { type = ModContent.NPCType<Shambling>(); break; }
			else if (numStalkers < i) { type = ModContent.NPCType<CryoStalker>(); break; }
			else if (numAbominables < i * 0.9f) { type = ModContent.NPCType<Abominable>(); break; }
		}

		if (type > 0)
		{
			var npc = NPC.NewNPCDirect(NPC.GetSource_FromThis(), (int)ctx.Center.X, (int)ctx.Center.Y, type);
			npc.netUpdate = true;
			if (!Main.dedServ) SoundEngine.PlaySound(position: ctx.Center, style: new($"{nameof(PathOfTerraria)}/Assets/Sounds/Conflux/CryoStalkerCast")
			{
				Pitch = -0.5f,
				PitchVariance = 0.03f,
				MaxInstances = 3,
			});

			spawnCooldown = (ushort)(Phase == PhaseType.Second ? 240 : 300);
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
			float target = quiet ? 0 : MathUtils.Clamp01(headVelocity.Length() * 0.075f);
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
		
		const int numSegments = 72;
		segments = new Segment[numSegments];
		
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
					Length = 160,
				};
			}
			else
			{
				float segFactor = segmentIndex / (float)numSegments;
				bool isSpiked = segmentIndex % 8 == 0;
				bool isLast = segmentIndex == numSegments - 1;
				byte sizeIndex = (byte)(segFactor < 0.8f ? 0 : (!isLast ? 1 : 2));
				string sizeName = sizeIndex switch { 0 => "Large", 1 => "Medium", 2 => "Small", _ => throw null! };
				string suffix = isSpiked ? "_Iced" : string.Empty;
				int length = sizeIndex switch { 0 => 80, 1 => 60, 2 => 40, _ => throw null! };
				
				segment = new()
				{
					TexturePath = $"{Texture}_Body_{sizeName}{suffix}",
					Frame = new SpriteFrame(1, 1) { PaddingX = 0, PaddingY = 0 },
					Position = nextPosition,
					BaseRotation = -MathHelper.PiOver2,
					Length = length,
					ContactDamageFactor = isSpiked ? 1f : 0f,
				};
			}

			nextPosition += new Vector2(0, segment.Length * 0.72f);
		}
	}

	private void UpdateSegments(in Context ctx)
	{
		if (segments.Length == 0) { return; }

		UpdateHeadMovement(in ctx);

		ref Segment head = ref segments[0];
		Vector2 oldHeadPosition = head.Position;
		head.Position = headPosition;
		head.Velocity = head.Position - oldHeadPosition;
		if (headVelocity != Vector2.Zero)
		{
			head.Rotation = headVelocity.ToRotation() + head.BaseRotation;
		}

		for (int segmentIndex = 1; segmentIndex < segments.Length; segmentIndex++)
		{
			ref Segment current = ref segments[segmentIndex];
			ref Segment previous = ref segments[segmentIndex - 1];
			Vector2 oldPosition = current.Position;
			Vector2 awayFromPrevious = previous.Position.SafeDirection(current.Position,
				-previous.Velocity.SafeNormalize(Vector2.UnitY));
			float spacing = (previous.Length + current.Length) * 0.38f * NPC.scale;

			current.Position = previous.Position + awayFromPrevious * spacing;
			current.Velocity = current.Position - oldPosition;
			current.Rotation = previous.Position.AngleTo(current.Position) + current.BaseRotation;
		}

		UpdateSegmentDepthAndEffects();
	}

	private void UpdateHeadMovement(in Context ctx)
	{
		Vector2 arenaCenter = GlacialRealm.IsActive && GlacialRealm.ArenaCenter.Get() is Point16 arenaPoint
			? arenaPoint.ToWorldCoordinates()
			: (NPC.HasValidTarget ? ctx.TargetCenter : headPosition);
		Vector2 targetCenter = NPC.HasValidTarget ? ctx.TargetCenter : arenaCenter;
		Vector2 focus = GlacialRealm.IsActive
			? Vector2.Lerp(arenaCenter, targetCenter, 0.65f)
			: targetCenter;

		float orbitAngle = globalCounter * (Phase == PhaseType.Second ? 0.014f : 0.011f) + NPC.whoAmI * 0.31f;
		float horizontalRadius = Phase == PhaseType.Second ? 720f : 640f;
		float verticalRadius = Phase == PhaseType.Second ? 430f : 360f;
		Vector2 desiredPosition = focus + new Vector2(
			MathF.Cos(orbitAngle) * horizontalRadius,
			MathF.Sin(orbitAngle * 1.37f) * verticalRadius - 100f);

		float speed = Phase == PhaseType.Second ? 15f : 12f;
		float maxTurn = Phase == PhaseType.Second ? 0.042f : 0.034f;

		if (attackType == AttackType.Bite && attackTimer < 58)
		{
			desiredPosition = attackAim + attackHeading * 160f;
			speed = 18f;
			maxTurn = 0.055f;
		}
		else if (attackType == AttackType.BreachSweep && attackTimer >= 20)
		{
			desiredPosition = attackAim + attackHeading * 1050f;
			speed = Phase == PhaseType.Second ? 22f : 19f;
			maxTurn = 0.022f;
		}

		if (Phase == PhaseType.Idle)
		{
			Vector2 idleFocus = GlacialRealm.IsActive ? arenaCenter : spawnPosition;
			desiredPosition = idleFocus + new Vector2(MathF.Cos(orbitAngle) * 220f,
				MathF.Sin(orbitAngle * 1.37f) * 140f - 80f);
			speed = 5f;
			maxTurn = 0.025f;
		}
		if (CutsceneActive)
		{
			speed *= Cutscene.Type == CutsceneType.Death ? 0.3f : 0.55f;
			maxTurn *= 0.75f;
		}

		Vector2 desiredDirection = headPosition.SafeDirection(desiredPosition, Vector2.UnitY);
		if (headVelocity == Vector2.Zero)
		{
			headVelocity = desiredDirection * speed;
		}
		else
		{
			float currentAngle = headVelocity.ToRotation();
			float angleDifference = MathHelper.WrapAngle(desiredDirection.ToRotation() - currentAngle);
			float nextAngle = currentAngle + MathHelper.Clamp(angleDifference, -maxTurn, maxTurn);
			float nextSpeed = MathUtils.StepTowards(headVelocity.Length(), speed, 0.35f);
			headVelocity = nextAngle.ToRotationVector2() * nextSpeed;
		}

		headPosition += headVelocity;

		if (Main.netMode == NetmodeID.MultiplayerClient && receivedHeadState)
		{
			networkHeadPosition += networkHeadVelocity;
			float correctionDistance = headPosition.Distance(networkHeadPosition);
			if (correctionDistance > 600f)
			{
				headPosition = networkHeadPosition;
				headVelocity = networkHeadVelocity;
			}
			else
			{
				headPosition = Vector2.Lerp(headPosition, networkHeadPosition, 0.08f);
				headVelocity = Vector2.Lerp(headVelocity, networkHeadVelocity, 0.06f);
			}
		}
	}

	private void UpdateSegmentDepthAndEffects()
	{
		for (int segmentIndex = 0; segmentIndex < segments.Length; segmentIndex++)
		{
			ref Segment segment = ref segments[segmentIndex];
			Point16 tilePoint = segment.Position.ToTileCoordinates16();
			bool submerged = TileUtils.InWorld(tilePoint) && TileUtils.HasSolid(Main.tile[tilePoint]);
			float targetDepth = submerged ? -0.35f : 0.2f;
			segment.Depth = MathHelper.Lerp(segment.Depth, targetDepth, 0.12f);

			if (globalCounter > 2 && submerged != segment.WasSubmerged && !Main.dedServ && segmentIndex % 8 == 0)
			{
				SoundEngine.PlaySound(SoundID.WormDig with { Volume = 0.35f, MaxInstances = 4 }, segment.Position);
				Main.instance.CameraModifiers.Add(new PunchCameraModifier(segment.Position,
					segment.Velocity.SafeNormalize(Vector2.UnitY), 1f, 2f, 18, 1200f,
					$"YryothSurface{NPC.whoAmI}_{segmentIndex}"));
			}

			segment.WasSubmerged = submerged;
		}
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

		// RenderSegments(in ctx, screenPos, filter: s => s.Depth >= 0f && (!drawingBackground || s.Depth > -0.05f));
		// RenderSegments(in ctx, screenPos, filter: s => s.Depth < 0f && (drawingBackground || s.Depth < 0.05f));
		RenderSegments(in ctx, screenPos, filter: s => s.Depth >= 0f && !drawingBackground);
		RenderSegments(in ctx, screenPos, filter: s => s.Depth < 0f && drawingBackground);
		return false;
	}

	private void RenderSegments(in Context ctx, Vector2 screenPos, Predicate<Segment>? filter = null)
	{
		filter ??= static _ => true;
		
		// Render segments back-to-front.
		for (int segmentIndex = segments.Length - 1; segmentIndex >= 0; segmentIndex--)
		{
			ref Segment segment = ref segments[segmentIndex];

			if (!filter(segment)) { continue; }

			if (!AssetUtils.AsyncValue(segment.TexturePath, ref segment.TextureCache, out Texture2D segTex)) { continue; }

			var segFrame = (Rectangle)segment.Frame.GetSourceRectangle(segTex);
			var segOrigin = (Vector2)(segFrame.Size() * 0.5f);
			var segPos = (Vector2)segment.Position;
			var segColor = (Color)Lighting.GetColor(segPos.ToTileCoordinates());
			float depthEffect = MathUtils.Clamp01(-segment.Depth * 2f);
			segColor = segColor.MultiplyRGB(new Color(Vector3.One * (1f - (depthEffect * 0.45f))));
			if (TryGetRimeVisualStrength(segmentIndex, out float rimeStrength))
			{
				float pulse = 0.78f + MathF.Sin(globalCounter * 0.42f + segmentIndex) * 0.22f;
				segColor = Color.Lerp(segColor, Color.Cyan, rimeStrength * pulse * 0.72f);
			}
			
			float usedScale = NPC.scale * MathHelper.Lerp(1f, 0.95f, depthEffect);
			using var _ = ValueOverride.Create(ref NPC.scale, usedScale);

			if (segmentIndex == 0
				&& AssetUtils.AsyncValue($"{Texture}_Horns", ref hornsTexture, out Texture2D hornTexture))
			{
				Rectangle hornFrame = hornTexture.Bounds;
				ctx.Animations.Render(NPC, hornTexture, hornFrame, screenPos, segColor, center: segPos,
					origin: hornFrame.Size() * 0.5f, rotation: segment.Rotation);
			}
			
			ctx.Animations.Render(NPC, segTex, segFrame, screenPos, segColor, center: segPos, origin: segOrigin, rotation: segment.Rotation);

			string glowPath = $"{segment.TexturePath}_Glow";
			if (ModContent.HasAsset(glowPath) && AssetUtils.AsyncValue(glowPath, ref segment.GlowCache, out Texture2D segGlow))
			{
				ctx.Animations.Render(NPC, segGlow, segFrame, screenPos, Color.White, center: segPos, origin: segOrigin, rotation: segment.Rotation);
			}
		}
	}

	private bool TryGetRimeVisualStrength(int segmentIndex, out float strength)
	{
		strength = 0f;
		if (rimeEruptionTimer == 0 || rimeEruptionBand == byte.MaxValue) { return false; }

		int start = rimeEruptionBand * SegmentsPerRimeBand;
		if (segmentIndex < start || segmentIndex >= start + SegmentsPerRimeBand) { return false; }

		strength = rimeEruptionTimer <= RimeEruptionDelay
			? rimeEruptionTimer / (float)RimeEruptionDelay
			: 1f - (rimeEruptionTimer - RimeEruptionDelay) / (float)(RimeEruptionLifetime - RimeEruptionDelay);
		strength = MathUtils.Clamp01(strength);
		return true;
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


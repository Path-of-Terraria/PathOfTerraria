using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using PathOfTerraria.Common.AI;
using PathOfTerraria.Common.NPCs;
using PathOfTerraria.Common.NPCs.Components;
using PathOfTerraria.Common.NPCs.Effects;
using PathOfTerraria.Common.Utilities;
using PathOfTerraria.Common.World.Utilities;
using PathOfTerraria.Content.Gores;
using PathOfTerraria.Content.Passives;
using PathOfTerraria.Core.IK;
using PathOfTerraria.Core.Time;
using PathOfTerraria.Utilities;
using PathOfTerraria.Utilities.Terraria;
using PathOfTerraria.Utilities.Xna;
using ReLogic.Content;
using ReLogic.Graphics;
using ReLogic.Utilities;
using Stubble.Core.Parser.TokenParsers;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;

#nullable enable

namespace PathOfTerraria.Content.Conflux;

internal sealed class InfernalBoss : ModNPC
{
	public record struct Limb()
	{
		public required IKLimb IK;
		public Point16? TileAttachment;
		public float PlacementAnimation;
		public bool PlayedSound;

		public static implicit operator Limb(IKLimb ik)
		{
			return new() { IK = ik };
		}
	}
	
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
		public NPCFootsteps Footsteps { get; } = npc.GetGlobalNPC<NPCFootsteps>();
		// public NPCTeleports Teleports { get; } = npc.GetGlobalNPC<NPCTeleports>();
	}

	private static readonly InverseKinematics.Config ikCfg = new()
	{
		RadianQuantization = MathHelper.ToRadians(4f),
	};
	private static readonly SpriteAnimation animIdle = new() { Id = "idle", Frames = [0], Speed = 3f };
	private static readonly SpriteAnimation animJump = new() { Id = "jump", Frames = [0] };
	private static readonly SpriteAnimation animFall = new() { Id = "fall", Frames = [0] };
	private static readonly SpriteAnimation animWalk = new() { Id = "walk", Frames = [0] };
	private static readonly SpriteAnimation animAttack = new() { Id = "attack", Frames = [0], Speed = 17f, Loop = false };
	private static Asset<Texture2D>? armsTexture;
	private static Asset<Texture2D>? helmetTexture;

	// public ref float FlightCounter => ref NPC.ai[3];
	public ref Flag Flags => ref Unsafe.As<float, Flag>(ref NPC.ai[3]);

	private float bodyAngleCurrent;
	private float bodyAngleTarget;
	private float movementAnimation;
	private float flightVolume;
	private SlotId flightSound;
	private Limb[] limbs = [];
	private (int NextIndex, float Cooldown) limbMovement = (0, 0f);

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
		NPC.width = 125;
		NPC.height = 125;
		NPC.knockBackResist = 0.0f;
		NPC.boss = true;
		NPC.lavaImmune = true;
		Music = 0;

		NPC.HitSound = new($"{nameof(PathOfTerraria)}/Assets/Sounds/HitEffects/FleshHit", 3) { Volume = 0.4f, Pitch = -0.5f, MaxInstances = 5 };
		NPC.DeathSound = SoundID.NPCDeath23 with { Pitch = -0.85f, PitchVariance = 0.15f, Identifier = $"{Name}Death" };

		NPC.TryEnableComponent<NPCNavigation>(e =>
		{
			e.JumpRange = new(25, 50);
		});
		NPC.TryEnableComponent<NPCMovement>(e =>
		{
			// e.Data.MaxSpeed = 0f;
			e.Data.MaxSpeed = 4.5f;
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

		SetupLimbs();
	}

	public override void AI()
	{
		Context ctx = new(NPC);

		// Invoke behaviors.
		ctx.Targeting.ManualUpdate(new(NPC));
		CustomBehavior(in ctx);
		// ctx.Teleports.ManualUpdate(new(NPC));
		ctx.Attacking.ManualUpdate(new(NPC));
		ctx.Movement.ManualUpdate(new(NPC));
	}

	private void CustomBehavior(in Context ctx)
	{
		bool atkActive = ctx.Attacking.Active;
		int atkProg = ctx.Attacking.Data.Progress;
		int atkBase = ctx.Attacking.Data.Dash.Start;

		// Reset overrides.
		ctx.Movement.Data.TargetOverride = null;
		ctx.Attacking.Data.ManualInitiation = true;

		UpdateLimbs(in ctx, out int numAttached, out float averageAngle);

		// NPC.aiStyle = -1;
		bool isFlying = numAttached >= Math.Max(1, limbs.Length);
		NPC.aiStyle = -1;
		NPC.noGravity = isFlying;
		
		if (isFlying && NPC.HasValidTarget)
		{
			NPCAimedTarget target = NPC.GetTargetData(ignorePlayerTankPets: true);
			Vector2 targetPos = target.Center;

			// Stand up, but only if that doesn't prevent the creature from getting into tight spaces.
			Vector2Int sizeInTiles = (NPC.Size * new Vector2(1f, 1.8f)).ToTileCoordinates();
			if (TileUtils.TryFitRectangleIntoTilemap(targetPos.ToTileCoordinates(), sizeInTiles, out Vector2Int adjustedPoint))
			{
				targetPos = (adjustedPoint + (sizeInTiles * 0.5f)).ToWorldCoordinates();
			}
			
			float acceleration = ctx.Movement.Data.Acceleration * TimeSystem.LogicDeltaTime;
			Vector2 direction = ctx.Center.DirectionTo(targetPos);
			
			// Slight upwards bias.
			direction = (direction + new Vector2(0f, -0.05f)).SafeNormalize(Vector2.UnitY);

			NPC.velocity = MovementUtils.DirAccel(NPC.velocity, direction, ctx.Movement.Data.MaxSpeed, acceleration);
			NPC.direction = direction.X > 0f ? 1 : -1;

			// Prevent navigation-driven movement, allow going through platforms.
			ctx.Movement.Data.InputOverride = new()
			{
				FallThroughPlatforms = true,
			};
		}

		// Update offsets.
		movementAnimation += MathF.Min(10f, NPC.velocity.Length());
		NPC.gfxOffY = MathF.Sin(((float)Main.timeForVisualEffects / 25f) + (movementAnimation / 40f)) * 6f;

		// Update rotation.
		float limbNormal = averageAngle - MathHelper.PiOver2;
		bodyAngleTarget = limbNormal * 0.55f;
		bodyAngleCurrent = MathUtils.LerpRadians(bodyAngleCurrent, bodyAngleTarget, 0.75f * TimeSystem.LogicDeltaTime);
		NPC.rotation = bodyAngleCurrent;
		Debug.Assert(!float.IsNaN(bodyAngleTarget));
		Debug.Assert(!float.IsNaN(bodyAngleCurrent));
		Debug.Assert(!float.IsNaN(NPC.rotation));

		// Shoot projectiles during non-respawn attacks.
		for (int i = 0; i < (atkActive ? 15 : 0); i++)
		{
			if (atkProg == 1)
			{
				SoundEngine.PlaySound(new($"{nameof(PathOfTerraria)}/Assets/Sounds/Conflux/FallenShamanAttack")
				{
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
			// Lighting.AddLight(NPC.Top, Color.Red.ToVector3() * 0.1f);

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
			SoundUtils.UpdateLoopingSound(ref flightSound, ctx.Center, volume, pitch, loopSound, _ => Main.npc[NPC.whoAmI] is { active: true } n && n == NPC);
		}
	}

	private void SetupLimbs()
	{
		Vector2 baseOffset = new(-10, 65);
		limbs =
		[
			// /*
			new IKLimb()
			{
				Offset = baseOffset + new Vector2(-10, -60),
				IsFlipped = true,
				RestingAngle = MathHelper.Pi,
				Segments =
				[
					new() { SrcRect = new(002, 002, 72, 98), TextureOrigin = new(016, 016), Length = 096, BaseAngle = MathHelper.ToRadians(-55f) },
					new() { SrcRect = new(078, 002, 44, 52), TextureOrigin = new(082, 006), Length = 056, BaseAngle = MathHelper.ToRadians(-55f) },
					new() { SrcRect = new(126, 002, 36, 44), TextureOrigin = new(132, 004), Length = 024, BaseAngle = MathHelper.ToRadians(-55f) },
				],
			},
			new IKLimb()
			{
				Offset = baseOffset + new Vector2(+100, -60),
				Segments =
				[
					new() { SrcRect = new(002, 002, 72, 98), TextureOrigin = new(016, 016), Length = 096, BaseAngle = MathHelper.ToRadians(-55f) },
					new() { SrcRect = new(078, 002, 44, 52), TextureOrigin = new(082, 006), Length = 056, BaseAngle = MathHelper.ToRadians(-55f) },
					new() { SrcRect = new(126, 002, 36, 44), TextureOrigin = new(132, 004), Length = 024, BaseAngle = MathHelper.ToRadians(-55f) },
				],
			},
			new IKLimb()
			{
				Offset = baseOffset + new Vector2(-70, -40),
				IsFlipped = true,
				RestingAngle = MathHelper.Pi,
				Segments =
				[
					new() { SrcRect = new(002, 002, 72, 98), TextureOrigin = new(016, 016), Length = 096, BaseAngle = MathHelper.ToRadians(-55f) },
					new() { SrcRect = new(078, 002, 44, 52), TextureOrigin = new(082, 006), Length = 056, BaseAngle = MathHelper.ToRadians(-55f) },
					new() { SrcRect = new(126, 002, 36, 44), TextureOrigin = new(132, 004), Length = 024, BaseAngle = MathHelper.ToRadians(-55f) },
				],
			},
			new IKLimb()
			{
				Offset = baseOffset + new Vector2(+50, -70),
				Segments =
				[
					new() { SrcRect = new(002, 002, 72, 98), TextureOrigin = new(016, 016), Length = 096, BaseAngle = MathHelper.ToRadians(-55f) },
					new() { SrcRect = new(078, 002, 44, 52), TextureOrigin = new(082, 006), Length = 056, BaseAngle = MathHelper.ToRadians(-55f) },
					new() { SrcRect = new(126, 002, 36, 44), TextureOrigin = new(132, 004), Length = 024, BaseAngle = MathHelper.ToRadians(-55f) },
				],
			},
			// */
		];

#if false
		// Double limbs.
		List<Limb> list = limbs.ToList();
		for (int i = 0; i < limbs.Length; i++)
		{
			Limb newLimb = limbs[i];
			newLimb.IK.Offset.X *= 0.5f;
			newLimb.IK.Offset.Y -= 16f;
			newLimb.IK.RestingAngle = MathF.Abs(newLimb.IK.RestingAngle - MathHelper.Pi);
			list.Add(newLimb);
		}
		limbs = list.ToArray();
#endif

		foreach (ref Limb limb in limbs.AsSpan())
		{
			limb.IK.TargetCurrent = NPC.Center;
			limb.IK.TargetWanted = NPC.Center;
		}
	}

	private void UpdateLimbs(in Context ctx, out int numAttached, out float averageAngle)
	{
#if DEBUG && true
		if (Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.N)) { SetupLimbs(); }
#endif
		
		Vector2 visualBodyCenter = NPC.Center;

		// DebugUtils.DrawInWorld(sb => sb.DrawString(FontAssets.MouseText.Value, "X", visualBodyCenter - Main.screenPosition, Color.Cyan));
		
		numAttached = 0;

		// Cooldown is reduced by the current speed.
		float moveSpeed = NPC.velocity.Length();
		float coolSpeed = MathF.Max(1f, moveSpeed);
		limbMovement.Cooldown = MathUtils.StepTowards(limbMovement.Cooldown, 0f, coolSpeed * TimeSystem.LogicDeltaTime);
		limbMovement.NextIndex = Math.Clamp(limbMovement.NextIndex, 0, limbs.Length - 1);

		int limbIndex = -1;
		bool placeAll = limbs.All(l => l.TileAttachment == null);
		bool advanceOrder = false;
		bool playedSounds = false;
		Vector2 averageDir = default;
		foreach (ref Limb limb in limbs.AsSpan())
		{
			limbIndex++;

			limb.IK.IsFlipped = limbIndex % 2 == 0;

			bool upNext = placeAll || (limbMovement.Cooldown <= 0f && limbIndex == limbMovement.NextIndex);
			float sqrLength = limb.IK.Length * limb.IK.Length;
			Vector2 visualLimbPos = limb.IK.GetPosition(NPC.Center + new Vector2(0f, NPC.gfxOffY), rotation: bodyAngleCurrent, xDir: NPC.spriteDirection);
			Vector2 logicalLimbPos = limb.IK.GetPosition(NPC.Center, rotation: bodyAngleCurrent, xDir: NPC.spriteDirection);
			limb.IK.Resolve(out InverseKinematics.Info info, in ikCfg, new()
			{
				Center = visualLimbPos,
				Direction = NPC.spriteDirection,
			});

			if (limb.TileAttachment?.ToWorldCoordinates() is {} attachPoint
			&& (attachPoint.DistanceSQ(logicalLimbPos) <= sqrLength || attachPoint.DistanceSQ(visualLimbPos) <= sqrLength))
			{
				if (upNext) { advanceOrder = true; }
			}
			// If not attached or is out of range, pick a new target.
			else if (upNext && PickLimbTarget(in ctx, ref limb, out Point16 tilePos))
			{
				limb.PlacementAnimation = 0f;
				limb.PlayedSound = false;
				limb.TileAttachment = tilePos;
				limb.IK.TargetWanted = ((Point16)tilePos).ToWorldCoordinates();
				(advanceOrder, limbMovement.Cooldown) = (true, (moveSpeed / limbs.Length) / 3f);
			}
			// Else - detach and flail.
			else
			{
				limb.TileAttachment = null;
				limb.IK.TargetCurrent = Vector2.Lerp(limb.IK.TargetCurrent, Vector2.Lerp(visualLimbPos + new Vector2(0f, 128f), NPC.GetTargetData().Center, 0.1f), 0.2f);
			}

			if (limb.TileAttachment != null)
			{
				numAttached++;

				static float MoveEasing(float x)
				{
					float c4 = (2f * MathF.PI) / 3;
					return x <= 0f ? 0f : x >= 1 ? 1f : (MathF.Pow(2, -5f * x) * MathF.Sin(((x * 10f) - 0.75f) * c4) + 1);
				}
				static float LiftEasing(float x)
				{
					return 1f - MathF.Pow(1f - x, 3f);
				}
				
				float anim = limb.PlacementAnimation = MathUtils.StepTowards(limb.PlacementAnimation, 1f, TimeSystem.LogicDeltaTime / 2.5f);
				float liftAnim = MathUtils.Clamp01(anim * 8);
				float liftEffect = LiftEasing(MathUtils.Clamp01(liftAnim <= 0.5f ? (liftAnim * 2f) : ((1f - liftAnim) * 2f)));
				var liftPos = new Vector2(limb.IK.TargetWanted.X, visualLimbPos.Y);
				var usedTarget = Vector2.Lerp(limb.IK.TargetWanted, visualLimbPos, liftEffect * 0.5f);
				limb.IK.TargetCurrent = Vector2.Lerp(limb.IK.TargetCurrent, usedTarget, MoveEasing(anim));

				const float soundRange = 16f;
				if (!Main.dedServ && !playedSounds && limb.IK.TargetCurrent.Distance(limb.IK.TargetWanted) <= soundRange && !limb.PlayedSound)
				{
					// SoundEngine.PlaySound(ctx.Footsteps.Data.StepSound!.Value with { Volume = 0.1f, Pitch = 0.5f }, limb.TargetWanted);
					SoundEngine.PlaySound(position: limb.IK.TargetWanted, style: new($"{nameof(PathOfTerraria)}/Assets/Sounds/HitEffects/FleshHit", 3)
					{
						MaxInstances = 10,
						Volume = 0.1f,
						Pitch = 0.0f,
						PitchVariance = 0.2f,
					});
					SoundEngine.PlaySound(position: limb.IK.TargetWanted, style: new($"{nameof(PathOfTerraria)}/Assets/Sounds/HitEffects/BoneHit", 3)
					{
						MaxInstances = 10,
						Volume = 0.2f,
						Pitch = -0.1f,
						PitchVariance = 0.3f,
					});
					SoundEngine.PlaySound(position: limb.IK.TargetWanted, style: new($"{nameof(PathOfTerraria)}/Assets/Sounds/Footsteps/MonsterStomp", 3)
					{
						MaxInstances = 10,
						Volume = 0.08f,
						Pitch = -0.1f,
						PitchVariance = 0.3f,
					});
					Main.instance.CameraModifiers.Add(new PunchCameraModifier(limb.IK.TargetWanted, new Vector2(0f, -1f), 1f, 4f, 15, 1000f, $"Footstep{limbIndex}"));
					limb.PlayedSound = true;
					playedSounds = true;
				}

				// Compute angle.
				Vector2 point = limb.IK.TargetCurrent;
				averageDir += point.DirectionFrom(visualBodyCenter);
				// DebugUtils.DrawInWorld(sb => sb.DrawString(FontAssets.MouseText.Value, "o", point - Main.screenPosition, Color.Orange));
			}
		}

		averageAngle = (averageDir / numAttached).ToRotation();
		averageAngle = !float.IsNaN(averageAngle) ? averageAngle : 0f;

		if (advanceOrder)
		{
			limbMovement.NextIndex = MathUtils.Modulo(limbMovement.NextIndex + NPC.spriteDirection, limbs.Length);
		}
	}

	private bool PickLimbTarget(in Context ctx, ref Limb limb, [MaybeNullWhen(false)] out Point16 result)
	{
		// Pick a direction to use for dot products.
		// With some bias towards the bottom.
		Vector2 movDir = (NPC.velocity + new Vector2(0f, 0.5f)).SafeNormalize(Vector2.UnitY);
		
		Vector2 startPos = limb.IK.GetPosition(NPC.Center, rotation: bodyAngleCurrent, xDir: NPC.spriteDirection);
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
				if (!tile.HasTile || (!Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType]))
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
					bool tileSurrounded = true
						&& WorldUtilities.SolidTile(x + 0, y - 1)
						&& WorldUtilities.SolidTile(x + 0, y + 1)
						&& WorldUtilities.SolidTile(x - 1, y + 0)
						&& WorldUtilities.SolidTile(x + 1, y + 0);
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

	public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
	{
		Context ctx = new(NPC);

		helmetTexture ??= ModContent.Request<Texture2D>($"{Texture}_Helmet", AssetRequestMode.ImmediateLoad);

		Texture2D bodyTexture = TextureAssets.Npc[Type].Value;
		ctx.Animations.Render(bodyTexture, NPC, screenPos, drawColor); //, origin: bodyTexture.Size() * new Vector2(0.5f, 0.75f));
		RenderArms(screenPos, drawColor);
		// ctx.Animations.Render(helmetTexture.Value, NPC, screenPos, drawColor);
		return false;
	}

	private void RenderArms(Vector2 screenPos, Color drawColor)
	{
		armsTexture ??= ModContent.Request<Texture2D>($"{Texture}_Limbs", AssetRequestMode.ImmediateLoad);

		foreach (ref Limb limb in limbs.AsSpan())
		{
			for (int j = 0; j < limb.IK.Segments.Length; j++)
			{
				DrawData drawData = limb.IK.GetDrawParams(armsTexture.Value, j, screenPos, NPC.spriteDirection);
				drawData.color = drawData.color.MultiplyRGBA(drawColor);
				// drawData.color = Lighting.GetColor((drawData.position + Main.screenPosition).ToTileCoordinates());
				Main.EntitySpriteDraw(drawData);
			}

#if DEBUG && false
			for (int j = 0; j < limb.Results.Length; j++)
			{
				Main.spriteBatch.DrawString(FontAssets.MouseText.Value, "x", arm.Results[j] - Main.screenPosition, Color.Cyan);
			}
#endif
		}
	}
}


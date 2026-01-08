//#define DEBUG_GIZMOS
//#define NEVER_ATTACK

using System.IO;
using System.Runtime.CompilerServices;
using PathOfTerraria.Common.AI;
using PathOfTerraria.Common.Encounters;
using PathOfTerraria.Common.NPCs.Components;
using PathOfTerraria.Common.NPCs.Effects;
using PathOfTerraria.Common.Utilities;
using PathOfTerraria.Core.Time;
using PathOfTerraria.Utilities.Terraria;
using PathOfTerraria.Utilities.Xna;
using ReLogic.Content;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;

#nullable enable
#pragma warning disable IDE0042 // Deconstruct variable declaration
#pragma warning disable IDE0053 // Use expression body for lambda expression
#pragma warning disable IDE2003 // Blank line required between block and subsequent statement

namespace PathOfTerraria.Content.Conflux;

/// <summary> Heavy low-speed demon that annihilates its targets.  </summary>
internal sealed class FallenTyrant : Fallen
{
	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		Main.npcFrameCount[Type] = 6;
	}
	public override void SetDefaults()
	{
		base.SetDefaults();

		NPC.lifeMax = 250;
		NPC.defense = 20;
		NPC.damage = 40;
		NPC.width = 36;
		NPC.height = 38;
		NPC.knockBackResist = 0.2f;

		Behavior.MaxSpeed = 1.5f;
	}
}

/// <summary> Brute medium-speed demon that stabs its targets.  </summary>
internal sealed class FallenSavage : Fallen
{
	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		Main.npcFrameCount[Type] = 6;
	}
	public override void SetDefaults()
	{
		base.SetDefaults();

		NPC.lifeMax = 150;
		NPC.defense = 8;
		NPC.damage = 35;
		NPC.width = 24;
		NPC.height = 40;
		NPC.knockBackResist = 0.3f;

		Behavior.MaxSpeed = 3f;
	}
}

/// <summary> Sneaky fast-speed demon that teleports at the player.  </summary>
internal sealed class FallenSchemer : Fallen
{
	private static readonly SpriteAnimation animIdle = new()
	{
		Id = "idle",
		Frames = [0],
	};
	private static readonly SpriteAnimation animWalk = new()
	{
		Id = "walk",
		Frames = [1, 2, 3, 4, 5, 6],
	};
	private static readonly SpriteAnimation animJump = new()
	{
		Id = "jump",
		Frames = [2],
		Speed = 4f,
		Loop = true,
	};
	private static SpriteAnimation animAttack => new()
	{
		Id = "attack",
		Frames = [7, 7, 8, 9, 9, 10, 10, 11, 12, 13, 13, 13, 13, 14],
		Speed = 17f,
		Loop = false,
	};
	private static readonly SpriteAnimation animTeleport = new()
	{
		Id = "teleport",
		Frames = [15],
	};

	private Vector2 teleportSource;
	private Vector2 teleportTarget;

	public ref ushort TeleportCooldown => ref Unsafe.As<float, ushort>(ref Unsafe.AddByteOffset(ref NPC.localAI[2], 2));

	protected override Asset<Texture2D> SlashTexture => ModContent.Request<Texture2D>($"{GetType().FullName}_Slash".Replace('.', '/'));

	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		Main.npcFrameCount[Type] = 4;
	}
	public override void SetDefaults()
	{
		base.SetDefaults();

		NPC.lifeMax = 125;
		NPC.defense = 25;
		NPC.damage = 20;
		NPC.width = 20;
		NPC.height = 44;
		NPC.knockBackResist = 0.5f;

		Behavior.MaxSpeed = 5.5f;
		Behavior.AttackDashTick = (ushort)(0.2 * 60);
		Behavior.AttackDashSoundTick = (ushort)(Behavior.AttackDashTick - 5);
		Behavior.AttackSlashSoundTick = (ushort)(Behavior.AttackDashTick - 3);
		Behavior.AttackNoGravityEndTick = (ushort)(Behavior.AttackDashTick + (0.25 * 60));
		Behavior.AttackDamageEndTick = (ushort)(Behavior.AttackDashTick + (0.25 * 60));
		Behavior.MaxAttackCooldown = Behavior.MinAttackCooldown = 10;

		NPC.GetGlobalNPC<NPCAnimations>().BaseFrame = new(4, 4);
	}

	public override void SendExtraAI(BinaryWriter writer)
	{
		base.SendExtraAI(writer);

		writer.WriteVector2(teleportTarget);
	}
	public override void ReceiveExtraAI(BinaryReader reader)
	{
		base.ReceiveExtraAI(reader);

		teleportTarget = reader.ReadVector2();
	}

	protected override Context InnerAI()
	{
		Context ctx = base.InnerAI();

		Portalling(in ctx);

		return ctx;
	}

	protected override SpriteAnimation? ChooseWantedAnimation(in Context ctx)
	{
		const float MinWalkSpeed = 0.5f;

		Vector2 effectiveVelocity = NPC.position - NPC.oldPosition;

		return ctx.Animations.Current switch
		{
			_ when ActiveAction == ActionType.Attack => animAttack,
			_ when ActiveAction == ActionType.Teleport => animTeleport,
			_ when MathF.Abs(effectiveVelocity.Y) > 5f => animJump,
			_ when effectiveVelocity.Y == 0f && Math.Abs(effectiveVelocity.X) >= MinWalkSpeed => animWalk with { Speed = effectiveVelocity.X * animWalk.Speed * 4f * NPC.spriteDirection },
			_ when effectiveVelocity.Y == 0f && Math.Abs(effectiveVelocity.X) <= MinWalkSpeed => animIdle,
			_ => null,
		};
	}

	private void Portalling(in Context ctx)
	{
		// Cool down cooldowns.
		if (TeleportCooldown > 0) { TeleportCooldown--; }

		bool TryPickPosition(in Context ctx)
		{
			Point targetPoint = ctx.TargetCenter.ToTileCoordinates();
			Rectangle targetArea = new Rectangle(targetPoint.X, targetPoint.Y, 0, 0).Inflated(10, 6);
			var spawn = new SpawnPlacement
			{
				Area = targetArea,
				CollisionSize = NPC.Size.ToPoint(),
				MinDistanceFromPlayers = 8f,
				MinDistanceFromEnemies = 64f,
				SkippedLiquids = LiquidMask.All,
				OnGround = true,
			};

			static bool IsSpawnPointSuitable(in Context ctx, Vector2 spawnPos)
			{
				// Must be significantly closer.
				if (spawnPos.DistanceSQ(ctx.TargetCenter) > 128 + ctx.Center.DistanceSQ(ctx.TargetCenter)) { return false; }

				// Must be change relative directions.	
				if (Math.Sign(ctx.Center.X - ctx.TargetCenter.X) == Math.Sign(spawnPos.X - ctx.TargetCenter.X)) { return false; }

				return true;
			}

			if (EnemySpawning.TryFindingSpawnPosition(spawn, out Vector2 spawnPos) && IsSpawnPointSuitable(in ctx, spawnPos))
			{
				teleportTarget = spawnPos;
				NPC.netUpdate = true;
				return true;
			}

			return false;
		}

		const float FarAwayDistance = 512f;
		const int DisappearStart = (int)(0.0 * 60);
		const int DisappearEnd = (int)(0.2 * 60);
		const int ReappearStart = (int)(0.7 * 60);
		const int ReappearEnd = (int)(0.9 * 60);
		const int InvulnerabilityStart = (int)(0.0 * 60);
		const int InvulnerabilityEnd = (int)(0.8 * 60);
		const int MaxCooldown = (int)(3.0 * 60);

		// Initiate teleportation if not busy.
		if (Main.netMode != NetmodeID.MultiplayerClient
		&& ActiveAction == ActionType.None && TeleportCooldown == 0)
		{
			bool farAway = ctx.Center.DistanceSQ(ctx.TargetCenter) >= FarAwayDistance * FarAwayDistance;
			bool targetMayBeAttackingUs = NPC.HasValidTarget
				&& NPC.GetTargetData().Type == Terraria.Enums.NPCTargetType.Player
				&& Main.player[NPC.target] is { } p && p.itemAnimation > 0
				&& p.direction == MathF.Sign(ctx.Center.X - p.Center.X);

			if ((farAway || targetMayBeAttackingUs) && TryPickPosition(in ctx))
			{
				(ActiveAction, ActionProgress) = (ActionType.Teleport, 0);
				NPC.netUpdate = true;
			}
		}

		void SpawnParticles(Vector2 center, int amount)
		{
			if (Main.dedServ) { return; }

			for (int i = 0; i < amount; i++)
			{
				Vector2 dustPos = center + Main.rand.NextVector2Circular(NPC.width * 0.5f, NPC.height * 0.5f);
				Vector2 dustVel = Main.rand.NextVector2Circular(1f, 2f) - (Vector2.UnitY * 1f);
				Dust.NewDustPerfect(dustPos, DustID.Blood, dustVel, Alpha: 10, Scale: 1.4f, newColor: Color.OrangeRed);
			}
		}

		if (ActiveAction == ActionType.Teleport)
		{
			float alphaDisappear = MathUtils.Clamp01((ActionProgress - DisappearStart) / (float)(DisappearEnd - DisappearStart));
			float alphaReappear = MathUtils.Clamp01((ActionProgress - ReappearStart) / (float)(ReappearEnd - ReappearStart));
			float alphaTotal = 1f - alphaDisappear + alphaReappear;
			NPC.alpha = byte.MaxValue - (byte)(alphaTotal * byte.MaxValue);

			float entrancePower = MathUtils.DistancePower(MathF.Abs(ActionProgress - DisappearEnd), 1, 20);
			float exitPower = MathUtils.DistancePower(MathF.Abs(ActionProgress - ReappearStart), 1, 20);

			if (entrancePower > 0f) { Lighting.AddLight(teleportSource, Color.OrangeRed.ToVector3() * entrancePower); }
			if (exitPower > 0f) { Lighting.AddLight(teleportTarget, Color.OrangeRed.ToVector3() * exitPower); }

			if (ActionProgress == 0)
			{
				NPC.velocity.X = NPC.direction * 2f;
				NPC.velocity.Y = -4f;
			}

			NPC.dontTakeDamage = ActionProgress >= InvulnerabilityStart && ActionProgress <= InvulnerabilityEnd;

			if (ActionProgress < DisappearEnd)
			{
				SpawnParticles(NPC.Center, 1);
				teleportSource = NPC.Center;
			}
			else if (ActionProgress == DisappearEnd)
			{
				SpawnParticles(NPC.Center, 10);
				SoundEngine.PlaySound(SoundID.Shimmer1 with { Pitch = 0.4f, PitchVariance = 0.1f }, NPC.Center);
				//SoundEngine.PlaySound(SoundID.DD2_DarkMageAttack with { Pitch = 0.4f, PitchVariance = 0.1f }, NPC.Center);
				NPC.alpha = byte.MaxValue;

				// Try to find a more up-to-date position.
				TryPickPosition(in ctx);
			}
			else if (ActionProgress == ReappearStart)
			{
				NPC.Center = teleportTarget;
				NPC.spriteDirection = NPC.direction = (ctx.Center.X - ctx.TargetCenter.X) > 0f ? 1 : -1;
				NPC.velocity.X = NPC.direction * 2f;
				NPC.velocity.Y = -5f;
				
				SpawnParticles(NPC.Center, 25);
				SoundEngine.PlaySound(SoundID.DD2_DarkMageAttack with { Pitch = -0.2f, PitchVariance = 0.1f }, NPC.Center);
			}
			else if (ActionProgress >= ReappearEnd)
			{
				NPC.alpha = 0;
				(ActiveAction, ActionProgress) = (ActionType.None, 0);
				TeleportCooldown = MaxCooldown;
				NPC.dontTakeDamage = false;
			}

			ActionProgress++;
		}
	}
}

internal abstract class Fallen : ModNPC
{
	protected struct Context(NPC npc)
	{
		public Vector2 Center { get; } = npc.Center;
		public NPCAnimations Animations { get; } = npc.GetGlobalNPC<NPCAnimations>();
		public NPCNavigation Navigation { get; } = npc.GetGlobalNPC<NPCNavigation>();
		public NPCTargetTracking Tracking { get; } = npc.GetGlobalNPC<NPCTargetTracking>();

		public Vector2 TargetCenter;
		public Rectangle TargetRect;
	}

	protected struct Stats()
	{
		private static readonly Stats Default = new();

		public Vector2 AttackInitiationRange = new(100f, 250f);
		public Vector2 AttackDashVelocity = new(10f, 5f);
		public ushort AttackLengthInTicks = (ushort)(1.0 * 60);
		public ushort AttackDashTick = (ushort)(0.33 * 60);
		public ushort AttackDashSoundTick = (ushort)(Default.AttackDashTick - 5);
		public ushort AttackSlashSoundTick = (ushort)(Default.AttackDashTick - 3);
		public ushort AttackNoGravityEndTick = (ushort)(Default.AttackDashTick + (0.25 * 60));
		public ushort AttackDamageEndTick = (ushort)(Default.AttackDashTick + (0.25 * 60));
		public ushort MinAttackCooldown = (ushort)(1.00f * 60);
		public ushort MaxAttackCooldown = (ushort)(1.00f * 60);
		public float MaxSpeed = 4f;
		public float Acceleration = 32f;
		public (float Ground, float Air) Friction = (8f, 2f);
	}

	public enum ActionType : byte
	{
		None,
		Attack,
		Teleport,
	}

	private static readonly SpriteAnimation animIdle = new()
	{
		Id = "idle",
		Frames = [1],
	};
	private static readonly SpriteAnimation animWalk = new()
	{
		Id = "walk",
		Frames = [0, 1, 2, 3, 4, 5],
	};
	private static readonly SpriteAnimation animJump = new()
	{
		Id = "jump",
		Frames = [2],
		Speed = 4f,
		Loop = true,
	};
	private static readonly SpriteAnimation animAttack = new()
	{
		Id = "attack",
		Frames = [1, 1, 1, 4, 5, 5, 5],
		Speed = 9f,
		Loop = false,
	};

	private Footsteps footsteps = new();
	protected AttackInstance? Attack;
	protected Stats Behavior = new();

	public ref float AttackAngle => ref NPC.localAI[0];
	public ref ushort ActionProgress => ref Unsafe.As<float, ushort>(ref Unsafe.AddByteOffset(ref NPC.localAI[1], 0));
	public ref ushort AttackCooldown => ref Unsafe.As<float, ushort>(ref Unsafe.AddByteOffset(ref NPC.localAI[1], 2));
	public ref ActionType ActiveAction => ref Unsafe.As<float, ActionType>(ref Unsafe.AddByteOffset(ref NPC.localAI[2], 0));
	public int AttackSign => AttackDirection.X >= 0f ? 1 : -1;
	public Vector2 AttackDirection
	{
		get => AttackAngle.ToRotationVector2();
		set => AttackAngle = value.ToRotation();
	}

	protected virtual Asset<Texture2D> SlashTexture => ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/Misc/Slash");

	public override void SetStaticDefaults()
	{
		NPCID.Sets.TeleportationImmune[Type] = true;
		NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Shimmer] = true;
	}

	public override void SetDefaults()
	{
		NPC.aiStyle = -1;
		NPC.HitSound = SoundID.NPCHit56 with { Pitch = +0.45f, PitchVariance = 0.11f, Identifier = "FallenHit" };
		NPC.DeathSound = SoundID.NPCDeath23 with { Pitch = +0.45f, PitchVariance = 0.15f, Identifier = "FallenDeath" };

		NPC.TryEnableComponent<NPCNavigation>(e =>
		{
			e.JumpRange = new(8, 20);
		});
		NPC.TryEnableComponent<NPCAnimations>(e =>
		{
			e.BaseFrame = new(1, 6);
		});
		NPC.TryEnableComponent<NPCTargetTracking>();

		NPC.TryEnableComponent<NPCHitEffects>(c =>
		{
			c.AddGore(new NPCHitEffects.GoreSpawnParameters(132, 1, NPCHitEffects.OnDeath));

			c.AddDust(new NPCHitEffects.DustSpawnParameters(DustID.Blood, 15));
			c.AddDust(new NPCHitEffects.DustSpawnParameters(DustID.Blood, 100, NPCHitEffects.OnDeath));
		});
	}

	public override void AI() { InnerAI(); }

	protected virtual Context InnerAI()
	{
		Context ctx = new(NPC);

		ctx.Animations.Advance();
		UpdateTarget(ref ctx);
		Movement(ref ctx);
		Attacking(ref ctx);
		UpdateAnimations(ref ctx);
		UpdateEffects(ref ctx);

		return ctx;
	}

	private void UpdateTarget(ref Context ctx, bool forceReset = false)
	{
		if (!NPC.HasValidTarget || forceReset)
		{
			Terraria.Utilities.NPCUtils.TargetClosestCommon(NPC, faceTarget: false);
		}

		ctx.TargetCenter = ctx.Tracking.GetTargetCenter(NPC);
		ctx.TargetRect = NPC.HasValidTarget ? NPC.GetTargetData().Hitbox : default;
	}

	public override bool? CanFallThroughPlatforms()
	{
		if (NPC.TryGetGlobalNPC(out NPCNavigation nav))
		{
			return nav.FallThroughPlatforms;
		}

		return null;
	}

	public override void SendExtraAI(BinaryWriter writer)
	{
		NPC.GetGlobalNPC<NPCNavigation>().SendPath(NPC, writer);
	}
	public override void ReceiveExtraAI(BinaryReader reader)
	{
		NPC.GetGlobalNPC<NPCNavigation>().ReceivePath(NPC, reader);
	}

	private void Movement(ref Context ctx)
	{
		// Push against peers.
		PushBehavior.Update(new PushBehavior.Context(NPC)
		{
			RequiredNpcType = NPC.type,
		});

		// Friction.
		float friction = NPC.velocity.Y == 0f ? Behavior.Friction.Ground : Behavior.Friction.Air;
		NPC.velocity.X = MathUtils.StepTowards(NPC.velocity.X, 0f, friction * TimeSystem.LogicDeltaTime);

		// Slopes.
		if (NPC.velocity.Y == 0f) { Collision.StepDown(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.stepSpeed, ref NPC.gfxOffY); }

		Collision.StepUp(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.stepSpeed, ref NPC.gfxOffY);

		if (!NPC.HasValidTarget || ActiveAction != ActionType.None)
		{
			return;
		}

		ctx.Navigation.Process(out NPCNavigation.Result navResult, new()
		{
			NPC = NPC,
			TargetPosition = ctx.TargetCenter,
		});

		// Fallback movement.
		if (!navResult.HadPath && ctx.Navigation.StateFlags.HasFlag(NPCNavigation.StateFlag.PathNotFound))
		{
			navResult.MovementVector.X = MathHelper.Clamp(ctx.TargetCenter.X - ctx.Center.X, -1f, +1f);

			if (NPC.velocity.Y == 0f)
			{
				float yDiff = ctx.TargetCenter.Y - ctx.Center.Y;

				if (yDiff < -NPC.height)
				{
					navResult.JumpVector.Y = -10f;
				}
			}
		}

		// Horizontal acceleration.
		if (NPC.velocity.Y == 0f && navResult.MovementVector.X != 0f)
		{
			NPC.velocity.X = MathUtils.StepTowards(NPC.velocity.X, Behavior.MaxSpeed * navResult.MovementVector.X, Behavior.Acceleration * TimeSystem.LogicDeltaTime);
			NPC.direction = navResult.MovementVector.X > 0f ? 1 : -1;
		}

		// Jumping.
		if (navResult.JumpVector != default)
		{
			NPC.velocity = navResult.JumpVector;
			NPC.direction = navResult.JumpVector.X != 0f ? (navResult.JumpVector.X > 0f ? 1 : -1) : NPC.direction;
		}
	}

	private void Attacking(ref Context ctx)
	{
		// Cool down cooldowns.
		if (AttackCooldown > 0) { AttackCooldown--; }

		Vector2 targetDiff = ctx.TargetCenter - ctx.Center;

		// Reset mutated variables.
		NPC.damage = -1;
		NPC.noGravity = false;
		NPC.color = Color.White;

		// Check if we can initiate an attack.
#if !NEVER_ATTACK
		Vector2 initRange = Behavior.AttackInitiationRange;

		if (ActiveAction == ActionType.None && AttackCooldown == 0 /*&& NPC.velocity.Y == 0f*/ && MathF.Abs(targetDiff.X) <= initRange.X && MathF.Abs(targetDiff.Y) <= initRange.Y)
		{
			if (Collision.CanHitLine(NPC.position, NPC.width, NPC.height, ctx.TargetRect.TopLeft(), ctx.TargetRect.Width, ctx.TargetRect.Height))
			{
				(ActiveAction, ActionProgress) = (ActionType.Attack, 0);
			}
		}
#endif

		// Update attacks.
		if (ActiveAction == ActionType.Attack)
		{
			// Play sounds.
			if (ActionProgress == Behavior.AttackDashSoundTick)
			{
				SoundEngine.PlaySound(position: NPC.Center, style: SoundID.Item71 with { Volume = 0.6f, Pitch = -0.80f, PitchVariance = 0.15f, MaxInstances = 3 });
			}
			else if (ActionProgress == Behavior.AttackSlashSoundTick)
			{
				SoundEngine.PlaySound(position: NPC.Center, style: SoundID.NPCHit56 with { Volume = 0.25f, Pitch = +0.85f, PitchVariance = 0.1f, MaxInstances = 3 });
			}

			if (ActionProgress < Behavior.AttackDashTick)
			{
				// Update Attack direction.
				AttackDirection = targetDiff.SafeNormalize(Vector2.UnitX * AttackSign);

				// Aim starts to update slower and slower.
				const float ChargeFactorForFullAimLag = 0.99f;
				ctx.Tracking.AimLag = Vector2.Lerp(default, Vector2.One * 0.2f, MathF.Min(1f, (ActionProgress / (float)Behavior.AttackDashTick) / ChargeFactorForFullAimLag));

				// Slow-slide.
				NPC.velocity.X = 0.4f * AttackSign;
			}
			else if (ActionProgress == Behavior.AttackDashTick)
			{
				// Dash.
				NPC.velocity = AttackDirection * Behavior.AttackDashVelocity;

				// Prepare damage.
				Attack = new AttackInstance
				{
					Aabb = default,
					Direction = default,
					Damage = NPC.defDamage,
					Knockback = 10f,
					Filter = AttackInstance.EnemyAttackFilter,
					Predicate = e => e is not NPC n || n.type != NPC.type,
					DeathReason = _ => PlayerDeathReason.ByNPC(NPC.whoAmI),
					ExcludedEntity = NPC,
					HitEntities = Attack?.HitEntities,
				};
				Attack.HitEntities?.Clear();
			}
			else
			{
				// Slow down.
				NPC.velocity.X *= 0.86f;
				// But also defy gravity for a few ticks.
				NPC.noGravity = ActionProgress >= Behavior.AttackDashTick && ActionProgress < Behavior.AttackNoGravityEndTick;
			}

			// Deal damage starting with the dash.

			if (Attack != null && ActionProgress >= Behavior.AttackDashTick && ActionProgress < Behavior.AttackDamageEndTick)
			{
				(Vector2 hitboxCenter, Rectangle hitboxAabb) = GetDamageArea();
				Attack.Aabb = hitboxAabb;
				Attack.Direction = AttackDirection;
				Attack.DamageEntities();
			}

			// End attack, start cooldown.
			if (ActionProgress >= Behavior.AttackLengthInTicks)
			{
				(ActiveAction, ActionProgress) = (ActionType.None, 0);
				AttackCooldown = (ushort)Main.rand.Next(Behavior.MinAttackCooldown, Behavior.MaxAttackCooldown);
				ctx.Tracking.AimLag = default;
			}

			ActionProgress++;
		}
	}

	protected virtual SpriteAnimation? ChooseWantedAnimation(in Context ctx)
	{
		const float MinWalkSpeed = 0.5f;

		Vector2 effectiveVelocity = NPC.position - NPC.oldPosition;

		return ctx.Animations.Current switch
		{
			_ when ActiveAction == ActionType.Attack => animAttack,
			_ when MathF.Abs(effectiveVelocity.Y) > 5f => animJump,
			_ when effectiveVelocity.Y == 0f && Math.Abs(effectiveVelocity.X) >= MinWalkSpeed => animWalk with { Speed = effectiveVelocity.X * animWalk.Speed * 4f * NPC.spriteDirection },
			_ when effectiveVelocity.Y == 0f && Math.Abs(effectiveVelocity.X) <= MinWalkSpeed => animIdle,
			_ => null,
		};
	}

	private void UpdateAnimations(ref Context ctx)
	{
		SpriteAnimation current = ctx.Animations.Current;
		SpriteAnimation? targetAnimation = ChooseWantedAnimation(in ctx);

		if (targetAnimation != null)
		{
			ctx.Animations.Set(targetAnimation.Value);
		}

		// Update sprite direction.
		NPC.spriteDirection = !current.Is(animAttack) ? NPC.direction : AttackSign;
	}

	private void UpdateEffects(ref Context ctx)
	{
		footsteps.Perform(new(NPC)
		{
			Frame = ctx.Animations.Current.Is(animWalk) ? (ctx.Animations.CurrentFrame, ctx.Animations.PreviousFrame ?? -1, 0, 3) : null,
			StepSound = new SoundStyle($"{PoTMod.ModName}/Assets/Sounds/Footsteps/MonsterStomp", 3)
			{
				Volume = 0.06f,
				Pitch = 0.8f,
				PitchVariance = 0.2f,
				MaxInstances = 8,
			},
		});
	}

	private (Vector2 Center, Rectangle Aabb) GetDamageArea()
	{
		const float RangeExtentX = 24f;
		const float RangeExtentY = 32f;
		const int SizeFull = 56;
		const int SizeExtent = SizeFull / 2;
		const int OriginOffsetX = 12;
		const int OriginOffsetY = 2;

		Vector2 center = NPC.Center + new Vector2(OriginOffsetX * AttackSign, OriginOffsetY) + (AttackDirection * new Vector2(RangeExtentX, RangeExtentY));
		Rectangle aabb = new Rectangle((int)center.X, (int)center.Y, 0, 0).Inflated(SizeExtent, SizeExtent);

		return (center, aabb);
	}

	public override bool PreDraw(SpriteBatch sb, Vector2 screenPos, Color color)
	{
		// Render the NPC using the proper source rectangle.
		Texture2D tex = TextureAssets.Npc[NPC.type].Value;
		Vector2 hitboxCorrection = new(0f, -((tex.Height / (Main.npcFrameCount[NPC.type])) - NPC.height) * 0.5f + 2);
		Vector2 position = NPC.Center + new Vector2(0f, NPC.gfxOffY) + hitboxCorrection - Main.screenPosition;
		Color mulColor = NPC.color.MultiplyRGBA(new Color(Vector4.One * ((byte.MaxValue - NPC.alpha) / (float)byte.MaxValue)));
		Main.EntitySpriteDraw(tex, position, NPC.frame, color.MultiplyRGBA(mulColor), NPC.rotation, NPC.frame.Size() * 0.5f, NPC.scale, NPC.spriteDirection >= 0 ? (SpriteEffects)1 : 0, 0f);

		string slashPath = $"{GetType().FullName}_Glowmask".Replace('.', '/');
		if (ModContent.HasAsset(slashPath) && ModContent.Request<Texture2D>(slashPath) is { IsLoaded: true, Value: { } glowmask })
		{
			Main.EntitySpriteDraw(glowmask, position, NPC.frame, mulColor, NPC.rotation, NPC.frame.Size() * 0.5f, NPC.scale, NPC.spriteDirection >= 0 ? (SpriteEffects)1 : 0, 0f);
		}

#if DEBUG && DEBUG_GIZMOS
		Vector2 targetCenter = NPC.GetGlobalNPC<NPCTargetTracking>().GetTargetCenter(NPC);
		Texture2D skullTexture = TextureAssets.NpcHeadBoss[19].Value;
		sb.Draw(skullTexture, targetCenter - Main.screenPosition, null, Color.IndianRed, 0f, skullTexture.Size() * 0.5f, 1f, 0, 0f);
#endif

		return false;
	}

	public override void PostDraw(SpriteBatch sb, Vector2 screenPos, Color color)
	{
		// Render the slash.
		if (ActiveAction == ActionType.Attack && ActionProgress >= Behavior.AttackDashTick && ActionProgress < Behavior.AttackDamageEndTick
		&& SlashTexture is { IsLoaded: true, Value: { } slashTexture })
		{
			byte frameIndex = (byte)Math.Min(2, Math.Floor((ActionProgress - Behavior.AttackDashTick) / (float)(Behavior.AttackDamageEndTick - Behavior.AttackDashTick) * 3));
			SpriteFrame slashFrame = new SpriteFrame(1, 3).With(0, AttackSign > 0 ? frameIndex : (byte)(2 - frameIndex));
			Color slashColor = Color.Lerp(color, Color.White, 0.33f).MultiplyRGBA(new(Vector4.One * 0.7f));

			Rectangle srcRect = slashFrame.GetSourceRectangle(slashTexture);
			Rectangle dstRect = GetDamageArea().Aabb;
			dstRect.X -= (int)Main.screenPosition.X;
			dstRect.Y -= (int)Main.screenPosition.Y;
			Vector2 origin = srcRect.Size() * 0.5f;

			//DebugUtils.DrawRectangle(sb, dstRect, Color.Red, lineWidth: 2);

			dstRect.X += (int)dstRect.Width / 2;
			dstRect.Y += (int)dstRect.Height / 2;
			sb.Draw(slashTexture, dstRect, srcRect, slashColor, AttackAngle, origin, 0, 0f);
		}
	}
}

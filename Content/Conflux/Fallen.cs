//#define DEBUG_GIZMOS
//#define NEVER_ATTACK

using System.IO;
using System.Runtime.CompilerServices;
using PathOfTerraria.Common.AI;
using PathOfTerraria.Common.NPCs.Components;
using PathOfTerraria.Common.NPCs.Effects;
using PathOfTerraria.Common.Utilities;
using PathOfTerraria.Core.Time;
using PathOfTerraria.Utilities.Xna;
using ReLogic.Content;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;

#nullable enable
#pragma warning disable IDE0053 // Use expression body for lambda expression

namespace PathOfTerraria.Content.Conflux;

// Heavy
internal sealed class FallenTank : Fallen
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
		NPC.defense = 10;
		NPC.damage = 40;
		NPC.width = 36;
		NPC.height = 38;

		Behavior.MaxSpeed = 1.5f;
	}
}

// Medium
internal sealed class FallenRogue : Fallen
{
	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		Main.npcFrameCount[Type] = 6;
	}
	public override void SetDefaults()
	{
		base.SetDefaults();

		NPC.lifeMax = 100;
		NPC.defense = 8;
		NPC.damage = 25;
		NPC.width = 24;
		NPC.height = 40;

		Behavior.MaxSpeed = 3f;
	}
}

// Light
internal sealed class FallenSchemer : Fallen
{
	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		Main.npcFrameCount[Type] = 6;
	}
	public override void SetDefaults()
	{
		base.SetDefaults();

		NPC.lifeMax = 80;
		NPC.defense = 15;
		NPC.damage = 20;
		NPC.width = 20;
		NPC.height = 44;

		Behavior.MaxSpeed = 5.5f;
		Behavior.AttackDashTick = (ushort)(0.2 * 60);
		Behavior.AttackDashSoundTick = (ushort)(Behavior.AttackDashTick - 5);
		Behavior.AttackSlashSoundTick = (ushort)(Behavior.AttackDashTick - 3);
		Behavior.AttackNoGravityEndTick = (ushort)(Behavior.AttackDashTick + (0.25 * 60));
		Behavior.AttackDamageEndTick = (ushort)(Behavior.AttackDashTick + (0.25 * 60));
		Behavior.MaxAttackCooldown = Behavior.MinAttackCooldown = 10;
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
		public float Friction = 8f;
	}

	private const float AttackDistanceX = 100f;
	private const float AttackDistanceY = 250f;

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

	protected AttackInstance? Attack;
	protected Stats Behavior = new();

	public ref float AttackAngle => ref NPC.localAI[0];
	public ref ushort AttackProgress => ref Unsafe.As<float, ushort>(ref Unsafe.AddByteOffset(ref NPC.localAI[2], 0));
	public ref ushort AttackCooldown => ref Unsafe.As<float, ushort>(ref Unsafe.AddByteOffset(ref NPC.localAI[2], 2));
	public int AttackSign => AttackDirection.X >= 0f ? 1 : -1;
	public Vector2 AttackDirection
	{
		get => AttackAngle.ToRotationVector2();
		set => AttackAngle = value.ToRotation();
	}

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

	public override void AI()
	{
		Context ctx = new(NPC);

		ctx.Animations.Advance();
		UpdateTarget(ref ctx);
		Movement(ref ctx);
		Attacking(ref ctx);
		UpdateAnimations(ref ctx);
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
		// Friction.
		if (NPC.velocity.Y == 0f)
		{
			NPC.velocity.X = MathUtils.StepTowards(NPC.velocity.X, 0f, Behavior.Friction * TimeSystem.LogicDeltaTime);
		}

		// Slopes.
		if (NPC.velocity.Y == 0f) { Collision.StepDown(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.stepSpeed, ref NPC.gfxOffY); }

		Collision.StepUp(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.stepSpeed, ref NPC.gfxOffY);

		if (!NPC.HasValidTarget || AttackProgress != 0)
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
		if (navResult.MovementVector.X != 0f)
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
		bool initiateAttack = false;
		NPC.damage = -1;
		NPC.noGravity = false;
		NPC.color = Color.White;

		// Check if we can initiate an attack.
#if !NEVER_ATTACK
		if (AttackProgress == 0 && AttackCooldown == 0 && NPC.velocity.Y == 0f && MathF.Abs(targetDiff.X) <= AttackDistanceX && MathF.Abs(targetDiff.Y) <= AttackDistanceY)
		{
			if (Collision.CanHitLine(NPC.position, NPC.width, NPC.height, ctx.TargetRect.TopLeft(), ctx.TargetRect.Width, ctx.TargetRect.Height))
			{
				initiateAttack = true;
			}
		}
#endif

		// Update attacks.
		if (AttackProgress != 0 || initiateAttack)
		{
			AttackProgress++;

			// Play sounds.
			if (AttackProgress == Behavior.AttackDashSoundTick)
			{
				SoundEngine.PlaySound(position: NPC.Center, style: SoundID.Item71 with { Volume = 0.6f, Pitch = -0.80f, PitchVariance = 0.15f, MaxInstances = 3 });
			}
			else if (AttackProgress == Behavior.AttackSlashSoundTick)
			{
				SoundEngine.PlaySound(position: NPC.Center, style: SoundID.NPCHit56 with { Volume = 0.25f, Pitch = +0.85f, PitchVariance = 0.1f, MaxInstances = 3 });
			}

			if (AttackProgress < Behavior.AttackDashTick)
			{
				// Update Attack direction.
				AttackDirection = targetDiff.SafeNormalize(Vector2.UnitX * AttackSign);

				// Aim starts to update slower and slower.
				const float ChargeFactorForFullAimLag = 0.99f;
				ctx.Tracking.AimLag = Vector2.Lerp(default, Vector2.One * 0.2f, MathF.Min(1f, (AttackProgress / (float)Behavior.AttackDashTick) / ChargeFactorForFullAimLag));

				// Slow-slide.
				NPC.velocity.X = 0.4f * AttackSign;
			}
			else if (AttackProgress == Behavior.AttackDashTick)
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
				NPC.noGravity = AttackProgress >= Behavior.AttackDashTick && AttackProgress < Behavior.AttackNoGravityEndTick;
			}

			// Deal damage starting with the dash.

			if (Attack != null && AttackProgress >= Behavior.AttackDashTick && AttackProgress < Behavior.AttackDamageEndTick)
			{
				(Vector2 hitboxCenter, Rectangle hitboxAabb) = GetDamageArea();
				Attack.Aabb = hitboxAabb;
				Attack.Direction = AttackDirection;
				Attack.DamageEntities();
			}

			// End attack, start cooldown.
			if (AttackProgress >= Behavior.AttackLengthInTicks)
			{
				AttackProgress = 0;
				AttackCooldown = (ushort)Main.rand.Next(Behavior.MinAttackCooldown, Behavior.MaxAttackCooldown);
				ctx.Tracking.AimLag = default;
			}
		}
	}

	private void UpdateAnimations(ref Context ctx)
	{
		const float MinWalkSpeed = 0.5f;

		Vector2 effectiveVelocity = NPC.position - NPC.oldPosition;
		SpriteAnimation current = ctx.Animations.Current;
		SpriteAnimation? targetAnimation = current switch
		{
			_ when AttackProgress != 0 => animAttack,
			_ when MathF.Abs(effectiveVelocity.Y) > 5f => animJump,
			_ when effectiveVelocity.Y == 0f && Math.Abs(effectiveVelocity.X) >= MinWalkSpeed => animWalk with { Speed = effectiveVelocity.X * animWalk.Speed * 4f * NPC.spriteDirection },
			_ when effectiveVelocity.Y == 0f && Math.Abs(effectiveVelocity.X) <= MinWalkSpeed => animIdle,
			_ => null,
		};

		if (targetAnimation != null)
		{
			ctx.Animations.Set(targetAnimation.Value);
		}

		// Update sprite direction.
		NPC.spriteDirection = !current.Is(animAttack) ? NPC.direction : AttackSign;
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
		color = color.MultiplyRGBA(NPC.color);
		Main.EntitySpriteDraw(tex, position, NPC.frame, color, NPC.rotation, NPC.frame.Size() * 0.5f, NPC.scale, NPC.spriteDirection >= 0 ? (SpriteEffects)1 : 0, 0f);

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
		if (AttackProgress >= Behavior.AttackDashTick && AttackProgress < Behavior.AttackDamageEndTick)
		{
			byte frameIndex = (byte)Math.Min(2, Math.Floor((AttackProgress - Behavior.AttackDashTick) / (float)(Behavior.AttackDamageEndTick - Behavior.AttackDashTick) * 3));
			SpriteFrame slashFrame = new SpriteFrame(1, 3).With(0, AttackSign > 0 ? frameIndex : (byte)(2 - frameIndex));
			Texture2D slashTexture = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/Misc/Slash", AssetRequestMode.ImmediateLoad).Value;
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

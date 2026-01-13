//#define DEBUG_GIZMOS
//#define NEVER_ATTACK

using System.IO;
using System.Runtime.CompilerServices;
using PathOfTerraria.Common.AI;
using PathOfTerraria.Common.NPCs.Components;
using PathOfTerraria.Common.NPCs.Effects;
using PathOfTerraria.Common.Utilities;
using PathOfTerraria.Content.Gores;
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

internal sealed class Abominable : ModNPC
{
	private struct Context(NPC npc)
	{
		public Vector2 Center { get; } = npc.Center;
		public NPCAnimations Animations { get; } = npc.GetGlobalNPC<NPCAnimations>();
		public NPCNavigation Navigation { get; } = npc.GetGlobalNPC<NPCNavigation>();
		public NPCTargetTracking Tracking { get; } = npc.GetGlobalNPC<NPCTargetTracking>();

		public Vector2 TargetCenter;
		public Rectangle TargetRect;
	}

	private const float AttackDistanceX = 215f;
	private const float AttackDistanceY = 400f;
	private const ushort AttackLengthInTicks = (ushort)(1.4 * 60);
	private const ushort AttackDashTick = (ushort)(0.66 * 60);
	private const ushort AttackDashSoundTick = AttackDashTick - 5;
	private const ushort AttackNoGravityEndTick = AttackDashTick + (ushort)(0.25 * 60);
	private const ushort AttackDamageEndTick = AttackDashTick + (ushort)(0.25 * 60);
	private const ushort MinAttackCooldown = (ushort)(1.00f * 60);
	private const ushort MaxAttackCooldown = (ushort)(1.60f * 60);

	private static readonly SpriteAnimation animIdle = new() { Id = "idle", Frames = [0] };
	private static readonly SpriteAnimation animJump = new() { Id = "jump", Frames = [1] };
	private static readonly SpriteAnimation animFall = new() { Id = "fall", Frames = [2] };
	private static readonly SpriteAnimation animWalk = new() { Id = "walk", Frames = [3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14], Speed = 5f };
	private static readonly SpriteAnimation animAttack = new() { Id = "attack", Frames = [15, 16, 17, 18, 19, 20, 21, 22, 23, 24], Speed = 9f, Loop = false };

	private AttackInstance? attack;
	private Footsteps footsteps = new();
	private VoiceBehavior voice = new();

	//public ref BitMask<uint> Flags => ref Unsafe.As<float, BitMask<uint>>(ref NPC.localAI[0]);
	public ref float AttackAngle => ref NPC.localAI[0];
	public ref ushort AttackProgress => ref Unsafe.As<float, ushort>(ref Unsafe.AddByteOffset(ref NPC.localAI[2], 0));
	public ref ushort AttackCooldown => ref Unsafe.As<float, ushort>(ref Unsafe.AddByteOffset(ref NPC.localAI[2], 2));
	public int AttackSign => AttackDirection.X >= 0f ? 1 : -1;
	public Vector2 AttackDirection
	{
		get => AttackAngle.ToRotationVector2();
		set => AttackAngle = value.ToRotation();
	}

	public override void Load()
	{
		GoreLoader.AddGoreFromTexture<AdvancedGore>(Mod, $"{Texture}_GoreHead");
		GoreLoader.AddGoreFromTexture<AdvancedGore>(Mod, $"{Texture}_GoreClaw");
		GoreLoader.AddGoreFromTexture<AdvancedGore>(Mod, $"{Texture}_GoreLeg1");
		GoreLoader.AddGoreFromTexture<AdvancedGore>(Mod, $"{Texture}_GoreLeg2");
	}

	public override void SetStaticDefaults()
	{
		NPCID.Sets.TeleportationImmune[Type] = true;
		NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Shimmer] = true;

		NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, new()
		{
			CustomTexturePath = $"{Mod.Name}/Assets/Conflux/{Name}_Bestiary",
		});

		Main.npcFrameCount[Type] = 5;
	}

	public override void SetDefaults()
	{
		NPC.aiStyle = -1;
		NPC.damage = 44;
		NPC.width = 44;
		NPC.height = 80;
		NPC.lifeMax = 750;
		NPC.defense = 35;
		NPC.knockBackResist = 0.00f;

		NPC.HitSound = new($"{nameof(PathOfTerraria)}/Assets/Sounds/HitEffects/FleshHit", 3) { MaxInstances = 5, Volume = 0.4f };
		NPC.DeathSound = new($"{nameof(PathOfTerraria)}/Assets/Sounds/Conflux/AbominableDeath") { Volume = 0.6f, Pitch = -0.05f, PitchVariance = 0.1f };

		NPC.TryEnableComponent<NPCNavigation>(e =>
		{
			e.JumpRange = new(15, 40);
		});
		NPC.TryEnableComponent<NPCAnimations>(e =>
		{
			e.BaseFrame = new SpriteFrame(5, 5) with { PaddingX = 0, PaddingY = 0 };
		});
		NPC.TryEnableComponent<NPCTargetTracking>();

		NPC.TryEnableComponent<NPCHitEffects>(c =>
		{
			c.AddGore(new NPCHitEffects.GoreSpawnParameters($"{PoTMod.ModName}/{nameof(BloodSplatSmall)}", 1));
			c.AddGore(new NPCHitEffects.GoreSpawnParameters($"{PoTMod.ModName}/{nameof(BloodSplatMedium)}", 1));

			c.AddGore(new($"{PoTMod.ModName}/{nameof(BloodSplatSmall)}", 5, NPCHitEffects.OnDeath));
			c.AddGore(new($"{PoTMod.ModName}/{nameof(BloodSplatMedium)}", 5, NPCHitEffects.OnDeath));
			c.AddGore(new($"{PoTMod.ModName}/{nameof(BloodSplatLarge)}", 5, NPCHitEffects.OnDeath));
			c.AddGore(new($"{PoTMod.ModName}/{Name}_GoreHead", 1, NPCHitEffects.OnDeath) { Position = (new(0, 0), new(+00, -32), new(3, 3)) });
			c.AddGore(new($"{PoTMod.ModName}/{Name}_GoreClaw", 1, NPCHitEffects.OnDeath) { Position = (new(0, 0), new(-32, +00), new(3, 3)) });
			c.AddGore(new($"{PoTMod.ModName}/{Name}_GoreClaw", 1, NPCHitEffects.OnDeath) { Position = (new(0, 0), new(+32, +00), new(3, 3)) });
			c.AddGore(new($"{PoTMod.ModName}/{Name}_GoreLeg1", 1, NPCHitEffects.OnDeath) { Position = (new(0, 0), new(+32, -32), new(3, 3)) });
			c.AddGore(new($"{PoTMod.ModName}/{Name}_GoreLeg2", 1, NPCHitEffects.OnDeath) { Position = (new(0, 0), new(+32, +32), new(3, 3)) });
			c.AddGore(new($"{PoTMod.ModName}/{nameof(BloodSplatSmall)}", 5, NPCHitEffects.OnDeath));
			c.AddGore(new($"{PoTMod.ModName}/{nameof(BloodSplatMedium)}", 5, NPCHitEffects.OnDeath));
			c.AddGore(new($"{PoTMod.ModName}/{nameof(BloodSplatLarge)}", 5, NPCHitEffects.OnDeath));
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
		UpdateEffects(ref ctx);
	}

	public override void HitEffect(NPC.HitInfo hit)
	{
		if (Main.dedServ) { return; }

		if (NPC.life <= 0)
		{
			voice.Stop();
			return;
		}

		if (Main.rand.NextBool(5))
		{
			voice.Play(NPC, new($"{nameof(PathOfTerraria)}/Assets/Sounds/Conflux/AbominablePain", 2)
			{
				MaxInstances = 3,
				Volume = 0.5f,
				PitchVariance = 0.1f,
			});
		}
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
		const float MaxSpeed = 4f;
		const float Acceleration = 32f;
		const float Friction = 8f;

		// Push away everyone who's nearby.
		PushBehavior.Update(new PushBehavior.Context(NPC)
		{
			RequiredNpcType = NPC.type,
			Push = (16f, 64f, 2.1f, 0.6f),
		});

		// Friction.
		if (NPC.velocity.Y == 0f)
		{
			NPC.velocity.X = MathUtils.StepTowards(NPC.velocity.X, 0f, Friction * TimeSystem.LogicDeltaTime);
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
			NPC.velocity.X = MathUtils.StepTowards(NPC.velocity.X, MaxSpeed * navResult.MovementVector.X, Acceleration * TimeSystem.LogicDeltaTime);
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

				if (!Main.dedServ)
				{
					voice.Play(NPC, new SoundStyle($"{nameof(PathOfTerraria)}/Assets/Sounds/Conflux/AbominableAttack", 2)
					{
						MaxInstances = 2,
						Pitch = 0.1f,
						Volume = 0.65f,
						PitchVariance = 0.15f,
					});
				}
			}
		}
#endif

		// Update attacks.
		if (AttackProgress != 0 || initiateAttack)
		{
			AttackProgress++;

			// Play slash/dash sound.
			if (AttackProgress == AttackDashSoundTick)
			{
				SoundEngine.PlaySound(position: NPC.Center, style: SoundID.Item71 with { Pitch = -1.00f, PitchVariance = 0.15f });
			}

			if (AttackProgress < AttackDashTick)
			{
				// Update Attack direction.
				AttackDirection = targetDiff.SafeNormalize(Vector2.UnitX * AttackSign);

				// Aim starts to update slower and slower.
				const float ChargeFactorForFullAimLag = 0.99f;
				ctx.Tracking.AimLag = Vector2.Lerp(default, Vector2.One * 0.2f, MathF.Min(1f, (AttackProgress / (float)AttackDashTick) / ChargeFactorForFullAimLag));

				// Slow-slide.
				NPC.velocity.X = 0.4f * AttackSign;
			}
			else if (AttackProgress == AttackDashTick)
			{
				// Dash.
				NPC.velocity = AttackDirection * new Vector2(30f, 12f);

				// Prepare damage.
				attack = new AttackInstance
				{
					Aabb = default,
					Direction = default,
					Damage = NPC.defDamage,
					Knockback = 10f,
					Filter = AttackInstance.EnemyAttackFilterWithInfighting,
					Predicate = e => e is not NPC n || n.type != NPC.type, 
					DeathReason = _ => PlayerDeathReason.ByNPC(NPC.whoAmI),
					ExcludedEntity = NPC,
					HitEntities = attack?.HitEntities,
				};
				attack.HitEntities?.Clear();
			}
			else
			{
				// Slow down.
				NPC.velocity.X *= 0.86f;
				// But also defy gravity for a few ticks.
				NPC.noGravity = AttackProgress >= AttackDashTick && AttackProgress < AttackNoGravityEndTick;
			}

			// Deal damage starting with the dash.

			if (attack != null && AttackProgress >= AttackDashTick && AttackProgress < AttackDamageEndTick)
			{
				(Vector2 hitboxCenter, Rectangle hitboxAabb) = GetDamageArea();
				attack.Aabb = hitboxAabb;
				attack.Direction = AttackDirection;
				attack.DamageEntities();
			}

			// End attack, start cooldown.
			if (AttackProgress >= AttackLengthInTicks)
			{
				AttackProgress = 0;
				AttackCooldown = (ushort)Main.rand.Next(MinAttackCooldown, MaxAttackCooldown);
				ctx.Tracking.AimLag = default;
			}
		}
	}

	private void UpdateEffects(ref Context ctx)
	{
		footsteps.Perform(new(NPC)
		{
			Frame = ctx.Animations.Current.Is(animWalk) ? (ctx.Animations.CurrentFrame, ctx.Animations.PreviousFrame ?? -1, 3, 9) : null,
			StepSound = new SoundStyle($"{PoTMod.ModName}/Assets/Sounds/Footsteps/MonsterStomp", 3)
			{
				Volume = 0.2f,
				PitchVariance = 0.2f,
				MaxInstances = 3,
			},
			AllowScreenShake = true,
		});
	}

	private void UpdateAnimations(ref Context ctx)
	{
		const float MinWalkSpeed = 1.5f;

		Vector2 vel = NPC.position - NPC.oldPosition;
		SpriteAnimation current = ctx.Animations.Current;
		SpriteAnimation? targetAnimation = current switch
		{
			_ when AttackProgress != 0 => animAttack,
			_ when vel.Y < 0f => animJump,
			_ when vel.Y > 0f => animFall,
			_ when vel.Y == 0f && Math.Abs(vel.X) >= MinWalkSpeed => animWalk with { Speed = vel.X * animWalk.Speed * NPC.spriteDirection },
			_ when vel.Y == 0f && Math.Abs(vel.X) <= MinWalkSpeed => animIdle,
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
		const float RangeExtentX = 48f;
		const float RangeExtentY = 32f;
		const int SizeFull = 112;
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
		Vector2 hitboxCorrection = new(0f, -(100 - NPC.height) * 0.5f);
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
		if (AttackProgress >= AttackDashTick && AttackProgress < AttackDamageEndTick)
		{
			string basePath = $"{GetType().FullName}_Slash".Replace('.', '/');
			basePath = ModContent.HasAsset(basePath) ? basePath : $"{PoTMod.ModName}/Assets/Misc/Slash";
			Texture2D texture = ModContent.Request<Texture2D>(basePath, AssetRequestMode.ImmediateLoad).Value;

			// First column is the diffuse, second is the glowmask.
			var baseFrame = new SpriteFrame(2, (byte)(texture.Height / (texture.Width / 2))) { PaddingX = 0, PaddingY = 0 };
			byte frameIndex = (byte)Math.Min((baseFrame.RowCount - 1), Math.Floor((AttackProgress - AttackDashTick) / (float)(AttackDamageEndTick - AttackDashTick) * baseFrame.RowCount));
			baseFrame = baseFrame.With(0, AttackSign > 0 ? frameIndex : (byte)((baseFrame.RowCount - 1) - frameIndex));

			Vector2 center = GetDamageArea().Center;
			Rectangle srcRect = baseFrame.GetSourceRectangle(texture);
			Rectangle dstRect = new((int)(center.X), (int)(center.Y), srcRect.Width, srcRect.Height);
			dstRect.X -= (int)Main.screenPosition.X;
			dstRect.Y -= (int)Main.screenPosition.Y;
			Vector2 origin = srcRect.Size() * 0.5f;

			sb.Draw(texture, dstRect, srcRect, color, AttackAngle, origin, 0, 0f);
			sb.Draw(texture, dstRect, srcRect with { X = srcRect.Width }, Color.White, AttackAngle, origin, 0, 0f);
		}

#if DEBUG && DEBUG_GIZMOS
		Main.DebugDrawer.Begin(Main.GameViewMatrix.TransformationMatrix);
		Main.DebugDrawer.DrawLine(NPC.Center - Main.screenPosition, (NPC.Center + AttackDirection * 64f) - Main.screenPosition, 2f, Color.Red);
		Main.DebugDrawer.End();
#endif
	}
}

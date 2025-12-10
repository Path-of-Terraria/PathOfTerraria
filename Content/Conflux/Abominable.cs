using System.Runtime.CompilerServices;
using PathOfTerraria.Common.AI;
using PathOfTerraria.Common.NPCs.Components;
using PathOfTerraria.Common.NPCs.Effects;
using PathOfTerraria.Utilities.Xna;
using ReLogic.Content;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;

#nullable enable
#pragma warning disable IDE0053 // Use expression body for lambda expression

namespace PathOfTerraria.Content.Conflux;

internal sealed class Abominable : ModNPC
{
	private readonly struct Context(NPC npc)
	{
		public Vector2 Center { get; } = npc.Center;
		public NPCAnimations Animations { get; } = npc.GetGlobalNPC<NPCAnimations>();
		public NPCTargetTracking Tracking { get; } = npc.GetGlobalNPC<NPCTargetTracking>();
	}

	private const float AttackDistanceX = 215f;
	private const float AttackDistanceY = 400f;
	private const ushort AttackLengthInTicks = (ushort)(1.4 * 60);
	private const ushort AttackDashTick = (ushort)(0.66 * 60);
	private const ushort AttackDashSoundTick = AttackDashTick - 5;
	private const ushort AttackNoGravityEndTick = AttackDashTick + (ushort)(0.25 * 60);
	private const ushort AttackDamageEndTick = AttackDashTick + (ushort)(0.25 * 60);
	private const ushort MinAttackCooldown = (ushort)(1.25f * 60);
	private const ushort MaxAttackCooldown = (ushort)(1.75f * 60);

	private static readonly SpriteAnimation animWalk = new()
	{
		Id = "walk",
		Frames = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11],
		Speed = 5f,
	};
	private static readonly SpriteAnimation animJump = new()
	{
		Id = "jump",
		Frames = [14, 15],
		Speed = 4f,
		Loop = true,
	};
	private static readonly SpriteAnimation animAttack = new()
	{
		Id = "attack",
		Frames = [12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22],
		Speed = 9f,
		Loop = false,
	};

	private AttackInstance? attack;
	private Vector2 oldVelocity;
	private uint lastFootstepTime;

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
		NPC.aiStyle = NPCAIStyleID.Fighter;
		NPC.damage = 44;
		NPC.width = 66;
		NPC.height = 80;
		NPC.lifeMax = 250;
		NPC.defense = 35;
		NPC.HitSound = SoundID.NPCHit56 with { Pitch = -0.37f, PitchVariance = 0.11f, Identifier = "AbominableHit" };
		NPC.DeathSound = SoundID.NPCDeath23 with { Pitch = -0.35f, PitchVariance = 0.15f, Identifier = "AbominableDeath" };
		NPC.knockBackResist = 0.00f;

		NPC.TryEnableComponent<NPCAnimations>(e =>
		{
			e.BaseFrame = new(5, 5);
		});
		NPC.TryEnableComponent<NPCTargetTracking>();

		NPC.TryEnableComponent<NPCHitEffects>(c =>
		{
			c.AddGore(new NPCHitEffects.GoreSpawnParameters(132, 10, NPCHitEffects.OnDeath));

			c.AddDust(new NPCHitEffects.DustSpawnParameters(DustID.Blood, 30));
			c.AddDust(new NPCHitEffects.DustSpawnParameters(DustID.Blood, 200, NPCHitEffects.OnDeath));
		});
	}

	public override bool PreAI()
	{
		return true;
	}

	public override void AI()
	{
		Context ctx = new(NPC);

		ctx.Animations.Advance();
		Behavior(in ctx);
		UpdateAnimations(in ctx);
		UpdateEffects(in ctx);
	}

	public override void PostAI()
	{
		Context ctx = new(NPC);

		_ = ctx;
	}

	private void Behavior(in Context ctx)
	{
		// Tick down cooldowns.
		if (AttackCooldown > 0) { AttackCooldown--; }

		// Reset mutated variables.
		bool initiateAttack = false;
		NPC.damage = -1;
		NPC.noGravity = false;
		NPC.aiStyle = NPCAIStyleID.Fighter;
		NPC.color = Color.White;

		// Track target.
		Vector2 targetCenter = ctx.Tracking.GetTargetCenter(NPC);
		Vector2 targetDiff = NPC.HasValidTarget ? new Vector2(targetCenter.X - ctx.Center.X, targetCenter.Y - ctx.Center.Y) : new(float.PositiveInfinity);
		Vector2 targetDirection = targetDiff.SafeNormalize(Vector2.UnitX * AttackSign);
		float targetDistance = NPC.HasValidTarget ? (targetDiff != default ? targetDiff.Length() : 0f) : float.PositiveInfinity;

		// Check if we can initiate an attack.
		if (AttackProgress == 0 && AttackCooldown == 0 && NPC.velocity.Y == 0f && MathF.Abs(targetDiff.X) <= AttackDistanceX && MathF.Abs(targetDiff.Y) <= AttackDistanceY)
		{
			initiateAttack = true;

			if (!Main.dedServ)
			{
				SoundEngine.PlaySound(position: NPC.Center, style: SoundID.NPCHit56 with { Pitch = +0.15f, PitchVariance = 0.1f, Identifier = "AbominableCharge" });
			}
		}

		// Update attacks.
		if (AttackProgress != 0 || initiateAttack)
		{
			AttackProgress++;

			// Use no base AI during attacks.
			NPC.aiStyle = -1;

			// Play slash/dash sound.
			if (AttackProgress == AttackDashSoundTick)
			{
				SoundEngine.PlaySound(position: NPC.Center, style: SoundID.Item71 with { Pitch = -1.00f, PitchVariance = 0.15f });
			}

			if (AttackProgress < AttackDashTick)
			{
				// Update Attack direction.
				AttackDirection = targetDirection;

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
				int whoAmI = NPC.whoAmI;
				attack = new AttackInstance
				{
					Aabb = default,
					Direction = default,
					Damage = NPC.defDamage,
					Knockback = 10f,
					Filter = AttackInstance.EntityKind.Player | AttackInstance.EntityKind.Neutral | AttackInstance.EntityKind.Friendly | AttackInstance.EntityKind.Enemy,
					Predicate = e => e is not Terraria.NPC || e.whoAmI != whoAmI,
					DeathReason = _ => PlayerDeathReason.ByNPC(whoAmI),
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

			if (Main.netMode != NetmodeID.MultiplayerClient && attack != null && AttackProgress >= AttackDashTick && AttackProgress < AttackDamageEndTick)
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

	private void UpdateEffects(in Context ctx)
	{
		int frameNew = ctx.Animations.CurrentFrame;
		int frameOld = ctx.Animations.PreviousFrame ?? -1;
		uint tickTime = Main.GameUpdateCount;

		// Footsteps.

		if (!Main.dedServ)
		{
			bool isOnGround = NPC.velocity.Y == 0f;
			bool wasOnGround = oldVelocity.Y == 0f;
			bool jumped = !isOnGround && wasOnGround;
			bool landed = isOnGround && !wasOnGround;

			if ((tickTime - lastFootstepTime) >= 10 && (landed || jumped || (isOnGround && ctx.Animations.Current.Is(animWalk) && (frameNew == 3 && frameOld != 3) || (frameNew == 9 && frameOld != 9))))
			{
				lastFootstepTime = tickTime;
				SoundEngine.PlaySound(position: NPC.Bottom, style: new SoundStyle($"{PoTMod.ModName}/Assets/Sounds/Footsteps/MonsterStomp", 3)
				{
					Volume = 0.2f,
					PitchVariance = 0.2f,
					MaxInstances = 3,
				});

				Main.instance.CameraModifiers.Add(new PunchCameraModifier(NPC.Center, new Vector2(0f, -1f), 1f, 3f, 15, 700f, "Abominable"));
			}
		}

		oldVelocity = NPC.velocity;
	}

	private void UpdateAnimations(in Context ctx)
	{
		SpriteAnimation current = ctx.Animations.Current;
		SpriteAnimation? targetAnimation = current switch
		{
			_ when AttackProgress != 0 => animAttack,
			_ when MathF.Abs(NPC.velocity.Y) > 5f => animJump,
			_ when NPC.velocity.Y == 0f => animWalk,
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
		Texture2D tex = TextureAssets.Npc[NPC.type].Value;
		Vector2 hitboxCorrection = new(0f, -(100 - NPC.height) * 0.5f);
		Vector2 position = NPC.Center + new Vector2(0f, NPC.gfxOffY) + hitboxCorrection - Main.screenPosition;

		color = color.MultiplyRGBA(NPC.color);

		sb.Draw(tex, position, NPC.frame, color, NPC.rotation, NPC.frame.Size() * 0.5f, NPC.scale, NPC.spriteDirection >= 0 ? (SpriteEffects)1 : 0, 0f);

#if DEBUG && false
		Vector2 targetCenter = NPC.GetGlobalNPC<NPCTargetTracking>().GetTargetCenter(NPC);
		Texture2D skullTexture = TextureAssets.NpcHeadBoss[19].Value;
		sb.Draw(skullTexture, targetCenter - Main.screenPosition, null, Color.IndianRed, 0f, skullTexture.Size() * 0.5f, 1f, 0, 0f);
#endif

		return false;
	}

	public override void PostDraw(SpriteBatch sb, Vector2 screenPos, Color color)
	{
		if (AttackProgress >= AttackDashTick && AttackProgress < AttackDamageEndTick)
		{
			byte frameIndex = (byte)Math.Min(2, Math.Floor((AttackProgress - AttackDashTick) / (float)(AttackDamageEndTick - AttackDashTick) * 3));
			SpriteFrame slashFrame = new SpriteFrame(1, 3).With(0, AttackSign > 0 ? frameIndex : (byte)(2 - frameIndex));
			Texture2D slashTexture = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/Misc/Slash", AssetRequestMode.ImmediateLoad).Value;
			Color slashColor = Color.Lerp(color, Color.White, 0.33f).MultiplyRGBA(new(Vector4.One * 0.7f));
			
			Rectangle srcRect = slashFrame.GetSourceRectangle(slashTexture);
			Rectangle dstRect = GetDamageArea().Aabb;
			dstRect.X -= (int)Main.screenPosition.X;
			dstRect.Y -= (int)Main.screenPosition.Y;
			Vector2 origin = srcRect.Size() * 0.5f;

#if DEBUG && false
			Main.DebugDrawer.Begin(Main.GameViewMatrix.TransformationMatrix);
			Main.DebugDrawer.DrawLine(new(dstRect.Left, dstRect.Top), new(dstRect.Right, dstRect.Top), 2f, Color.Red);
			Main.DebugDrawer.DrawLine(new(dstRect.Right, dstRect.Top), new(dstRect.Right, dstRect.Bottom), 2f, Color.Red);
			Main.DebugDrawer.DrawLine(new(dstRect.Right, dstRect.Bottom), new(dstRect.Left, dstRect.Bottom), 2f, Color.Red);
			Main.DebugDrawer.DrawLine(new(dstRect.Left, dstRect.Bottom), new(dstRect.Left, dstRect.Top), 2f, Color.Red);
			Main.DebugDrawer.End();
#endif

			dstRect.X += (int)srcRect.Width;
			dstRect.Y += (int)srcRect.Height;
			sb.Draw(slashTexture, dstRect, srcRect, slashColor, AttackAngle, origin, 0, 0f);
		}

#if DEBUG && false
		Main.DebugDrawer.Begin();
		Main.DebugDrawer.DrawLine(NPC.Center - Main.screenPosition, (NPC.Center + AttackDirection * 64f) - Main.screenPosition, 2f, Color.Red);
		Main.DebugDrawer.End();
#endif
	}
}

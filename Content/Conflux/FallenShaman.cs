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
using PathOfTerraria.Utilities.Xna;
using ReLogic.Content;
using ReLogic.Utilities;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;

#nullable enable

namespace PathOfTerraria.Content.Conflux;

internal sealed class FallenShamanRocket : ModProjectile
{
	private static Asset<Texture2D>? glowmask;

	public override void SetStaticDefaults()
	{
		Main.projFrames[Type] = 2;
	}
	public override void SetDefaults()
	{
		Projectile.damage = 1;
		Projectile.Size = new(16);
		Projectile.timeLeft = 300;
		Projectile.hostile = true;
		Projectile.friendly = false;
		Projectile.aiStyle = -1;
		Projectile.extraUpdates = 1;
	}

	public override void AI()
	{
		if (Projectile.velocity == default)
		{
			Projectile.velocity = Vector2.UnitX * 5f;
		}

		Vector2 center = Projectile.Center;
		(Player? closestPlayer, float closestSqrDst) = (null, float.PositiveInfinity);
		foreach (Player player in Main.ActivePlayers)
		{
			if (player.DistanceSQ(center) is float sqrDst && sqrDst < closestSqrDst)
			{
				(closestPlayer, closestSqrDst) = (player, sqrDst);
			}
		}

		if (closestPlayer != null)
		{
			float currAngle = Projectile.velocity.ToRotation();
			float wishAngle = center.AngleTo(closestPlayer.Center);
			Vector2 currDirection = currAngle.ToRotationVector2();
			Vector2 wishDirection = wishAngle.ToRotationVector2();
			bool canTurn = Vector2.Dot(wishDirection, currDirection) > -0.1f;
			wishDirection = canTurn ? wishDirection : currDirection;
			float wishSpeed = MathF.Min(10f, (3600 - Projectile.timeLeft) * 0.15f);
			float acceleration = 3f * TimeSystem.LogicDeltaTime;
			Projectile.velocity = MovementUtils.DirAccelQ(Projectile.velocity, wishDirection, wishSpeed, acceleration);
		}

		Projectile.frame = (int)MathF.Floor(((float)Main.timeForVisualEffects + (Projectile.identity * 100f)) / 5f) % 2;
		Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.ToRadians(180f + (Projectile.frame == 0 ? 59f : 39f));
		Lighting.AddLight(Projectile.Center, Color.IndianRed.ToVector3());
	}

	public override void OnHitPlayer(Player target, Player.HurtInfo info)
	{
		Projectile.Kill();
	}
	public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
	{
		Projectile.Kill();
	}

	public override void OnKill(int timeLeft)
	{
		if (!Main.dedServ)
		{
			var sound = new SoundStyle($"{nameof(PathOfTerraria)}/Assets/Sounds/HitEffects/FrostMagic")
			{
				Volume = 0.175f,
				MaxInstances = 5,
				Pitch = +0.50f,
				PitchVariance = 0.4f,
			};
			SoundEngine.PlaySound(in sound, Projectile.Center);

			for (int i = 0; i < 10; i++) { Dust.NewDustPerfect(Projectile.Center, DustID.SomethingRed, Main.rand.NextVector2Circular(10f, 10f)); }
		}
	}

	public override bool PreDraw(ref Color lightColor)
	{
		Texture2D texture = TextureAssets.Projectile[Type].Value;
		SpriteFrame spriteFrame = new(1, (byte)Main.projFrames[Type]) { PaddingX = 0, PaddingY = 0 };
		Rectangle frame = spriteFrame.With(0, (byte)Projectile.frame).GetSourceRectangle(texture);
		Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, Color.White, Projectile.rotation, frame.Size() * 0.5f, Projectile.scale, 0, 0f);

		if ((glowmask ??= ModContent.Request<Texture2D>($"{Texture}_Glowmask")) is { IsLoaded: true, Value: { } glowTexture })
		{
			Main.EntitySpriteDraw(glowTexture, Projectile.Center - Main.screenPosition, frame, Color.White, Projectile.rotation, frame.Size() * 0.5f, Projectile.scale, 0, 0f);
		}

		return false;
	}
}

/// <summary> Necromancer miniboss that supports its army of demons. </summary>
internal sealed class FallenShaman : ModNPC
{
	public enum Flag : byte
	{
		Resurrecting = 1 << 0,
	}
	private readonly struct Context(NPC npc)
	{
		public Vector2 Center { get; } = npc.Center;
		public NPCMovement Movement { get; } = npc.GetGlobalNPC<NPCMovement>();
		public NPCNavigation Navigation { get; } = npc.GetGlobalNPC<NPCNavigation>();
		public NPCTargeting Targeting { get; } = npc.GetGlobalNPC<NPCTargeting>();
		public NPCAttacking Attacking { get; } = npc.GetGlobalNPC<NPCAttacking>();
		public NPCAnimations Animations { get; } = npc.GetGlobalNPC<NPCAnimations>();
		public NPCTeleports Teleports { get; } = npc.GetGlobalNPC<NPCTeleports>();
	}

	private static readonly SpriteAnimation animIdle = new() { Id = "idle", Frames = [23, 24, 25, 26, 27, 28, 29, 30], Speed = 3f };
	private static readonly SpriteAnimation animJump = new() { Id = "jump", Frames = [5] };
	private static readonly SpriteAnimation animFall = new() { Id = "fall", Frames = [1] };
	private static readonly SpriteAnimation animWalk = new() { Id = "walk", Frames = [0, 1, 2, 3, 4, 5] };
	private static readonly SpriteAnimation animAttack = new() { Id = "attack", Frames = [6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22], Speed = 17f, Loop = false };
	private static readonly SpriteAnimation animVanish = new() { Id = "vanish", Frames = [7, 8, 9, 10, 11], Speed = 20f, Loop = false };
	private static readonly SpriteAnimation animAppear = new() { Id = "appear", Frames = [11, 10, 9, 8, 7], Speed = 20f, Loop = false };

	// public ref float FlightCounter => ref NPC.ai[3];
	public ref Flag Flags => ref Unsafe.As<float, Flag>(ref NPC.ai[3]);

	private float flightVolume;
	private SlotId flightSound;

	public override void Load()
	{
		GoreLoader.AddGoreFromTexture<AdvancedGore>(Mod, $"{Texture}_GoreHead");
		GoreLoader.AddGoreFromTexture<AdvancedGore>(Mod, $"{Texture}_GoreChest");
		GoreLoader.AddGoreFromTexture<AdvancedGore>(Mod, $"{Texture}_GoreLeg");
		GoreLoader.AddGoreFromTexture<SimpleModGore>(Mod, $"{Texture}_GoreCloth1");
		GoreLoader.AddGoreFromTexture<SimpleModGore>(Mod, $"{Texture}_GoreCloth2");
		GoreLoader.AddGoreFromTexture<SimpleModGore>(Mod, $"{Texture}_GoreStaff1");
		GoreLoader.AddGoreFromTexture<SimpleModGore>(Mod, $"{Texture}_GoreStaff2");
		GoreLoader.AddGoreFromTexture<SimpleModGore>(Mod, $"{Texture}_GoreStaff3");
	}

	public override void SetStaticDefaults()
	{
		NPCID.Sets.UsesNewTargetting[Type] = true;
		NPCID.Sets.TeleportationImmune[Type] = true;
		NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Shimmer] = true;
		Main.npcFrameCount[Type] = 4;
	}
	public override void SetDefaults()
	{
		NPC.aiStyle = -1;
		NPC.lifeMax = 1000;
		NPC.defense = 35;
		NPC.damage = 50;
		NPC.width = 20;
		NPC.height = 44;
		NPC.knockBackResist = 0.0f;
		NPC.lavaImmune = true;

		NPC.HitSound = new($"{nameof(PathOfTerraria)}/Assets/Sounds/HitEffects/FleshHit", 3) { Volume = 0.4f, Pitch = -0.5f, MaxInstances = 5 };
		NPC.DeathSound = SoundID.NPCDeath23 with { Pitch = -0.85f, PitchVariance = 0.15f, Identifier = "FallenShamanDeath" };

		NPC.TryEnableComponent<NPCNavigation>(e =>
		{
			e.JumpRange = new(8, 20);
		});
		NPC.TryEnableComponent<NPCMovement>(e =>
		{
			e.Data.MaxSpeed = 3.5f;
			e.Data.Acceleration = 16f;
			e.Data.Friction = (8f, 2f);
			// e.Data.Push = new() { RequiredNpcType = Type };
		});
		NPC.TryEnableComponent<NPCAnimations>(e =>
		{
			e.BaseFrame = new SpriteFrame(8, 4) with { PaddingX = 0, PaddingY = 0 };
			e.SpriteOffset = new(0f, -31f);
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
				Volume = 0.06f,
				Pitch = 0.8f,
				PitchVariance = 0.2f,
				MaxInstances = 8,
			};
		});
		NPC.TryEnableComponent<NPCVoice>(e =>
		{
			e.Data.PainSound = (3, SoundID.NPCHit56 with { Pitch = 0f, PitchVariance = 0.5f, Identifier = "ShamanHit" });
		});
		NPC.TryEnableComponent<NPCHitEffects>(c =>
		{
			c.AddGore(new($"{PoTMod.ModName}/{nameof(BloodSplatMedium)}", 1));

			c.AddGore(new($"{PoTMod.ModName}/{nameof(BloodSplatSmall)}", 3, NPCHitEffects.OnDeath));
			c.AddGore(new($"{PoTMod.ModName}/{nameof(BloodSplatMedium)}", 2, NPCHitEffects.OnDeath));
			c.AddGore(new($"{PoTMod.ModName}/{nameof(BloodSplatLarge)}", 1, NPCHitEffects.OnDeath));
			c.AddGore(new($"{PoTMod.ModName}/{Name}_GoreHead", +1, NPCHitEffects.OnDeath) { Position = (new(0, 0), new(+00, -16), new(3, 3)) });
			c.AddGore(new($"{PoTMod.ModName}/{Name}_GoreChest", 1, NPCHitEffects.OnDeath) { Position = (new(0, 0), new(+00, +00), new(3, 3)) });
			c.AddGore(new($"{PoTMod.ModName}/{Name}_GoreLeg", +1, NPCHitEffects.OnDeath) { Position = (new(0, 0), new(+16, -16), new(3, 3)) });
			c.AddGore(new($"{PoTMod.ModName}/{Name}_GoreLeg", +1, NPCHitEffects.OnDeath) { Position = (new(0, 0), new(+16, +16), new(3, 3)) });
			c.AddGore(new($"{PoTMod.ModName}/{Name}_GoreStaff1", +1, NPCHitEffects.OnDeath) { Position = (new(0, 0), new(-16, -32), new(3, 3)) });
			c.AddGore(new($"{PoTMod.ModName}/{Name}_GoreStaff2", +1, NPCHitEffects.OnDeath) { Position = (new(0, 0), new(+00, -16), new(3, 3)) });
			c.AddGore(new($"{PoTMod.ModName}/{Name}_GoreStaff3", +1, NPCHitEffects.OnDeath) { Position = (new(0, 0), new(+16, +00), new(3, 3)) });
			c.AddGore(new($"{PoTMod.ModName}/{Name}_GoreCloth1", +1, NPCHitEffects.OnDeath) { Position = (new(0, 0), new(+16, +00), new(3, 3)) });
			c.AddGore(new($"{PoTMod.ModName}/{Name}_GoreCloth2", +1, NPCHitEffects.OnDeath) { Position = (new(0, 0), new(+16, +00), new(3, 3)) });
			c.AddGore(new($"{PoTMod.ModName}/{nameof(BloodSplatSmall)}", 3, NPCHitEffects.OnDeath));
			c.AddGore(new($"{PoTMod.ModName}/{nameof(BloodSplatMedium)}", 2, NPCHitEffects.OnDeath));
			c.AddGore(new($"{PoTMod.ModName}/{nameof(BloodSplatLarge)}", 1, NPCHitEffects.OnDeath));
		});
	}

	public override void AI()
	{
		Context ctx = new(NPC);

		// Invoke behaviors.
		CustomBehavior(in ctx);
		ctx.Animations.Advance();
		ctx.Targeting.ManualUpdate(new(NPC));
		ctx.Teleports.ManualUpdate(new(NPC));
		ctx.Attacking.ManualUpdate(new(NPC));
		ctx.Movement.ManualUpdate(new(NPC));
		ctx.Animations.Set(PickAnimation(in ctx));
	}

	private void CustomBehavior(in Context ctx)
	{
		bool atkActive = ctx.Attacking.Active;
		int atkProg = ctx.Attacking.Data.Progress;
		int atkBase = ctx.Attacking.Data.Dash.Start;
		(FallenSoul? soul, float soulSqrDst) = (null, float.PositiveInfinity);

		// Reset overrides.
		ctx.Movement.Data.TargetOverride = null;
		ctx.Attacking.Data.ManualInitiation = false;

		// Look for a soul to potentially respawn.
		const float seekRangeSqr = 600f * 600f;
		const float triggerRangeSqr = 60f * 60f;
		const float respawnRangeSqr = 350f * 350f;
		float resurrectRangeSqr = (atkActive && Flags.HasFlag(Flag.Resurrecting)) ? respawnRangeSqr : triggerRangeSqr;
		if (!atkActive || Flags.HasFlag(Flag.Resurrecting))
		{
			foreach (Projectile proj in Main.ActiveProjectiles)
			{
				if (proj.ModProjectile is not FallenSoul thisSoul) { continue; }
				float sqrDst = proj.DistanceSQ(ctx.Center);
				if (sqrDst > seekRangeSqr) { continue; }

				if (sqrDst < soulSqrDst)
				{
					(soul, soulSqrDst) = (thisSoul, sqrDst);
				}
			}

			if (soul != null && !atkActive)
			{
				// Navigate towards the soul.
				ctx.Movement.Data.TargetOverride = soul.Projectile.Center;
				ctx.Attacking.Data.ManualInitiation = true;
			}
		}

		// Decide whether the next casting animation will be an attack or a resurrection.
		if (!atkActive
		&& (Flags.HasFlag(Flag.Resurrecting) is bool alreadyIs)
		&& ((soul != null && soulSqrDst <= resurrectRangeSqr) is bool shouldBe)
		&& shouldBe != alreadyIs)
		{
			Flags = shouldBe ? (Flags | Flag.Resurrecting) : (Flags & ~Flag.Resurrecting);
			NPC.netUpdate = true;
		}

		// Start resurrections if close.
		if (!atkActive && Flags.HasFlag(Flag.Resurrecting))
		{
			ctx.Attacking.TryStarting(new(NPC));
		}

		// Resurrection.
		if (soul != null && atkActive && atkProg <= atkBase)
		{
			Vector2 projCenter = soul.Projectile.Center;

			// Spawn effects.
			for (int i = 0; i < ((atkProg >= atkBase - 30) ? (atkProg == atkBase ? 10 : 1) : 0); i++)
			{
				Vector2 basePos = projCenter - new Vector2(16f, 16f);
				Vector2 gorePos = basePos + Main.rand.NextVector2Circular(32f, 32f);
				Vector2 goreVel = gorePos.DirectionTo(basePos) * 5f;
				Gore.NewGore(null, gorePos, goreVel, ModContent.GoreType<BloodSplatMedium>());
			}
			if (atkProg == 1)
			{
				SoundEngine.PlaySound(new($"{nameof(PathOfTerraria)}/Assets/Sounds/Conflux/FallenShamanResurrect", 2) {
					MaxInstances = 2,
					Volume = 0.5f,
					PitchVariance = 0.15f,
				}, ctx.Center);
			}
			if (atkProg == atkBase - 15)
			{
				SoundEngine.PlaySound(new($"{nameof(PathOfTerraria)}/Assets/Sounds/Enemies/ResurrectBloody", 3)
				{
					Volume = 0.75f,
					MaxInstances = 3,
				}, projCenter);
			}

			// Respawn.
			if (atkProg == atkBase && soul.Respawn())
			{
				// Reset the flight cooldown because we did something good (evil).
				// FlightCounter = 0;
				// NPC.netUpdate = true;
			}
		}

		// Shoot projectiles during non-respawn attacks.
		for (int i = 0; i < (atkActive && !Flags.HasFlag(Flag.Resurrecting) ? 6 : 0); i++)
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
			// Self-knockback.
			NPC.velocity = MovementUtils.DirAccel(NPC.velocity, -targetCenter.AngleFrom(NPC.Center).ToRotationVector2(), 3f, 1f);

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

		// Handle flight.
		const int flightAI = NPCAIStyleID.Bat;
		bool isFlying = soul == null; // && (((int)FlightCounter / 900) % 2 == 0);
		NPC.aiStyle = isFlying ? flightAI : -1;
		NPC.noGravity = isFlying;
		// FlightCounter = isFlying ? (FlightCounter + 1) : FlightCounter;

		// Effects.
		if (!Main.dedServ)
		{
			Lighting.AddLight(NPC.Top, Color.Red.ToVector3() * 0.5f);

			if (isFlying)
			{
				Dust.NewDustDirect(NPC.BottomLeft + new Vector2(0f, -8f), NPC.width, 8, DustID.SomethingRed, NPC.velocity.X * 2f, NPC.velocity.Y * 2f, Scale: 1f);
			}

			var loopSound = new SoundStyle($"{nameof(PathOfTerraria)}/Assets/Sounds/Conflux/FallenShamanFlight")
			{
				Volume = 0.1f,
				IsLooped = true,
				PauseBehavior = PauseBehavior.PauseWithGame,
			};
			float speed = NPC.velocity.Length();
			float halfStep = (isFlying ? 1f : 1f) * TimeSystem.LogicDeltaTime;
			float target = isFlying ? MathUtils.Clamp01(0.25f + NPC.velocity.Length() / 4f) : 0f;
			float volume = flightVolume = MathUtils.StepTowards(MathHelper.Lerp(flightVolume, target, halfStep), target, halfStep);
			float pitch = Math.Clamp(-0.5f + (speed * 0.1f), -0.5f, 0.2f);
			SoundUtils.UpdateLoopingSound(ref flightSound, ctx.Center, volume, pitch, loopSound,_ => Main.npc[NPC.whoAmI] is { active: true } n && n == NPC);
		}
	}

	private SpriteAnimation? PickAnimation(in Context ctx)
	{
		const float MinWalkSpeed = 0.5f;

		Vector2 vel = NPC.position - NPC.oldPosition;

		return ctx.Animations.Current switch
		{
			_ when ctx.Attacking.Active => animAttack with { Speed = 7f },
			_ when ctx.Teleports.Active => ctx.Teleports.Data.Progress <= ctx.Teleports.Data.Reappear.Start ? animVanish with { Speed = 6f } : animAppear,
			_ when vel.Y < 0f => animJump,
			_ when vel.Y > 0f => animFall,
			_ when vel.Y == 0f && Math.Abs(vel.X) >= MinWalkSpeed => animWalk with { Speed = vel.X * animWalk.Speed * 4f * NPC.spriteDirection },
			_ when vel.Y == 0f && Math.Abs(vel.X) <= MinWalkSpeed => animIdle with { Speed = 6f },
			_ => null,
		};
	}
}


using System.Diagnostics;
using PathOfTerraria.Common.AI;
using PathOfTerraria.Common.NPCs.Components;
using PathOfTerraria.Common.NPCs.Effects;
using PathOfTerraria.Common.Utilities;
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

internal sealed class CryoStalkerIcicle : ModProjectile
{
	private static Asset<Texture2D>? glowmask;

	public ref float Activation => ref Projectile.ai[0];

	public override void SetStaticDefaults()
	{
		Main.projFrames[Type] = 2;
	}
	public override void SetDefaults()
	{
		Projectile.damage = 1;
		Projectile.Size = new(16);
		//Projectile.timeLeft = 300;
		Projectile.hostile = true;
		Projectile.friendly = false;
		Projectile.aiStyle = -1;
		Projectile.frame = Main.rand.Next(2);
		Projectile.extraUpdates = 1;
	}

	public override void AI()
	{
		if (Projectile.velocity == default)
		{
			Projectile.velocity = Vector2.UnitX * 5f;
		}

		Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.ToRadians(180f + (Projectile.frame == 0 ? 59f : 39f));
		Lighting.AddLight(Projectile.Center, ColorUtils.FromHexRgb(0x7ce8ff).ToVector3());
	}

	public override void OnKill(int timeLeft)
	{
		if (!Main.dedServ)
		{
			var sound = new SoundStyle($"{nameof(PathOfTerraria)}/Assets/Sounds/HitEffects/FrostMagic")
			{
				Volume = 0.15f,
				MaxInstances = 5,
				Pitch = 0.15f,
				PitchVariance = 0.2f,
			};
			SoundEngine.PlaySound(in sound, Projectile.Center);

			for (int i = 0; i < 10; i++) { Dust.NewDustPerfect(Projectile.Center, DustID.Ice, Main.rand.NextVector2Circular(10f, 10f)); }
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

internal sealed class CryoStalker : ModNPC
{
	private readonly struct Context(NPC npc)
	{
		public Vector2 Center { get; } = npc.Center;
		public NPCMovement Movement { get; } = npc.GetGlobalNPC<NPCMovement>();
		//public NPCNavigation Navigation { get; } = npc.GetGlobalNPC<NPCNavigation>();
		public NPCTargeting Targeting { get; } = npc.GetGlobalNPC<NPCTargeting>();
		public NPCAttacking Attacking { get; } = npc.GetGlobalNPC<NPCAttacking>();
		public NPCAnimations Animations { get; } = npc.GetGlobalNPC<NPCAnimations>();
		public NPCTeleports Teleports { get; } = npc.GetGlobalNPC<NPCTeleports>();
	}

	private static readonly SpriteAnimation animIdle = new() { Id = "idle", Frames = [0, 1, 2, 3, 4, 5, 6, 7] };
	private static readonly SpriteAnimation animCast = new() { Id = "cast", Frames = [8, 9, 10, 11, 12, 13, 14, 15] };
	private static readonly SpriteAnimation animVanish = new() { Id = "vanish", Frames = [16, 17, 18, 19], Speed = 5f, Loop = false };
	private static readonly SpriteAnimation animAppear = new() { Id = "appear", Frames = [19, 18, 17, 16], Speed = 9f, Loop = false };

	private SlotId loopSound;

	public override void Load()
	{
		//TODO: Add own gores.
		GoreLoader.AddGoreFromTexture<SimpleModGore>(Mod, $"{Mod.Name}/Assets/Conflux/FallenShaman_GoreCloth1");
		GoreLoader.AddGoreFromTexture<SimpleModGore>(Mod, $"{Mod.Name}/Assets/Conflux/FallenShaman_GoreCloth2");
	}

	public override void SetStaticDefaults()
	{
		NPCID.Sets.UsesNewTargetting[Type] = true;
		NPCID.Sets.TeleportationImmune[Type] = true;
		NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Shimmer] = true;
		Main.npcFrameCount[Type] = 5;
	}

	public override void SetDefaults()
	{
		//NPC.aiStyle = -1;
		NPC.aiStyle = NPCAIStyleID.DemonEye;
		NPC.damage = 35;
		NPC.width = 40;
		NPC.height = 32;
		NPC.lifeMax = 500;
		NPC.defense = 30;
		NPC.knockBackResist = 0.4f;
		NPC.noGravity = true;

		NPC.HitSound = new($"{nameof(PathOfTerraria)}/Assets/Sounds/HitEffects/BoneHit", 3) { Pitch = +0f, MaxInstances = 5, Volume = 0.8f, PitchVariance = 0.2f };
		NPC.DeathSound = SoundID.NPCDeath51 with { Volume = 0.45f, Pitch = -0.15f, PitchVariance = 0.1f };

		//NPC.TryEnableComponent<NPCNavigation>(e =>
		//{
		//	e.JumpRange = new(15, 40);
		//});
		NPC.TryEnableComponent<NPCMovement>(e =>
		{
			e.Data.MaxSpeed = 4f;
			e.Data.Acceleration = 32f;
			e.Data.Friction = (8f, 2f);
			e.Data.Push = new() { RequiredNpcType = Type, ApplyOnlyOnGround = false };
		});
		NPC.TryEnableComponent<NPCAnimations>(e =>
		{
			e.BaseFrame = new SpriteFrame(4, 5) with { PaddingX = 0, PaddingY = 0 };
			e.SpriteOffset = new Vector2(0f, -9f);
		});
		NPC.TryEnableComponent<NPCTeleports>(e =>
		{
			// General
			e.Data.Movement = TeleportType.Interpolated;
			e.Data.MaxCooldown = 30;
			e.Data.Disappear = (0, 12);
			e.Data.Reappear = (42, 54);
			e.Data.Invulnerability = (14, 40);
			e.Data.CooldownDamage = (0f, 0);
			e.Data.Velocity = (new(0f, 0f), new(0f, 0f));
			e.Data.TurnInvisible = false;
			e.Data.DisableGravity = false;
			e.Data.LightColor = ColorUtils.FromHexRgb(0x7ce8ff).ToVector3();
			e.Data.DisappearSound = (0, SoundID.Item77 with { Identifier = "Disappear", Volume = 0.5f, Pitch = -0.4f, PitchVariance = 0.2f });
			e.Data.ReappearSound = (40, SoundID.Item77 with { Identifier = "Reappear", Volume = 0.5f, Pitch = +0.4f, PitchVariance = 0.2f });
			// Triggers
			e.Data.TriggerIfEndangered = true;
			e.Data.TriggerAtDistance = (1024f, float.PositiveInfinity); //(0f, 192f);
			// Placement
			e.Data.PlaceOriginAtTarget = true;
			e.Data.RequireLineOfSightOnExit = true;
			e.Data.RequiredTargetDistance = (300f, 520f);
			// e.Data.RequiredTargetDistanceDiff = (-32f, -64f);
			e.Data.BasePlacement.OnGround = false;
			e.Data.BasePlacement.Area = new Rectangle() with { Width = 64, Height = 35 };
		});
		NPC.TryEnableComponent<NPCTargeting>();
		NPC.TryEnableComponent<NPCAttacking>(e =>
		{
			e.Data.LengthInTicks = 84;
			e.Data.CooldownLength = 96;
			e.Data.NoGravityLength = 0;
			e.Data.InitiationRange = new(500f, 250f);
			e.Data.Dash = (39, 54, new(-9f, -5f)); // Knocked back.
			e.Data.Damage = (39, 54, 0);
			e.Data.Hitbox = default;
			e.Data.Movement = (0f, 1f);
			e.Data.AimLag = ((new(0.01f), new(0.01f)), (0f, 0.99f));

			if (!Main.dedServ)
			{
				e.Data.Sounds =
				[
					(0, SoundID.NPCDeath51 with {
						MaxInstances = 2,
						Volume = 0.15f,
						Pitch = -0.8f,
						PitchVariance = 0.15f,
					}),
					(0, new($"{nameof(PathOfTerraria)}/Assets/Sounds/Conflux/CryoStalkerCast") {
						MaxInstances = 2,
						Volume = 0.3f,
						Pitch = -0f,
						PitchVariance = 0.15f,
					}),
				];
			}
		});
		NPC.TryEnableComponent<NPCVoice>(e =>
		{
			e.Data.PainSound = (5, SoundID.NPCDeath51 with { Volume = 0.2f, Pitch = +0.8f, PitchVariance = 0.1f });
		});
		NPC.TryEnableComponent<NPCHitEffects>(c =>
		{
			c.AddDust(new(DustID.Ice, 7));
			c.AddGore(new($"{PoTMod.ModName}/{nameof(BloodSplatSmall)}", 1));
			c.AddGore(new($"{PoTMod.ModName}/{nameof(BloodSplatMedium)}", 1));

			c.AddDust(new(DustID.Ice, 50, NPCHitEffects.OnDeath, d => d.velocity.Y -= 5f));
			c.AddGore(new($"{PoTMod.ModName}/{nameof(BloodSplatSmall)}", 2, NPCHitEffects.OnDeath));
			c.AddGore(new($"{PoTMod.ModName}/{nameof(BloodSplatMedium)}", 2, NPCHitEffects.OnDeath));
			c.AddGore(new($"{PoTMod.ModName}/{nameof(BloodSplatLarge)}", 2, NPCHitEffects.OnDeath));
			c.AddGore(new($"{PoTMod.ModName}/FallenShaman_GoreCloth1", 2, NPCHitEffects.OnDeath));
			c.AddGore(new($"{PoTMod.ModName}/FallenShaman_GoreCloth2", 2, NPCHitEffects.OnDeath));
			c.AddGore(new($"{PoTMod.ModName}/{nameof(BloodSplatSmall)}", 2, NPCHitEffects.OnDeath));
			c.AddGore(new($"{PoTMod.ModName}/{nameof(BloodSplatMedium)}", 2, NPCHitEffects.OnDeath));
			c.AddGore(new($"{PoTMod.ModName}/{nameof(BloodSplatLarge)}", 2, NPCHitEffects.OnDeath));
		});
	}

	public override void AI()
	{
		Context ctx = new(NPC);

		ctx.Animations.Advance();
		ctx.Targeting.ManualUpdate(new(NPC));
		ctx.Teleports.ManualUpdate(new(NPC));
		ctx.Attacking.ManualUpdate(new(NPC));
		ctx.Movement.ManualUpdate(new(NPC));
		ctx.Animations.Set(PickAnimation(in ctx));

		UpdateEffects(in ctx);

		// Prevent staying too high.
		Vector2 targetCenter = ctx.Targeting.GetTargetCenter(NPC);
		Vector2 targetDiff = targetCenter - ctx.Center;
		if (targetDiff.Y > 200f)
		{
			NPC.velocity = MovementUtils.DirAccel(NPC.velocity, Vector2.UnitY, 8f, 1f * TimeSystem.LogicDeltaTime);
		}

		// Shoot projectiles during the attack.
		bool atkActive = ctx.Attacking.Active;
		int atkProg = ctx.Attacking.Data.Progress;
		int atkBase = ctx.Attacking.Data.Dash.Start;
		for (int i = 0; i < (atkActive ? 3 : 0); i++)
		{
			if (atkProg != atkBase + (i * 3)) { continue; }

			if (!Main.dedServ)
			{
				SoundEngine.PlaySound(SoundID.Item92 with { Volume = 0.65f, MaxInstances = 3, Pitch = 0.2f, PitchVariance = 0.2f }, ctx.Center);
			}

			if (Main.netMode != NetmodeID.MultiplayerClient)
			{
				int projType = ModContent.ProjectileType<CryoStalkerIcicle>();
				float angleOffset = (i == 0 ? 0f : (i == 1 ? -1f : +1f)) * MathHelper.ToRadians(10f);
				Vector2 velocity = (ctx.Attacking.Data.Angle + angleOffset).ToRotationVector2() * 7f;
				Projectile.NewProjectileDirect(NPC.GetSource_FromThis(), NPC.Center, velocity, projType, NPC.defDamage, 1f);
			}
		}

		// Experiment with it leaving trail projectiles.
		// if (ctx.Teleports.Active && ctx.Teleports.Data.Progress % 10 == 0)
		// {
		// 	int projType = ProjectileID.HappyBomb;
		// 	Vector2 velocity = new Vector2(0f, -5f);
		// 	Projectile.NewProjectileDirect(NPC.GetSource_FromThis(), NPC.Center, velocity, projType, NPC.defDamage, 1f);
		// }
	}

	private SpriteAnimation? PickAnimation(in Context ctx)
	{
		Vector2 vel = NPC.position - NPC.oldPosition;

		return 0 switch
		{
			_ when ctx.Attacking.Active && ctx.Attacking.Data.Progress <= ctx.Attacking.Data.Damage.Start => animCast with { Speed = 25f },
			_ when ctx.Teleports.Active => ctx.Teleports.Data.Progress <= ctx.Teleports.Data.Reappear.Start ? animVanish with { Speed = 10f } : animAppear,
			_ => animIdle with { Speed = NPC.velocity.LengthSquared() },
		};
	}

	private void UpdateEffects(in Context ctx)
	{
		if (Main.dedServ) { return; }

		// Rotation
		NPC.rotation = -0.4f * NPC.spriteDirection;
		NPC.rotation += (NPC.velocity.X * 0.05f);
		NPC.rotation += (NPC.velocity.Y * -0.05f);
		if (NPC.HasValidTarget)
		{
			Vector2 targetDiff = ctx.Targeting.GetTargetCenter(NPC) - ctx.Center;

			if (NPC.spriteDirection == (targetDiff.X >= 0f ? 1 : -1))
			{
				NPC.rotation += (targetDiff * NPC.spriteDirection).ToRotation() * 0.5f;
			}
		}

		// Light
		float castPower = MathUtils.DistancePower(MathF.Abs(ctx.Attacking.Data.Progress - (ctx.Attacking.Data.Damage.Start - 1)), 1, 30);
		float lightPower = MathHelper.Lerp(0.2f, 1.0f, castPower);
		Lighting.AddLight(ctx.Center, ColorUtils.FromHexRgb(0x7ce8ff).ToVector3() * lightPower);

		// Sound
		var sound = new SoundStyle($"{nameof(PathOfTerraria)}/Assets/Sounds/Conflux/CryoStalkerLoop")
		{
			Volume = 0.5f,
			MaxInstances = 3,
			SoundLimitBehavior = SoundLimitBehavior.IgnoreNew,
			PauseBehavior = PauseBehavior.PauseWithGame,
			IsLooped = true,
		};
		float speed = NPC.velocity.Length();
		float volume = 1f; // Math.Clamp(speed * 0.1f, 0.01f, 1f);
		float pitch = Math.Clamp(-1f + (speed * 0.2f), -1f, 1f);
		SoundUtils.UpdateLoopingSound(ref loopSound, ctx.Center, volume, null, sound, _ => Main.npc[NPC.whoAmI] is { active: true } n && n == NPC);
	}
}

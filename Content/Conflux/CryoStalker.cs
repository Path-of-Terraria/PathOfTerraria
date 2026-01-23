using PathOfTerraria.Common.AI;
using PathOfTerraria.Common.NPCs.Components;
using PathOfTerraria.Common.NPCs.Effects;
using PathOfTerraria.Common.Utilities;
using PathOfTerraria.Content.Gores;
using PathOfTerraria.Core.Time;
using PathOfTerraria.Utilities.Terraria;
using PathOfTerraria.Utilities.Xna;
using ReLogic.Content;
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
				PitchVariance = 0.25f,
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
		public NPCNavigation Navigation { get; } = npc.GetGlobalNPC<NPCNavigation>();
		public NPCTargeting Targeting { get; } = npc.GetGlobalNPC<NPCTargeting>();
		public NPCAttacking Attacking { get; } = npc.GetGlobalNPC<NPCAttacking>();
		public NPCAnimations Animations { get; } = npc.GetGlobalNPC<NPCAnimations>();
		public NPCTeleports Teleports { get; } = npc.GetGlobalNPC<NPCTeleports>();
	}

	private static readonly SpriteAnimation animIdle = new() { Id = "idle", Frames = [0, 1, 2, 3, 4, 5, 6, 7] };
	private static readonly SpriteAnimation animCast = new() { Id = "cast", Frames = [8, 9, 10, 11, 12, 13, 14, 15] };
	private static readonly SpriteAnimation animVanish = new() { Id = "vanish", Frames = [16, 17, 18, 19], Speed = 5f, Loop = false };
	private static readonly SpriteAnimation animAppear = new() { Id = "appear", Frames = [19, 18, 17, 16], Speed = 9f, Loop = false };

	public override void Load()
	{
		//TODO: Gore.
	}

	public override void SetStaticDefaults()
	{
		NPCID.Sets.TeleportationImmune[Type] = true;
		NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Shimmer] = true;
		Main.npcFrameCount[Type] = 5;
	}

	public override void SetDefaults()
	{
		//NPC.aiStyle = -1;
		NPC.aiStyle = NPCAIStyleID.DemonEye;
		NPC.damage = 35;
		NPC.width = 60;
		NPC.height = 32;
		NPC.lifeMax = 500;
		NPC.defense = 30;
		NPC.knockBackResist = 0.5f;
		NPC.noGravity = true;

		NPC.HitSound = new($"{nameof(PathOfTerraria)}/Assets/Sounds/HitEffects/FleshHit", 3) { Pitch = +0.35f, MaxInstances = 5, Volume = 0.4f, PitchVariance = 0.2f };
		NPC.DeathSound = SoundID.NPCDeath51 with { Volume = 0.53f, Pitch = -0.1f, PitchVariance = 0.1f };

		NPC.TryEnableComponent<NPCNavigation>(e =>
		{
			e.JumpRange = new(15, 40);
		});
		NPC.TryEnableComponent<NPCMovement>(e =>
		{
			e.Data.MaxSpeed = 4f;
			e.Data.Acceleration = 32f;
			e.Data.Friction = (8f, 2f);
		});
		NPC.TryEnableComponent<NPCAnimations>(e =>
		{
			e.BaseFrame = new SpriteFrame(4, 5) with { PaddingX = 0, PaddingY = 0 };
			e.SpriteOffset = new Vector2(0f, -9f);
		});
		NPC.TryEnableComponent<NPCTeleports>(e =>
		{
			e.Data.MaxCooldown = 300;
			e.Data.Disappear = (0, 12);
			e.Data.Reappear = (42, 54);
			e.Data.Invulnerability = (0, 45);
			e.Data.CooldownDamage = (0f, 0);
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
						Volume = 0.2f,
						Pitch = -0.8f,
						PitchVariance = 0.15f,
					}),
				];
			}
		});
		NPC.TryEnableComponent<NPCVoice>(e =>
		{
			e.Data.PainSound = (3, SoundID.NPCHit20 with
			{
				MaxInstances = 3,
				Volume = 0.9f,
				PitchVariance = 0.15f,
			});
		});
		NPC.TryEnableComponent<NPCHitEffects>(c =>
		{
			c.AddGore(new NPCHitEffects.GoreSpawnParameters($"{PoTMod.ModName}/{nameof(BloodSplatSmall)}", 1));
			c.AddGore(new NPCHitEffects.GoreSpawnParameters($"{PoTMod.ModName}/{nameof(BloodSplatMedium)}", 1));

			c.AddGore(new($"{PoTMod.ModName}/{nameof(BloodSplatSmall)}", 2, NPCHitEffects.OnDeath));
			c.AddGore(new($"{PoTMod.ModName}/{nameof(BloodSplatMedium)}", 2, NPCHitEffects.OnDeath));
			c.AddGore(new($"{PoTMod.ModName}/{nameof(BloodSplatLarge)}", 2, NPCHitEffects.OnDeath));
			//TODO: Gores.
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
		//ctx.Teleports.ManualUpdate(new(NPC));
		ctx.Attacking.ManualUpdate(new(NPC));
		ctx.Movement.ManualUpdate(new(NPC));
		ctx.Animations.Set(PickAnimation(ref ctx));

		// Prevent staying too high.
		Vector2 targetCenter = ctx.Targeting.GetTargetCenter(NPC);
		Vector2 targetDiff = targetCenter - ctx.Center;
		if (targetDiff.Y > 200f)
		{
			NPC.velocity = MovementUtils.DirAccel(NPC.velocity, Vector2.UnitY, 8f, 1f * TimeSystem.LogicDeltaTime);
		}

		if (!Main.dedServ)
		{
			NPC.rotation = 0f;

			if (NPC.spriteDirection == (targetDiff.X >= 0f ? 1 : -1))
			{
				NPC.rotation += (targetDiff * NPC.spriteDirection).ToRotation() * 0.5f;
			}

			NPC.rotation += (NPC.velocity.X * 0.05f);
			NPC.rotation += (NPC.velocity.Y * -0.05f);

			float castPower = MathUtils.DistancePower(MathF.Abs(ctx.Attacking.Data.Progress - (ctx.Attacking.Data.Damage.Start - 1)), 1, 30);
			float lightPower = MathHelper.Lerp(0.2f, 1.0f, castPower);
			Lighting.AddLight(ctx.Center, ColorUtils.FromHexRgb(0x7ce8ff).ToVector3() * lightPower);
		}

		// Shoot projectiles during the attack.
		int atkProg = ctx.Attacking.Data.Progress;
		int atkBase = ctx.Attacking.Data.Dash.Start;
		for (int i = 0; i < (ctx.Attacking.Active ? 3 : 0); i++)
		{
			if (atkProg != atkBase + (i * 3)) { continue; }

			if (!Main.dedServ)
			{
				SoundEngine.PlaySound(SoundID.Item92 with { Volume = 0.5f, MaxInstances = 3, PitchVariance = 0.2f }, ctx.Center);
			}

			if (Main.netMode != NetmodeID.MultiplayerClient)
			{
				int projType = ModContent.ProjectileType<CryoStalkerIcicle>();
				float angleOffset = (i == 0 ? 0f : (i == 1 ? -1f : +1f)) * MathHelper.ToRadians(10f);
				Vector2 velocity = (ctx.Attacking.Data.Angle + angleOffset).ToRotationVector2() * 7f;
				Projectile.NewProjectileDirect(NPC.GetSource_FromThis(), NPC.Center, velocity, projType, NPC.defDamage, 1f);
			}
		}
	}

	private SpriteAnimation? PickAnimation(ref Context ctx)
	{
		Vector2 vel = NPC.position - NPC.oldPosition;

		return 0 switch
		{
			_ when ctx.Attacking.Active && ctx.Attacking.Data.Progress <= ctx.Attacking.Data.Damage.Start => animCast with { Speed = 25f },
			_ when ctx.Teleports.Active => ctx.Teleports.Data.Progress <= ctx.Teleports.Data.Reappear.Start ? animVanish : animAppear,
			_ => animIdle with { Speed = NPC.velocity.LengthSquared() },
		};
	}
}

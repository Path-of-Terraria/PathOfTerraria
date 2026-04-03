using System.Diagnostics;
using PathOfTerraria.Common.AI;
using PathOfTerraria.Common.NPCs.Components;
using PathOfTerraria.Common.NPCs.Effects;
using PathOfTerraria.Common.Utilities;
using PathOfTerraria.Content.Gores;
using PathOfTerraria.Core.Time;
using PathOfTerraria.Utilities.Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;

#nullable enable

namespace PathOfTerraria.Content.Conflux;

/// <summary> Weak walking fodder encounterd in masses. </summary>
internal sealed class Shambling : ModNPC
{
	private readonly struct Context(NPC npc)
	{
		// public Vector2 Center { get; } = npc.Center;
		public NPCMovement Movement { get; } = npc.GetGlobalNPC<NPCMovement>();
		public NPCNavigation Navigation { get; } = npc.GetGlobalNPC<NPCNavigation>();
		public NPCTargeting Targeting { get; } = npc.GetGlobalNPC<NPCTargeting>();
		public NPCAttacking Attacking { get; } = npc.GetGlobalNPC<NPCAttacking>();
		public NPCAnimations Animations { get; } = npc.GetGlobalNPC<NPCAnimations>();
	}

	private static readonly SpriteAnimation animIdle = new() { Id = "idle", Frames = [0, 1, 2, 3], Speed = 3f };
	private static readonly SpriteAnimation animJump = new() { Id = "jump", Frames = [4] };
	private static readonly SpriteAnimation animFall = new() { Id = "fall", Frames = [5] };
	private static readonly SpriteAnimation animWalk = new() { Id = "walk", Frames = [6, 7, 8, 9, 10, 11] };
	private static readonly SpriteAnimation animAttack = new() { Id = "attack", Frames = [12, 12, 13, 14, 14, 15, 15, 16, 17, 18, 18, 18, 18, 19], Speed = 12f, Loop = false };

	public override void Load()
	{
		if (Main.dedServ) { return; }
		GoreLoader.AddGoreFromTexture<AdvancedGore>(Mod, $"{Texture}_GoreHead");
		GoreLoader.AddGoreFromTexture<AdvancedGore>(Mod, $"{Texture}_GoreChest");
		GoreLoader.AddGoreFromTexture<AdvancedGore>(Mod, $"{Texture}_GoreArm1");
		GoreLoader.AddGoreFromTexture<AdvancedGore>(Mod, $"{Texture}_GoreArm2");
		GoreLoader.AddGoreFromTexture<AdvancedGore>(Mod, $"{Texture}_GoreLeg1");
		GoreLoader.AddGoreFromTexture<AdvancedGore>(Mod, $"{Texture}_GoreLeg2");
	}

	public override void SetStaticDefaults()
	{
		NPCID.Sets.UsesNewTargetting[Type] = true;
		NPCID.Sets.TeleportationImmune[Type] = true;
		NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Shimmer] = true;
		Main.npcFrameCount[Type] = 6;
	}
	public override void SetDefaults()
	{
		NPC.aiStyle = -1;
		NPC.lifeMax = 200;
		NPC.defense = 15;
		NPC.damage = 30;
		NPC.width = 20;
		NPC.height = 44;
		NPC.knockBackResist = 0.6f;

		NPC.HitSound = new($"{nameof(PathOfTerraria)}/Assets/Sounds/HitEffects/FleshHit", 3) { MaxInstances = 5, Volume = 0.4f };
		NPC.DeathSound = SoundID.NPCDeath23 with { Pitch = +0.1f, PitchVariance = 0.15f };

		NPC.TryEnableComponent<NPCNavigation>(e =>
		{
			e.JumpRange = new(7, 14);
		});
		NPC.TryEnableComponent<NPCMovement>(e =>
		{
			e.Data.MaxSpeed = 2.5f;
			e.Data.Acceleration = 10f;
			e.Data.Friction = (6f, 2f);
			e.Data.Push = new()
			{
				Speed = (2f, 1f),
				Range = (8f, 16f),
				RequiredNpcType = Type,
				ApplyOnlyOnGround = false,
			};
		});
		NPC.TryEnableComponent<NPCAnimations>(e =>
		{
			e.BaseFrame = new SpriteFrame(6, 6) with { PaddingX = 0, PaddingY = 0 };
			e.SpriteOffset = new(0f, -3f);
		});
		NPC.TryEnableComponent<NPCTargeting>();
		NPC.TryEnableComponent<NPCAttacking>(e =>
		{
			e.Data.LengthInTicks = 90;
			e.Data.CooldownLength = 60;
			e.Data.NoGravityLength = 15;
			e.Data.InitiationRange = new(100f, 180f);
			e.Data.Dash = (30, 45, new(8f, 4f));
			e.Data.Damage = (30, 45, DamageInstance.EnemyAttackFilter);
			e.Data.Hitbox = (new(80, 80), new(32f, 24), new(8f, 2f));
			e.Data.Movement = (0f, 0.95f, 1f);

			if (!Main.dedServ)
			{
				e.Data.SlashTexture = ModContent.Request<Texture2D>($"{Texture}_Slash");
				e.Data.Sounds =
				[
					(0, SoundID.NPCHit55 with
					{
						MaxInstances = 3,
						Volume = 0.2f,
						Pitch = 0.0f,
						PitchVariance = 0.2f,
						Identifier = $"{nameof(Shambling)}Attack",
					}),
					(20, SoundID.Item71 with { Volume = 0.6f, Pitch = -0.90f, PitchVariance = 0.15f, MaxInstances = 3 }),
					// (15, SoundID.NPCHit56 with { Volume = 0.25f, Pitch = +0.9f, PitchVariance = 0.1f, MaxInstances = 3 }),
				];
			}
		});
		NPC.TryEnableComponent<NPCFootsteps>(e =>
		{
			e.Data.Frames = new() { { "walk", [0, 3] } };
			e.Data.StepSound = new SoundStyle($"{PoTMod.ModName}/Assets/Sounds/Footsteps/MonsterStomp", 3)
			{
				Volume = 0.05f,
				Pitch = +0.8f,
				PitchVariance = 0.2f,
				MaxInstances = 9,
			};
		});
		NPC.TryEnableComponent<NPCVoice>(e =>
		{
			e.Data.PainSound = (3, new($"{nameof(PathOfTerraria)}/Assets/Sounds/Conflux/AbominablePain", 2)
			{
				MaxInstances = 5,
				Volume = 0.5f,
				Pitch = +0.5f,
				PitchVariance = 0.2f,
				Identifier = $"{nameof(Shambling)}Pain",
			});
		});
		NPC.TryEnableComponent<NPCHitEffects>(c =>
		{
			c.AddGore(new($"{PoTMod.ModName}/{nameof(BloodSplatMedium)}", 1));

			c.AddGore(new($"{PoTMod.ModName}/{nameof(BloodSplatSmall)}", 2, NPCHitEffects.OnDeath));
			c.AddGore(new($"{PoTMod.ModName}/{nameof(BloodSplatMedium)}", 1, NPCHitEffects.OnDeath));
			c.AddGore(new($"{PoTMod.ModName}/{nameof(BloodSplatLarge)}", 1, NPCHitEffects.OnDeath));
			c.AddGore(new($"{PoTMod.ModName}/{Name}_GoreHead", +1, NPCHitEffects.OnDeath) { Position = (new(0, 0), new(+00, -16), new(3, 3)) });
			c.AddGore(new($"{PoTMod.ModName}/{Name}_GoreChest", 1, NPCHitEffects.OnDeath) { Position = (new(0, 0), new(+00, +00), new(3, 3)) });
			c.AddGore(new($"{PoTMod.ModName}/{Name}_GoreArm1", +1, NPCHitEffects.OnDeath) { Position = (new(0, 0), new(-16, +00), new(3, 3)) });
			c.AddGore(new($"{PoTMod.ModName}/{Name}_GoreArm2", +1, NPCHitEffects.OnDeath) { Position = (new(0, 0), new(+16, +00), new(3, 3)) });
			c.AddGore(new($"{PoTMod.ModName}/{Name}_GoreLeg1", +1, NPCHitEffects.OnDeath) { Position = (new(0, 0), new(+16, -16), new(3, 3)) });
			c.AddGore(new($"{PoTMod.ModName}/{Name}_GoreLeg2", +1, NPCHitEffects.OnDeath) { Position = (new(0, 0), new(+16, +16), new(3, 3)) });
			c.AddGore(new($"{PoTMod.ModName}/{nameof(BloodSplatSmall)}", 2, NPCHitEffects.OnDeath));
			c.AddGore(new($"{PoTMod.ModName}/{nameof(BloodSplatMedium)}", 1, NPCHitEffects.OnDeath));
			c.AddGore(new($"{PoTMod.ModName}/{nameof(BloodSplatLarge)}", 1, NPCHitEffects.OnDeath));
		});
	}

	public override void AI()
	{
		Context ctx = new(NPC);

		ctx.Movement.Data.MaxSpeed = 3f;
		ctx.Attacking.Data.InitiationRange = new(100f, 180f);

		bool climbing = false;
		float closestChase = float.PositiveInfinity;
		foreach (NPC other in Main.ActiveNPCs)
		{
			if (other == NPC) { continue; }
			if (other.type != NPC.type) { continue; }

			float howMuchAhead = (NPC.position.X - other.position.X) * NPC.direction;

			if (!climbing && (NPC.position.Y <= other.position.Y && howMuchAhead < 0f)
			&& other.WithinRange(NPC.Center, 30f))
			{
				climbing = true;
			}

			if (other.direction == NPC.direction && MathF.Abs(other.position.Y - NPC.position.Y) <= 32f && howMuchAhead > 0f)
			{
				closestChase = MathF.Min(closestChase, howMuchAhead);
			}
		}

		// Slow down to let friends boost themselves.
		if (closestChase < float.PositiveInfinity)
		{
			float linear = MathUtils.Clamp01(closestChase / 250f);
			float exponential = MathF.Pow(linear, 2f);
			// Main.NewText($"{exponential:0.00}");
			float multiplier = MathHelper.Lerp(0.1f, 1.0f, exponential);
			ctx.Movement.Data.MaxSpeed *= multiplier;
		}

		if (climbing && ctx.Attacking.Data.Cooldown.Value <= 0)
		{
			// NPC.velocity.X = NPC.direction * 5f;
			NPC.velocity = MovementUtils.DirAccel(NPC.velocity, NPC.DirectionTo(ctx.Targeting.GetTargetCenter(NPC)), 5.5f, 5.5f);
			NPC.velocity.Y = -4.4f;
			NPC.spriteDirection = NPC.direction;

			// Having climbed, trigger attacks at a high range.
			ctx.Attacking.Data.InitiationRange.X = 360f;

			if (!Main.dedServ)
			{
				NPC.GetGlobalNPC<NPCVoice>().Play(NPC, new($"{nameof(PathOfTerraria)}/Assets/Sounds/Conflux/AbominablePain", 2)
				{
					MaxInstances = 3,
					Volume = 0.4f,
					Pitch = +0.9f,
					PitchVariance = 0.2f,
					Identifier = $"{nameof(Shambling)}Jump",
				});
			}
		}

		ctx.Animations.Advance();
		ctx.Targeting.ManualUpdate(new(NPC));
		ctx.Attacking.ManualUpdate(new(NPC));
		ctx.Movement.ManualUpdate(new(NPC));
		ctx.Animations.Set(PickAnimation(in ctx));
	}

	private SpriteAnimation? PickAnimation(in Context ctx)
	{
		const float MinWalkSpeed = 0.5f;

		Vector2 vel = NPC.position - NPC.oldPosition;

		return ctx.Animations.Current switch
		{
			_ when ctx.Attacking.Active => animAttack,
			_ when vel.Y < 0f => animJump,
			_ when vel.Y > 0f => animFall,
			_ when vel.Y == 0f && Math.Abs(vel.X) >= MinWalkSpeed => animWalk with { Speed = vel.X * animWalk.Speed * 4f * NPC.spriteDirection },
			_ when vel.Y == 0f && Math.Abs(vel.X) <= MinWalkSpeed => animIdle,
			_ => null,
		};
	}
}


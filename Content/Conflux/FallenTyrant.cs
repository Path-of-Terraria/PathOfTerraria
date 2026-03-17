using PathOfTerraria.Common.AI;
using PathOfTerraria.Common.NPCs.Components;
using PathOfTerraria.Common.NPCs.Effects;
using PathOfTerraria.Content.Gores;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;

#nullable enable

namespace PathOfTerraria.Content.Conflux;

/// <summary> Heavy low-speed demon that annihilates its targets if they get too close. </summary>
internal sealed class FallenTyrant : ModNPC
{
	private readonly struct Context(NPC npc)
	{
		public NPCMovement Movement { get; } = npc.GetGlobalNPC<NPCMovement>();
		public NPCNavigation Navigation { get; } = npc.GetGlobalNPC<NPCNavigation>();
		public NPCTargeting Targeting { get; } = npc.GetGlobalNPC<NPCTargeting>();
		public NPCAttacking Attacking { get; } = npc.GetGlobalNPC<NPCAttacking>();
		public NPCAnimations Animations { get; } = npc.GetGlobalNPC<NPCAnimations>();
	}

	private static readonly SpriteAnimation animIdle = new() { Id = "idle", Frames = [0, 1, 2, 3], Speed = 3f };
	private static readonly SpriteAnimation animJump = new() { Id = "jump", Frames = [4] };
	private static readonly SpriteAnimation animFall = new() { Id = "fall", Frames = [5] };
	private static readonly SpriteAnimation animWalk = new() { Id = "walk", Frames = [6, 7, 8, 9, 10, 11], Speed = 4f };
	private static readonly SpriteAnimation animAttack = new()
	{
		Id = "attack",
		Frames = [23, 23, 24, 24, 24, 24, 25, 26, 27, 28, 28, 28, 29],
		Speed = 12f,
		Loop = false,
	};

	public override void Load()
	{
		GoreLoader.AddGoreFromTexture<SimpleModGore>(Mod, $"{Texture}_GoreBlade");
		GoreLoader.AddGoreFromTexture<AdvancedGore>(Mod, $"{Texture}_GoreHead");
		GoreLoader.AddGoreFromTexture<AdvancedGore>(Mod, $"{Texture}_GoreChest");
		GoreLoader.AddGoreFromTexture<AdvancedGore>(Mod, $"{Texture}_GoreArm");
		GoreLoader.AddGoreFromTexture<AdvancedGore>(Mod, $"{Texture}_GoreLeg");
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
		NPC.aiStyle = -1;
		NPC.lifeMax = 550;
		NPC.defense = 15;
		NPC.damage = 65;
		NPC.width = 36;
		NPC.height = 38;
		NPC.knockBackResist = 0.2f;

		NPC.HitSound = new($"{nameof(PathOfTerraria)}/Assets/Sounds/HitEffects/FleshHit", 3) { MaxInstances = 5, Volume = 0.4f };
		NPC.DeathSound = SoundID.NPCDeath23 with { Pitch = +0.1f, PitchVariance = 0.15f, Identifier = "FallenDeath" };

		NPC.TryEnableComponent<NPCNavigation>(e =>
		{
			e.JumpRange = new(8, 20);
		});
		NPC.TryEnableComponent<NPCMovement>(e =>
		{
			e.Data.MaxSpeed = 1.5f;
			e.Data.Acceleration = 32f;
			e.Data.Friction = (8f, 2f);
			e.Data.Push = new() { RequiredNpcType = Type };
		});
		NPC.TryEnableComponent<NPCAnimations>(e =>
		{
			e.BaseFrame = new SpriteFrame(6, 5) with { PaddingX = 0, PaddingY = 0 };
			e.SpriteOffset = new(+0f, +1f);
		});
		NPC.TryEnableComponent<NPCTargeting>();
		NPC.TryEnableComponent<NPCAttacking>(e =>
		{
			e.Data.LengthInTicks = 120;
			e.Data.CooldownLength = 120;
			e.Data.NoGravityLength = 15;
			e.Data.InitiationRange = new(512f, 384f);
			e.Data.Dash = (40, 55, new(3f, 0f));
			e.Data.Damage = (40, 55, DamageInstance.EnemyAttackFilterWithInfighting);
			e.Data.Hitbox = (new(40, 40), new(32f, 32f), new(+8f, +2f));
			e.Data.Movement = (0.4f, 0.9f, 0.95f);

			if (!Main.dedServ)
			{
				e.Data.SlashTexture = ModContent.Request<Texture2D>($"{nameof(PathOfTerraria)}/Assets/Misc/Slash");
				e.Data.Sounds =
				[
					(5, SoundID.NPCHit56 with { Volume = 0.25f, Pitch = +0.4f, PitchVariance = 0.1f, MaxInstances = 3 }),
					(25, SoundID.Item71 with { Volume = 0.6f, Pitch = -0.80f, PitchVariance = 0.15f, MaxInstances = 3 }),
				];
			}
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
			e.Data.PainSound = (3, SoundID.NPCHit56 with { Pitch = +0.3f, PitchVariance = 0.4f, Identifier = "FallenHit" });
		});
		NPC.TryEnableComponent<NPCHitEffects>(c =>
		{
			c.AddGore(new($"{PoTMod.ModName}/{nameof(BloodSplatMedium)}", 1));

			c.AddGore(new($"{PoTMod.ModName}/{nameof(BloodSplatSmall)}", 2, NPCHitEffects.OnDeath));
			c.AddGore(new($"{PoTMod.ModName}/{nameof(BloodSplatMedium)}", 1, NPCHitEffects.OnDeath));
			c.AddGore(new($"{PoTMod.ModName}/{nameof(BloodSplatLarge)}", 1, NPCHitEffects.OnDeath));
			c.AddGore(new($"{PoTMod.ModName}/{Name}_GoreHead", +1, NPCHitEffects.OnDeath) { Position = (new(0, 0), new(+00, -16), new(3, 3)) });
			c.AddGore(new($"{PoTMod.ModName}/{Name}_GoreChest", 1, NPCHitEffects.OnDeath) { Position = (new(0, 0), new(+00, +00), new(3, 3)) });
			c.AddGore(new($"{PoTMod.ModName}/{Name}_GoreBlade", 1, NPCHitEffects.OnDeath) { Position = (new(0, 0), new(+00, +00), new(15, 15)) });
			c.AddGore(new($"{PoTMod.ModName}/{Name}_GoreArm", +1, NPCHitEffects.OnDeath) { Position = (new(0, 0), new(-16, +00), new(3, 3)) });
			c.AddGore(new($"{PoTMod.ModName}/{Name}_GoreArm", +1, NPCHitEffects.OnDeath) { Position = (new(0, 0), new(+16, +00), new(3, 3)) });
			c.AddGore(new($"{PoTMod.ModName}/{Name}_GoreLeg", +1, NPCHitEffects.OnDeath) { Position = (new(0, 0), new(+16, -16), new(3, 3)) });
			c.AddGore(new($"{PoTMod.ModName}/{Name}_GoreLeg", +1, NPCHitEffects.OnDeath) { Position = (new(0, 0), new(+16, +16), new(3, 3)) });
			c.AddGore(new($"{PoTMod.ModName}/{nameof(BloodSplatSmall)}", 2, NPCHitEffects.OnDeath));
			c.AddGore(new($"{PoTMod.ModName}/{nameof(BloodSplatMedium)}", 1, NPCHitEffects.OnDeath));
			c.AddGore(new($"{PoTMod.ModName}/{nameof(BloodSplatLarge)}", 1, NPCHitEffects.OnDeath));
		});
	}

	public override void AI()
	{
		Context ctx = new(NPC);

		ctx.Animations.Advance();
		ctx.Targeting.ManualUpdate(new(NPC));
		ctx.Attacking.ManualUpdate(new(NPC));
		ctx.Movement.ManualUpdate(new(NPC));
		ctx.Animations.Set(PickAnimation(in ctx));

		// The tyrant shoots projectiles with its normal attack.
		//TODO: This is a proof of concept.
		if (ctx.Attacking.Active && ctx.Attacking.Data.Progress == ctx.Attacking.Data.Damage.Start - 5)
		{
			if (Main.netMode != NetmodeID.MultiplayerClient)
			{
				Projectile.NewProjectileDirect(NPC.GetSource_FromThis(), NPC.Center, ctx.Attacking.Direction * 6f, ProjectileID.DemonSickle, (int)(NPC.defDamage * 0.5f), 1f);
			}
		}
	}
	public override void OnKill()
	{
		// Spawn soul.
		Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, default, ModContent.ProjectileType<FallenSoul>(), 0, 0f, ai0: Type);
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
			_ when vel.Y == 0f && Math.Abs(vel.X) >= MinWalkSpeed => animWalk with { Speed = vel.X * animWalk.Speed * NPC.spriteDirection },
			_ when vel.Y == 0f && Math.Abs(vel.X) <= MinWalkSpeed => animIdle,
			_ => null,
		};
	}
}


using PathOfTerraria.Common.AI;
using PathOfTerraria.Common.NPCs.Components;
using PathOfTerraria.Common.NPCs.Effects;
using PathOfTerraria.Content.Gores;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;

#nullable enable

namespace PathOfTerraria.Content.Conflux;

/// <summary> Brute medium-speed demon that stabs its targets.  </summary>
internal sealed class FallenSavage : ModNPC
{
	private readonly struct Context(NPC npc)
	{
		public NPCMovement Movement { get; } = npc.GetGlobalNPC<NPCMovement>();
		public NPCNavigation Navigation { get; } = npc.GetGlobalNPC<NPCNavigation>();
		public NPCTargeting Targeting { get; } = npc.GetGlobalNPC<NPCTargeting>();
		public NPCAttacking Attacking { get; } = npc.GetGlobalNPC<NPCAttacking>();
		public NPCAnimations Animations { get; } = npc.GetGlobalNPC<NPCAnimations>();
	}

	private static readonly SpriteAnimation animIdle = new() { Id = "idle", Frames = [0, 5], Speed = 2f };
	private static readonly SpriteAnimation animWalk = new() { Id = "walk", Frames = [10, 11, 12, 13, 14, 15], Speed = 4f };
	private static readonly SpriteAnimation animJump = new() { Id = "jump", Frames = [6] };
	private static readonly SpriteAnimation animFall = new() { Id = "fall", Frames = [7] };
	private static readonly SpriteAnimation animJumpAttack = new() { Id = "jumpAttack", Frames = [8] };
	private static readonly SpriteAnimation animFallAttack = new() { Id = "fallAttack", Frames = [9] };
	private static readonly SpriteAnimation animAttack = new()
	{
		Id = "attack",
		Frames = [16, 16, 17, 17, 17, 18, 19, 20, 21, 18, 19, 20, 21, 22, 22, 23],
		Speed = 20f,
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
		NPC.lifeMax = 400;
		NPC.defense = 15;
		NPC.damage = 50;
		NPC.width = 24;
		NPC.height = 40;
		NPC.knockBackResist = 0.3f;

		NPC.HitSound = new($"{nameof(PathOfTerraria)}/Assets/Sounds/HitEffects/FleshHit", 3) { MaxInstances = 5, Volume = 0.4f };
		NPC.DeathSound = SoundID.NPCDeath23 with { Pitch = +0.1f, PitchVariance = 0.15f, Identifier = "FallenDeath" };

		NPC.TryEnableComponent<NPCNavigation>(e =>
		{
			e.JumpRange = new(8, 20);
		});
		NPC.TryEnableComponent<NPCMovement>(e =>
		{
			e.Data.MaxSpeed = 3f;
			e.Data.Acceleration = 32f;
			e.Data.Friction = (8f, 2f);
			e.Data.Push = new() { RequiredNpcType = Type };
		});
		NPC.TryEnableComponent<NPCAnimations>(e =>
		{
			e.BaseFrame = new SpriteFrame(5, 5) with { PaddingX = 0, PaddingY = 0 };
			e.SpriteOffset = new(0f, -1f);
		});
		NPC.TryEnableComponent<NPCTargeting>();
		NPC.TryEnableComponent<NPCAttacking>(e =>
		{
			e.Data.LengthInTicks = 60;
			e.Data.CooldownLength = 60;
			e.Data.NoGravityLength = 15;
			e.Data.InitiationRange = new(100f, 250f);
			e.Data.Dash = (18, 33, new(10f, 5f));
			e.Data.Damage = (18, 33, DamageInstance.EnemyAttackFilterWithInfighting);
			e.Data.Hitbox = (new(40, 40), new(56f, 56f), new(+8f, +2f));
			e.Data.Movement = (0.4f, 0.9f, 0.95f);

			if (!Main.dedServ)
			{
				e.Data.SlashTexture = ModContent.Request<Texture2D>($"{Texture}_Slash");
				e.Data.Sounds =
				[
					(13, SoundID.Item71 with { Volume = 0.6f, Pitch = -0.80f, PitchVariance = 0.15f, MaxInstances = 3 }),
					(15, SoundID.NPCHit56 with { Volume = 0.25f, Pitch = +0.85f, PitchVariance = 0.1f, MaxInstances = 3 }),
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
		bool isAttacking = ctx.Attacking.Data.Progress >= 0;
		bool rendersSlash = isAttacking && ctx.Attacking.DealingDamage;

		return ctx.Animations.Current switch
		{
			_ when vel.Y < 0f => rendersSlash ? animJumpAttack : animJump,
			_ when vel.Y > 0f => rendersSlash ? animFallAttack : animFall,
			_ when isAttacking => animAttack,
			_ when vel.Y == 0f && Math.Abs(vel.X) >= MinWalkSpeed => animWalk with { Speed = vel.X * animWalk.Speed * NPC.spriteDirection },
			_ when vel.Y == 0f && Math.Abs(vel.X) <= MinWalkSpeed => animIdle,
			_ => null,
		};
	}
}


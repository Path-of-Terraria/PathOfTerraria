using PathOfTerraria.Common.AI;
using PathOfTerraria.Common.NPCs.Components;
using PathOfTerraria.Common.NPCs.Effects;
using PathOfTerraria.Content.Gores;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;

#nullable enable

namespace PathOfTerraria.Content.Conflux;

/// <summary> Sneaky fast-speed demon that teleports at the player.  </summary>
internal sealed class FallenSchemer : ModNPC
{
	private readonly struct Context(NPC npc)
	{
		public NPCMovement Movement { get; } = npc.GetGlobalNPC<NPCMovement>();
		public NPCNavigation Navigation { get; } = npc.GetGlobalNPC<NPCNavigation>();
		public NPCTargeting Targeting { get; } = npc.GetGlobalNPC<NPCTargeting>();
		public NPCAttacking Attacking { get; } = npc.GetGlobalNPC<NPCAttacking>();
		public NPCAnimations Animations { get; } = npc.GetGlobalNPC<NPCAnimations>();
		public NPCTeleports Teleports { get; } = npc.GetGlobalNPC<NPCTeleports>();
	}

	private static readonly SpriteAnimation animIdle = new() { Id = "idle", Frames = [0, 1, 2, 3], Speed = 3f };
	private static readonly SpriteAnimation animJump = new() { Id = "jump", Frames = [4] };
	private static readonly SpriteAnimation animFall = new() { Id = "fall", Frames = [5] };
	private static readonly SpriteAnimation animWalk = new() { Id = "walk", Frames = [6, 7, 8, 9, 10, 11] };
	private static readonly SpriteAnimation animAttack = new() { Id = "attack", Frames = [12, 12, 13, 14, 14, 15, 15, 16, 17, 18, 18, 18, 18, 19], Speed = 17f, Loop = false };
	private static readonly SpriteAnimation animVanish = new() { Id = "vanish", Frames = [20, 21, 22, 23, 24, 25, 26, 33], Speed = 20f, Loop = false };
	private static readonly SpriteAnimation animAppear = new() { Id = "appear", Frames = [26, 27, 28, 29, 30, 31, 32], Speed = 20f, Loop = false };

	public override void Load()
	{
		GoreLoader.AddGoreFromTexture<AdvancedGore>(Mod, $"{Texture}_GoreHead");
		GoreLoader.AddGoreFromTexture<AdvancedGore>(Mod, $"{Texture}_GoreChest");
		GoreLoader.AddGoreFromTexture<AdvancedGore>(Mod, $"{Texture}_GoreArm1");
		GoreLoader.AddGoreFromTexture<AdvancedGore>(Mod, $"{Texture}_GoreArm2");
		GoreLoader.AddGoreFromTexture<AdvancedGore>(Mod, $"{Texture}_GoreLeg1");
		GoreLoader.AddGoreFromTexture<AdvancedGore>(Mod, $"{Texture}_GoreLeg2");
	}

	public override void SetStaticDefaults()
	{
		NPCID.Sets.TeleportationImmune[Type] = true;
		NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Shimmer] = true;
		Main.npcFrameCount[Type] = 6;
	}
	public override void SetDefaults()
	{
		NPC.aiStyle = -1;
		NPC.lifeMax = 250;
		NPC.defense = 15;
		NPC.damage = 45;
		NPC.width = 20;
		NPC.height = 44;
		NPC.knockBackResist = 0.5f;

		NPC.HitSound = new($"{nameof(PathOfTerraria)}/Assets/Sounds/HitEffects/FleshHit", 3) { MaxInstances = 5, Volume = 0.4f };
		NPC.DeathSound = SoundID.NPCDeath23 with { Pitch = +0.1f, PitchVariance = 0.15f, Identifier = "FallenDeath" };

		NPC.TryEnableComponent<NPCNavigation>(e =>
		{
			e.JumpRange = new(8, 20);
		});
		NPC.TryEnableComponent<NPCMovement>(e =>
		{
			e.Data.MaxSpeed = 5.5f;
			e.Data.Acceleration = 32f;
			e.Data.Friction = (8f, 2f);
		});
		NPC.TryEnableComponent<NPCAnimations>(e =>
		{
			e.BaseFrame = new SpriteFrame(6, 6) with { PaddingX = 0, PaddingY = 0 };
			e.SpriteOffset = new(0f, -3f);
		});
		NPC.TryEnableComponent<NPCTargeting>();
		NPC.TryEnableComponent<NPCAttacking>(e =>
		{
			e.Data.LengthInTicks = 60;
			e.Data.CooldownLength = 60;
			e.Data.NoGravityLength = 15;
			e.Data.InitiationRange = new(140f, 250f);
			e.Data.Dash = (18, 33, new(15f, 10f));
			e.Data.Damage = (18, 33, DamageInstance.EnemyAttackFilterWithInfighting);
			e.Data.Hitbox = (new(112, 112), new(48f, 32f), new(12f, 2f));
			e.Data.Movement = (0.4f, 0.9f);

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
		NPC.TryEnableComponent<NPCTeleports>(e =>
		{
			e.Data.MaxCooldown = 60;
			e.Data.Disappear = (0, 12);
			e.Data.Reappear = (42, 54);
			e.Data.Invulnerability = (0, 45);
			e.Data.CooldownDamage = (0f, 0);
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

		ctx.Animations.Advance();
		ctx.Targeting.ManualUpdate(new(NPC));
		ctx.Teleports.ManualUpdate(new(NPC));
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
			_ when ctx.Teleports.Active => ctx.Teleports.Data.Progress <= ctx.Teleports.Data.Reappear.Start ? animVanish : animAppear,
			_ when vel.Y < 0f => animJump,
			_ when vel.Y > 0f => animFall,
			_ when vel.Y == 0f && Math.Abs(vel.X) >= MinWalkSpeed => animWalk with { Speed = vel.X * animWalk.Speed * 4f * NPC.spriteDirection },
			_ when vel.Y == 0f && Math.Abs(vel.X) <= MinWalkSpeed => animIdle,
			_ => null,
		};
	}
}


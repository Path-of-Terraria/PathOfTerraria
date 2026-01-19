//#define NEVER_ATTACK

using PathOfTerraria.Common.AI;
using PathOfTerraria.Common.NPCs.Components;
using PathOfTerraria.Common.NPCs.Effects;
using PathOfTerraria.Content.Gores;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;

#nullable enable

namespace PathOfTerraria.Content.Conflux;

internal sealed class Abominable : ModNPC
{
	private struct Context(NPC npc)
	{
		public Vector2 Center { get; } = npc.Center;
		public NPCMovement Movement { get; } = npc.GetGlobalNPC<NPCMovement>();
		public NPCNavigation Navigation { get; } = npc.GetGlobalNPC<NPCNavigation>();
		public NPCTargeting Targeting { get; } = npc.GetGlobalNPC<NPCTargeting>();
		public NPCAttacking Attacking { get; } = npc.GetGlobalNPC<NPCAttacking>();
		public NPCAnimations Animations { get; } = npc.GetGlobalNPC<NPCAnimations>();
	}

	private static readonly SpriteAnimation animIdle = new() { Id = "idle", Frames = [0] };
	private static readonly SpriteAnimation animJump = new() { Id = "jump", Frames = [1] };
	private static readonly SpriteAnimation animFall = new() { Id = "fall", Frames = [2] };
	private static readonly SpriteAnimation animWalk = new() { Id = "walk", Frames = [3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14], Speed = 5f };
	private static readonly SpriteAnimation animAttack = new() { Id = "attack", Frames = [15, 16, 17, 18, 19, 20, 21, 22, 23, 24], Speed = 9f, Loop = false };

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
		NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, new() { CustomTexturePath = $"{Mod.Name}/Assets/Conflux/{Name}_Bestiary" });
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
		NPC.TryEnableComponent<NPCMovement>(e =>
		{
			e.Data.MaxSpeed = 4f;
			e.Data.Acceleration = 32f;
			e.Data.Friction = (8f, 2f);
		});
		NPC.TryEnableComponent<NPCAnimations>(e =>
		{
			e.BaseFrame = new SpriteFrame(5, 5) with { PaddingX = 0, PaddingY = 0 };
			e.SpriteOffset = new Vector2(0f, -6f);
		});
		NPC.TryEnableComponent<NPCTargeting>();
		NPC.TryEnableComponent<NPCAttacking>(e =>
		{
			e.Data.LengthInTicks = 84;
			e.Data.CooldownLength = 96;
			e.Data.NoGravityLength = 15;
			e.Data.InitiationRange = new(215f, 400f);
			e.Data.Dash = (39, 54, new(30f, 12f));
			e.Data.Damage = (39, 54, DamageInstance.EnemyAttackFilterWithInfighting);
			e.Data.Hitbox = (new(112, 112), new(48f, 32f), new(12f, 2f));
			e.Data.Movement = (0.4f, 0.84f);

			if (!Main.dedServ)
			{
				e.Data.SlashTexture = ModContent.Request<Texture2D>($"{nameof(PathOfTerraria)}/Assets/Misc/Slash");
				e.Data.Sounds =
				[
					(0, new SoundStyle($"{nameof(PathOfTerraria)}/Assets/Sounds/Conflux/AbominableAttack", 2) {
						MaxInstances = 2,
						Volume = 0.65f,
						Pitch = 0.1f,
						PitchVariance = 0.15f,
					}),
					(34, SoundID.Item71 with { Pitch = -1.00f, PitchVariance = 0.15f }),
				];
			}
		});
		NPC.TryEnableComponent<NPCFootsteps>(e =>
		{
			e.Data.Frames = new() { { "walk", [3, 9] } };
			e.Data.StepSound = new SoundStyle($"{PoTMod.ModName}/Assets/Sounds/Footsteps/MonsterStomp", 3)
			{
				Volume = 0.2f,
				PitchVariance = 0.2f,
				MaxInstances = 3,
			};
			e.Data.ScreenShake = true;
		});
		NPC.TryEnableComponent<NPCVoice>(e =>
		{
			e.Data.PainSound = (3, new($"{nameof(PathOfTerraria)}/Assets/Sounds/Conflux/AbominablePain", 2)
			{
				MaxInstances = 3,
				Volume = 0.5f,
				PitchVariance = 0.1f,
			});
		});
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
		ctx.Targeting.ManualUpdate(new(NPC));
		ctx.Attacking.ManualUpdate(new(NPC));
		ctx.Movement.ManualUpdate(new(NPC));
		ctx.Animations.Set(PickAnimation(ref ctx));
	}

	private SpriteAnimation? PickAnimation(ref Context ctx)
	{
		const float MinWalkSpeed = 1.5f;

		Vector2 vel = NPC.position - NPC.oldPosition;

		return 0 switch
		{
			_ when ctx.Attacking.Data.Progress >= 0 => animAttack,
			_ when vel.Y < 0f => animJump,
			_ when vel.Y > 0f => animFall,
			_ when vel.Y == 0f && Math.Abs(vel.X) >= MinWalkSpeed => animWalk with { Speed = vel.X * animWalk.Speed * NPC.spriteDirection },
			_ when vel.Y == 0f && Math.Abs(vel.X) <= MinWalkSpeed => animIdle,
			_ => null,
		};
	}
}

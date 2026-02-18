using NPCUtils;
using PathOfTerraria.Common.AI;
using PathOfTerraria.Common.NPCs;
using PathOfTerraria.Common.NPCs.Components;
using PathOfTerraria.Common.NPCs.Effects;
using PathOfTerraria.Common.Systems.Synchronization;
using PathOfTerraria.Content.Gores;
using Terraria.GameContent.Bestiary;
using Terraria.ID;

namespace PathOfTerraria.Content.Swamp.NPCs.SwampBoss;

internal class Mossling : ModNPC
{
	private readonly struct Context(NPC npc)
	{
		public NPCAnimations Animations { get; } = npc.GetGlobalNPC<NPCAnimations>();
	}

	private enum States
	{
		Idle,
		Scared
	}

	private static readonly SpriteAnimation animIdle = new() { Id = "idle", Frames = [0], Speed = 0 };
	//private static readonly SpriteAnimation animStartGrab = new() { Id = "startGrab", Frames = [4, 5, 6, 7], Speed = 10, Loop = false };
	//private static readonly SpriteAnimation animTryGrab = new() { Id = "tryGrab", Frames = [8, 9, 10, 11], Speed = 10 };
	//private static readonly SpriteAnimation animCarry = new() { Id = "carry", Frames = [12, 13, 14, 15], Speed = 12 };

	private States State
	{
		get => (States)NPC.ai[0];
		set => NPC.ai[0] = (float)value;
	}

	private ref float Timer => ref NPC.ai[1];

	public override void SetStaticDefaults()
	{
		Main.npcFrameCount[Type] = 1;
	}

	public override void SetDefaults()
	{
		NPC.aiStyle = -1;
		NPC.lifeMax = 20;
		NPC.friendly = true;
		NPC.width = 42;
		NPC.height = 20;
		NPC.knockBackResist = 1;
		NPC.noGravity = true;

		NPC.HitSound = new($"{nameof(PathOfTerraria)}/Assets/Sounds/HitEffects/FleshHit", 3) { MaxInstances = 5, Volume = 0.4f };
		NPC.DeathSound = SoundID.NPCDeath23 with { Pitch = +0.1f, PitchVariance = 0.15f, Identifier = $"{Name}Death" };

		//NPC.TryEnableComponent<NPCAnimations>(e =>
		//{
		//	e.BaseFrame = new SpriteFrame(1, (byte)Main.npcFrameCount[Type]) with { PaddingX = 0, PaddingY = 0 };
		//	e.SpriteOffset = new(0f, 0f);
		//});
		NPC.TryEnableComponent<NPCVoice>(e =>
		{
			e.Data.PainSound = (3, SoundID.NPCHit56 with { Pitch = +0.3f, PitchVariance = 0.4f, Identifier = $"{Name}Hit" });
		});
		NPC.TryEnableComponent<NPCHitEffects>(c =>
		{
			c.AddGore(new($"{PoTMod.ModName}/{nameof(BloodSplatMedium)}", 1));

			c.AddGore(new($"{PoTMod.ModName}/{nameof(BloodSplatSmall)}", 2, NPCHitEffects.OnDeath));
			c.AddGore(new($"{PoTMod.ModName}/{nameof(BloodSplatMedium)}", 1, NPCHitEffects.OnDeath));
			c.AddGore(new($"{PoTMod.ModName}/{nameof(BloodSplatLarge)}", 1, NPCHitEffects.OnDeath));
			//c.AddGore(new($"{PoTMod.ModName}/{Name}_Head", +1, NPCHitEffects.OnDeath) { Position = (new(0, 0), new Vector2(-25, +13), new(3, 3)) });
			//c.AddGore(new($"{PoTMod.ModName}/{Name}_Body", +1, NPCHitEffects.OnDeath) { Position = (new(0, 0), new Vector2(-10, +01), new(3, 3)) });
			//c.AddGore(new($"{PoTMod.ModName}/{Name}_Mandible", +1, NPCHitEffects.OnDeath) { Position = (new(0, 0), new Vector2(+10, +32), new(3, 3)) });
			//c.AddGore(new($"{PoTMod.ModName}/{Name}_Mandible", +1, NPCHitEffects.OnDeath) { Position = (new(0, 0), new Vector2(-23, +32), new(3, 3)) });
			//c.AddGore(new($"{PoTMod.ModName}/{Name}_Wings", +1, NPCHitEffects.OnDeath) { Position = (new(0, 0), new Vector2(+05, -19), new(3, 3)) });
			//c.AddGore(new($"{PoTMod.ModName}/{Name}_Wings", +1, NPCHitEffects.OnDeath) { Position = (new(0, 0), new Vector2(-25, -23), new(3, 3)) });
			//c.AddGore(new($"{PoTMod.ModName}/{Name}_WingSmall", +1, NPCHitEffects.OnDeath) { Position = (new(0, 0), new Vector2(-09, -17), new(3, 3)) });
			//c.AddGore(new($"{PoTMod.ModName}/{Name}_WingSmall", +1, NPCHitEffects.OnDeath) { Position = (new(0, 0), new Vector2(+17, -18), new(3, 3)) });
			//c.AddGore(new($"{PoTMod.ModName}/{Name}_TailTip", +1, NPCHitEffects.OnDeath) { Position = (new(0, 0), new Vector2(+51, +15), new(3, 3)) });
			c.AddGore(new($"{PoTMod.ModName}/{nameof(BloodSplatSmall)}", 2, NPCHitEffects.OnDeath));
			c.AddGore(new($"{PoTMod.ModName}/{nameof(BloodSplatMedium)}", 1, NPCHitEffects.OnDeath));
			c.AddGore(new($"{PoTMod.ModName}/{nameof(BloodSplatLarge)}", 1, NPCHitEffects.OnDeath));
		});
	}

	public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
	{
		NPC.damage = ModeUtils.ByMode(5, 10, 20, 33);
	}

	public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
	{
		bestiaryEntry.AddInfo(this, "");
	}

	public override void FindFrame(int frameHeight)
	{
		//Context ctx = new(NPC);
		//ctx.Animations.Advance();
		//ctx.Animations.Set(PickAnimation(in ctx));
	}

	public override void AI()
	{
		Context ctx = new(NPC);
		Lighting.AddLight(NPC.position, new Vector3(0.9f, 0.75f, 0.95f));

		if (Math.Abs(NPC.velocity.X) > 0.01f)
		{
			NPC.direction = Math.Sign(NPC.velocity.X);
			NPC.spriteDirection = NPC.direction;
		}
		else
		{
			NPC.direction = NPC.spriteDirection;
		}

		Timer++;
		NPC.TargetClosest();
		Player target = Main.player[NPC.target];

		if (State == States.Idle)
		{
		}
	}

	// Placeholder for animations
	private SpriteAnimation? PickAnimation(in Context ctx)
	{
		Vector2 vel = NPC.position - NPC.oldPosition;

		//if (State == States.IdleFly && SwitchingToGrab)
		//{
			return animIdle;
		//}

		//return ctx.Animations.Current switch
		//{
		//	_ when State == States.ChaseFly || NPC.IsABestiaryIconDummy => animTryGrab with { Speed = 16 },
		//	_ when State == States.IdleFly => animFly,
		//	_ when State == States.CarryingPlayer && !HasLetGo => animCarry with { Speed = 12 + ShakeTimer / 5f },
		//	_ when State == States.CarryingPlayer && HasLetGo => animFly,
		//	_ => null,
		//};
	}
}

using NPCUtils;
using PathOfTerraria.Common.AI;
using PathOfTerraria.Common.NPCs;
using PathOfTerraria.Common.NPCs.Components;
using PathOfTerraria.Common.NPCs.Effects;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Content.Gores;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.ID;

namespace PathOfTerraria.Content.NPCs.Mapping.Swamp;

internal class Mosquito : ModNPC, IGrabberNPC
{
	private readonly struct Context(NPC npc)
	{
		public NPCTargeting Targeting { get; } = npc.GetGlobalNPC<NPCTargeting>();
		public NPCAnimations Animations { get; } = npc.GetGlobalNPC<NPCAnimations>();
	}

	private enum States
	{
		IdleFly,
		ChaseFly,
		CarryingPlayer
	}

	private static readonly SpriteAnimation animFly = new() { Id = "fly", Frames = [0, 1, 2, 3, 4, 5], Speed = 10 };
	private static readonly SpriteAnimation animCarry = new() { Id = "carry", Frames = [12, 13, 14, 15], Speed = 12 };

	private States State
	{
		get => (States)NPC.ai[0];
		set => NPC.ai[0] = (float)value;
	}

	private ref float Timer => ref NPC.ai[1];

	private Player CarryingPlayer => Main.player[CarryingPlayerWhoAmI];

	private int CarryingPlayerWhoAmI
	{
		get => (int)NPC.ai[2];
		set => NPC.ai[2] = value;
	}

	private bool HasLetGo
	{
		get => NPC.localAI[0] == 1;
		set => NPC.localAI[0] = value ? 1 : 0;
	}

	private ref float ShakeTimer => ref NPC.ai[3];

	void IGrabberNPC.OnGrab(int playerGrabbed)
	{
		CarryingPlayerWhoAmI = playerGrabbed;
	}

	Vector2 IGrabberNPC.GetGrabOffset(int playerGrabbed)
	{
		return new Vector2(NPC.direction == -1 ? -12 : 12, 40);
	}

	void IGrabberNPC.UpdateGrabbing(int playerGrabbed, float attemptedPlayerMoveMagnitude, bool grappling)
	{
		ShakeTimer += attemptedPlayerMoveMagnitude * (grappling ? 0.02f : 1f);
	}

	bool IGrabberNPC.LetGo()
	{
		if (ShakeTimer > 100f)
		{
			HasLetGo = true;
			return true;
		}

		return false;
	}

	public override void SetStaticDefaults()
	{
		NPCID.Sets.TeleportationImmune[Type] = true;
		NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Shimmer] = true;
		Main.npcFrameCount[Type] = 16;
	}

	public override void SetDefaults()
	{
		NPC.aiStyle = -1;
		NPC.lifeMax = 400;
		NPC.defense = 15;
		NPC.damage = 5;
		NPC.width = 142;
		NPC.height = 42;
		NPC.knockBackResist = 0;
		NPC.noGravity = true;
		NPC.hide = true;

		NPC.HitSound = new($"{nameof(PathOfTerraria)}/Assets/Sounds/HitEffects/FleshHit", 3) { MaxInstances = 5, Volume = 0.4f };
		NPC.DeathSound = SoundID.NPCDeath23 with { Pitch = +0.1f, PitchVariance = 0.15f, Identifier = "FallenDeath" };

		NPC.TryEnableComponent<NPCTargeting>();
		NPC.TryEnableComponent<NPCAnimations>(e =>
		{
			e.BaseFrame = new SpriteFrame(1, (byte)Main.npcFrameCount[Type]) with { PaddingX = 0, PaddingY = 0 };
			e.SpriteOffset = new(0f, 0f);
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
			c.AddGore(new($"{PoTMod.ModName}/{Name}_Head", +1, NPCHitEffects.OnDeath) { Position = (new(0, 0), new Vector2(-25, +13), new(3, 3)) });
			c.AddGore(new($"{PoTMod.ModName}/{Name}_Body", +1, NPCHitEffects.OnDeath) { Position = (new(0, 0), new Vector2(-10, +1), new(3, 3)) });
			//c.AddGore(new($"{PoTMod.ModName}/{Name}_2", +1, NPCHitEffects.OnDeath) { Position = (new(0, 0), new Vector2(+70, +32), new(3, 3)), FlipWithDirection = false, NoCentering = true });
			//c.AddGore(new($"{PoTMod.ModName}/{Name}_3", +1, NPCHitEffects.OnDeath) { Position = (new(0, 0), new Vector2(+68, +18), new(12, 3)), FlipWithDirection = false, NoCentering = true });
			//c.AddGore(new($"{PoTMod.ModName}/{Name}_4", +1, NPCHitEffects.OnDeath) { Position = (new(0, 0), new Vector2(+36, +32), new(3, 3)), FlipWithDirection = false, NoCentering = true });
			c.AddGore(new($"{PoTMod.ModName}/{nameof(BloodSplatSmall)}", 2, NPCHitEffects.OnDeath));
			c.AddGore(new($"{PoTMod.ModName}/{nameof(BloodSplatMedium)}", 1, NPCHitEffects.OnDeath));
			c.AddGore(new($"{PoTMod.ModName}/{nameof(BloodSplatLarge)}", 1, NPCHitEffects.OnDeath));
		});
	}

	public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
	{
		NPC.damage = ModeUtils.ByMode(5, 10, 20, 33);
	}

	public override void DrawBehind(int index)
	{
		Main.instance.DrawCacheNPCsOverPlayers.Add(index);
	}

	public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
	{
		bestiaryEntry.AddInfo(this, "");
	}

	public override void FindFrame(int frameHeight)
	{
		Context ctx = new(NPC);
		ctx.Animations.Advance();
		ctx.Animations.Set(PickAnimation(in ctx));
	}

	public override void AI()
	{
		Context ctx = new(NPC);
		ctx.Targeting.ManualUpdate(new(NPC));

		if (Math.Abs(NPC.velocity.X) > 0.01f)
		{
			NPC.direction = Math.Sign(NPC.velocity.X);
		}
		else
		{
			NPC.direction = NPC.spriteDirection;
		}

		Timer++;

		if (State == States.IdleFly)
		{
			if (Timer == 120)
			{
				Timer = 0;

				if (Main.netMode != NetmodeID.MultiplayerClient)
				{
					NPC.velocity = Main.rand.NextVector2CircularEdge(4, 4) * Main.rand.NextFloat(0.5f, 1f);
					NPC.netUpdate = true;
				}
			}

			NPC.velocity *= 0.967f;

			if (ctx.Targeting.GetTargetCenter(NPC).DistanceSQ(NPC.Center) < 500 * 500)
			{
				State = States.ChaseFly;
			}
		}
		else if (State == States.ChaseFly)
		{
			Vector2 targetCenter = ctx.Targeting.GetTargetCenter(NPC);
			NPC.velocity = Vector2.SmoothStep(NPC.velocity, NPC.DirectionTo(targetCenter) * 13, 0.1f);

			foreach (Player player in Main.ActivePlayers)
			{
				if (player.Hitbox.Intersects(NPC.Hitbox) && player.GetModPlayer<GrabbedPlayer>().BeingGrabbed == -1)
				{
					CarryingPlayerWhoAmI = player.whoAmI;
					State = States.CarryingPlayer;
					break;
				}
			}

			if (NPC.DistanceSQ(targetCenter) > 600 * 600)
			{
				State = States.IdleFly;
				Timer = 0;
			}
		}
		else if (State == States.CarryingPlayer)
		{
			if (!HasLetGo)
			{
				CarryingPlayer.GetModPlayer<GrabbedPlayer>().BeingGrabbed = NPC.whoAmI;

				NPC.velocity = Vector2.Lerp(NPC.velocity, NPC.DirectionTo(Main.MouseWorld) * 6, 0.1f);
				NPC.velocity *= 0.9f;
			}
			else
			{
				ShakeTimer--;

				NPC.velocity *= 0.95f;
			}

			if (CarryingPlayer.dead || (HasLetGo && ShakeTimer <= 0))
			{
				State = States.IdleFly;
				HasLetGo = false;
				Timer = 0;
			}
		}
	}

	private SpriteAnimation? PickAnimation(in Context ctx)
	{
		Vector2 vel = NPC.position - NPC.oldPosition;

		return ctx.Animations.Current switch
		{
			_ when State == States.ChaseFly || NPC.IsABestiaryIconDummy => animFly with { Speed = 16 },
			_ when State == States.IdleFly => animFly,
			_ when State == States.CarryingPlayer => animCarry,
			_ => null,
		};
	}
}

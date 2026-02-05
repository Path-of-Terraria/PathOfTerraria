using NPCUtils;
using PathOfTerraria.Common;
using PathOfTerraria.Common.AI;
using PathOfTerraria.Common.NPCs;
using PathOfTerraria.Common.NPCs.Components;
using PathOfTerraria.Common.NPCs.Effects;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Content.Buffs;
using PathOfTerraria.Content.Gores;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.ID;

namespace PathOfTerraria.Content.NPCs.Mapping.Swamp;

internal class DragonFly : ModNPC, IGrabberNPC
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

	private static readonly SpriteAnimation animFly = new() { Id = "fly", Frames = [0, 1, 2, 3], Speed = 10 };
	private static readonly SpriteAnimation animStartGrab = new() { Id = "startGrab", Frames = [4, 5, 6, 7], Speed = 10, Loop = false };
	private static readonly SpriteAnimation animTryGrab = new() { Id = "tryGrab", Frames = [8, 9, 10, 11], Speed = 10 };
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

	private bool SwitchingToGrab
	{
		get => NPC.localAI[1] == 1;
		set => NPC.localAI[1] = value ? 1 : 0;
	}

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
		NPC.lifeMax = 300;
		NPC.defense = 15;
		NPC.damage = 5;
		NPC.width = 142;
		NPC.height = 42;
		NPC.knockBackResist = 0;
		NPC.noGravity = true;
		NPC.hide = true;
		NPC.value = Item.buyPrice(0, 0, 20, 0);

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
			c.AddGore(new($"{PoTMod.ModName}/{Name}_Body", +1, NPCHitEffects.OnDeath) { Position = (new(0, 0), new Vector2(-10, +01), new(3, 3)) });
			c.AddGore(new($"{PoTMod.ModName}/{Name}_Mandible", +1, NPCHitEffects.OnDeath) { Position = (new(0, 0), new Vector2(+10, +32), new(3, 3)) });
			c.AddGore(new($"{PoTMod.ModName}/{Name}_Mandible", +1, NPCHitEffects.OnDeath) { Position = (new(0, 0), new Vector2(-23, +32), new(3, 3)) });
			c.AddGore(new($"{PoTMod.ModName}/{Name}_Wings", +1, NPCHitEffects.OnDeath) { Position = (new(0, 0), new Vector2(+05, -19), new(3, 3)) });
			c.AddGore(new($"{PoTMod.ModName}/{Name}_Wings", +1, NPCHitEffects.OnDeath) { Position = (new(0, 0), new Vector2(-25, -23), new(3, 3)) });
			c.AddGore(new($"{PoTMod.ModName}/{Name}_WingSmall", +1, NPCHitEffects.OnDeath) { Position = (new(0, 0), new Vector2(-09, -17), new(3, 3)) });
			c.AddGore(new($"{PoTMod.ModName}/{Name}_WingSmall", +1, NPCHitEffects.OnDeath) { Position = (new(0, 0), new Vector2(+17, -18), new(3, 3)) });
			c.AddGore(new($"{PoTMod.ModName}/{Name}_TailTip", +1, NPCHitEffects.OnDeath) { Position = (new(0, 0), new Vector2(+51, +15), new(3, 3)) });
			c.AddGore(new($"{PoTMod.ModName}/{nameof(BloodSplatSmall)}", 2, NPCHitEffects.OnDeath));
			c.AddGore(new($"{PoTMod.ModName}/{nameof(BloodSplatMedium)}", 1, NPCHitEffects.OnDeath));
			c.AddGore(new($"{PoTMod.ModName}/{nameof(BloodSplatLarge)}", 1, NPCHitEffects.OnDeath));
		});
	}

	public override bool CanHitPlayer(Player target, ref int cooldownSlot)
	{
		return false;
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
			IdleMovement();

			if (ctx.Targeting.GetTargetCenter(NPC).DistanceSQ(NPC.Center) < 500 * 500)
			{
				SwitchingToGrab = true;
			}

			if (SwitchingToGrab && ctx.Animations.Completed)
			{
				State = States.ChaseFly;
			}
		}
		else if (State == States.ChaseFly)
		{
			Vector2 targetCenter = ctx.Targeting.GetTargetCenter(NPC);
			NPC.velocity = Vector2.SmoothStep(NPC.velocity, NPC.SafeDirectionTo(targetCenter) * 13, 0.1f);

			foreach (Player player in Main.ActivePlayers)
			{
				if (player.Hitbox.Intersects(NPC.Hitbox) && player.GetModPlayer<GrabbedPlayer>().BeingGrabbed == -1 && !player.dead)
				{
					CarryingPlayerWhoAmI = player.whoAmI;
					State = States.CarryingPlayer;
					break;
				}
			}

			if (NPC.DistanceSQ(targetCenter) > 600 * 600)
			{
				State = States.IdleFly;
				SwitchingToGrab = false;
				Timer = 0;
			}
		}
		else if (State == States.CarryingPlayer)
		{
			if (CarryingPlayer.dead || !CarryingPlayer.active)
			{
				HasLetGo = true;
			}

			if (!HasLetGo)
			{
				CarryingPlayer.GetModPlayer<GrabbedPlayer>().BeingGrabbed = NPC.whoAmI;
				CarryingPlayer.AddBuff(ModContent.BuffType<MosquitoTrappedDebuff>(), 2);

				if (!Collision.SolidCollision(NPC.position - new Vector2(0, 120), NPC.width, 80) && NPC.position.Y > (Main.offLimitBorderTiles + 30) * 16) 
				{
					NPC.velocity = Vector2.SmoothStep(NPC.velocity, new Vector2(0, -8 + ShakeTimer / 25f), 0.1f);

					if (Timer >= 30 && Main.netMode != NetmodeID.MultiplayerClient)
					{
						Timer = 0;

						NPC.velocity.X = Main.rand.NextFloat(0.25f, 2) * NPC.spriteDirection;
					}
				}
				else
				{
					IdleMovement(true);

					if (NPC.velocity.Y < 0)
					{
						NPC.velocity.Y *= 0.9f;
					}
				}
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
				SwitchingToGrab = false;
			}
		}
	}

	private void IdleMovement(bool carrying = false)
	{
		if (Timer == (carrying ? 60 : 120))
		{
			Timer = 0;

			if (Main.netMode != NetmodeID.MultiplayerClient)
			{
				NPC.velocity = Main.rand.NextVector2CircularEdge(4, 4) * Main.rand.NextFloat(0.5f, 1f);
				NPC.netUpdate = true;

				if (carrying)
				{
					NPC.velocity.Y = MathF.Abs(NPC.velocity.Y);
					NPC.velocity *= Main.rand.NextFloat(1.25f, 2f);
				}
			}
		}

		NPC.velocity *= 0.967f;
	}

	private SpriteAnimation? PickAnimation(in Context ctx)
	{
		Vector2 vel = NPC.position - NPC.oldPosition;

		if (State == States.IdleFly && SwitchingToGrab)
		{
			return animStartGrab;
		}

		return ctx.Animations.Current switch
		{
			_ when State == States.ChaseFly || NPC.IsABestiaryIconDummy => animTryGrab with { Speed = 16 },
			_ when State == States.IdleFly => animFly,
			_ when State == States.CarryingPlayer && !HasLetGo => animCarry with { Speed = 12 + ShakeTimer / 5f },
			_ when State == States.CarryingPlayer && HasLetGo => animFly,
			_ => null,
		};
	}
}

using NPCUtils;
using PathOfTerraria.Common.AI;
using PathOfTerraria.Common.NPCs;
using PathOfTerraria.Common.NPCs.Components;
using PathOfTerraria.Common.NPCs.Effects;
using PathOfTerraria.Common.Systems.Synchronization;
using PathOfTerraria.Content.Gores;
using PathOfTerraria.Content.Projectiles.Utility;
using System.IO;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ID;

namespace PathOfTerraria.Content.NPCs.Mapping.Swamp;

internal class Mudsquit : ModNPC
{
	private readonly struct Context(NPC npc)
	{
		public NPCAnimations Animations { get; } = npc.GetGlobalNPC<NPCAnimations>();
		public NPCMovement Movement { get; } = npc.GetGlobalNPC<NPCMovement>();
	}

	public class ExplodeMudsquitHandler : Handler
	{
		public static void Send(NPC npc)
		{
			ModPacket packet = Networking.GetPacket<ExplodeMudsquitHandler>();
			packet.Write((byte)npc.whoAmI);
			packet.Send();
		}

		internal override void Receive(BinaryReader reader, byte sender)
		{
			byte who = reader.ReadByte();
			NPC npc = Main.npc[who];

			npc.ai[0] = 1;
			npc.ai[1] = 0;
			npc.netUpdate = true;
		}
	}

	private enum States
	{
		Chase,
		BloatAndExplode
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
		NPCID.Sets.TeleportationImmune[Type] = true;
		NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Shimmer] = true;
		Main.npcFrameCount[Type] = 1;
	}

	public override void SetDefaults()
	{
		NPC.aiStyle = -1;
		NPC.lifeMax = 300;
		NPC.defense = 15;
		NPC.damage = 5;
		NPC.width = 42;
		NPC.height = 20;
		NPC.knockBackResist = 1;
		NPC.noGravity = true;
		NPC.value = Item.buyPrice(0, 0, 2, 0);

		NPC.HitSound = new($"{nameof(PathOfTerraria)}/Assets/Sounds/HitEffects/FleshHit", 3) { MaxInstances = 5, Volume = 0.4f };
		NPC.DeathSound = SoundID.NPCDeath23 with { Pitch = +0.1f, PitchVariance = 0.15f, Identifier = "FallenDeath" };

		//NPC.TryEnableComponent<NPCAnimations>(e =>
		//{
		//	e.BaseFrame = new SpriteFrame(1, (byte)Main.npcFrameCount[Type]) with { PaddingX = 0, PaddingY = 0 };
		//	e.SpriteOffset = new(0f, 0f);
		//});
		NPC.TryEnableComponent<NPCMovement>(e =>
		{
			e.Data.Push = new() { RequiredNpcType = Type };
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
		ctx.Movement.ManualUpdate(new(NPC));

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

		if (State == States.Chase)
		{
			if (NPC.wet)
			{
				NPC.velocity = Vector2.SmoothStep(NPC.velocity, NPC.DirectionTo(target.Center) * 6, 0.15f);
			}
			else
			{
				NPC.velocity.Y += 0.3f;

				if (NPC.collideY)
				{
					NPC.velocity.X *= 0.8f;

					if (Timer > 120)
					{
						Timer = 0;
						NPC.velocity.Y = -4;

						if (Main.netMode != NetmodeID.MultiplayerClient)
						{
							NPC.velocity.X = Main.rand.NextFloat(-1, 1);
							NPC.netUpdate = true;
						}
					}
				}
			}
		}
		else
		{
			NPC.velocity *= 0.9f;

			if (Timer > 120)
			{
				NPC.active = false;

				var package = new ExplosionHitbox.VFXPackage(8, 30, 30, SmokeDustType: DustID.Blood, TorchDustType: DustID.RedTorch, DustVelocityModifier: 3f);
				int damage = ModeUtils.ProjectileDamage(120, 170, 230, 300);
				IEntitySource src = NPC.GetSource_Death();
				ExplosionHitbox.QuickSpawn(src, NPC, damage, Main.myPlayer, new Vector2(200, 200), ExplosionSpawnInfo.HostileSpawn, package);

				if (Main.netMode != NetmodeID.MultiplayerClient)
				{
					foreach (NPC npc in Main.ActiveNPCs)
					{
						if (npc.whoAmI != NPC.whoAmI && npc.type == Type && NPC.DistanceSQ(npc.Center) < 180 * 180 && npc.ai[0] != 1)
						{
							npc.ai[0] = 1;
							npc.ai[1] = 0;
							npc.netUpdate = true;
						}
					}
				}
			}

			if (!NPC.wet)
			{
				NPC.velocity.Y += 0.3f;
			}
		}
	}

	public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
	{
		if (State == States.BloatAndExplode)
		{
			return;
		}

		State = States.BloatAndExplode;
		Timer = 0;
		NPC.netUpdate = true;

		if (Main.netMode == NetmodeID.MultiplayerClient)
		{
			ExplodeMudsquitHandler.Send(NPC);
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

	public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
	{
		Texture2D tex = TextureAssets.Npc[Type].Value;
		Vector2 position = NPC.Center - screenPos;
		SpriteEffects effects = NPC.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
		float redFactor = 0f;

		if (State == States.BloatAndExplode)
		{
			redFactor = MathF.Abs(MathF.Sin(Timer * (0.15f + Timer * 0.001f)));
		}

		spriteBatch.Draw(tex, position, null, Color.Lerp(drawColor, new Color(255, 100, 100), redFactor), NPC.rotation, NPC.frame.Size() / 2f, 1f, effects, 0);
		return false;
	}
}

using NPCUtils;
using PathOfTerraria.Common.NPCs.Components;
using PathOfTerraria.Common.NPCs.Effects;
using PathOfTerraria.Content.Projectiles.Hostile;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.ID;

namespace PathOfTerraria.Content.NPCs.HellEvent;

public sealed class FireMaw : ModNPC
{
	public enum States : byte
	{
		Idle,
		Nip,
		Spit
	}

	private States State
	{
		get => (States)NPC.ai[0];
		set => NPC.ai[0] = (float)value;
	}

	private ref float Timer => ref NPC.ai[1];

	private bool HasLava
	{
		get => NPC.ai[2] == 1;
		set => NPC.ai[2] = value ? 1 : 0;
	}

	private ref float GotoY => ref NPC.ai[3];

	public override void SetStaticDefaults()
	{
		Main.npcFrameCount[Type] = 6;
	}

	public override void SetDefaults()
	{
		NPC.CloneDefaults(NPCID.Crimera);
		NPC.aiStyle = -1;
		NPC.Size = new Vector2(22, 28);
		NPC.damage = 60;
		NPC.defense = 5;
		NPC.value = Item.buyPrice(0, 0, 5, 0);
		NPC.knockBackResist = 0f;
		NPC.lifeMax = 120;
		NPC.lavaImmune = true;

		NPC.TryEnableComponent<NPCHitEffects>(c =>
		{
			c.AddDust(new(DustID.Ash, 4));
			c.AddDust(new(DustID.Ash, 15, NPCHitEffects.OnDeath));
		});
	}

	public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
	{
		bestiaryEntry.AddInfo(this, "TheUnderworld");
	}

	public override bool? CanFallThroughPlatforms()
	{
		return true;
	}

	public override void AI()
	{
		switch (State)
		{
			case States.Idle:
				IdleState();
				break;

			case States.Nip:
				NipState();
				break;

			case States.Spit:
				Timer++;

				if (Timer == 15)
				{
					for (int i = 0; i < 30; ++i)
					{
						int dustType = Utils.SelectRandom(Main.rand, 6, 259, 158);
						Dust.NewDustPerfect(NPC.Center, dustType, new Vector2(Main.rand.NextFloat(-2, 2), Main.rand.NextFloat(2, 6)));
					}

					Point16 pos = NPC.Bottom.ToTileCoordinates16();
					WorldGen.PlaceLiquid(pos.X, pos.Y, (byte)LiquidID.Lava, 255);

					HasLava = false;
				}
				else if (Timer == 30)
				{
					Timer = 0;
					State = States.Idle;
				}

				break;
		}
	}

	private void NipState()
	{
		if (GotoY > -1)
		{
			Timer++;
			NPC.velocity.Y = 10;

			if (Timer > 6 && (Collision.SolidCollision(NPC.position, NPC.width, NPC.height + 2) || NPC.lavaWet))
			{
				GotoY = -30;

				if (NPC.lavaWet)
				{
					HasLava = true;
				}
			}
		}
		else
		{
			NPC.velocity.Y = 0;
			GotoY++;

			if (GotoY == -1)
			{
				State = States.Idle;
				Timer = 0;
			}
		}
	}

	private void IdleState()
	{
		Timer++;

		NPC.velocity.Y -= 0.1f;

		if (Timer > 60 && Collision.SolidCollision(NPC.Top - new Vector2(0, 2), NPC.width, 6))
		{
			if (HasLava)
			{
				State = States.Spit;
				Timer = 0;
			}
			else
			{
				foreach (Player player in Main.ActivePlayers)
				{
					if (Math.Abs(player.Center.X - NPC.Center.X) < 30 && player.Center.Y > NPC.Center.Y && Collision.CanHit(NPC, player))
					{
						GotoY = player.Center.Y;
						State = States.Nip;
						Timer = 0;
						break;
					}
				}
			}
		}
	}

	public override bool CanHitPlayer(Player target, ref int cooldownSlot)
	{
		return State == States.Nip && NPC.velocity.Y > 0;
	}

	public override void FindFrame(int frameHeight)
	{
		switch (State)
		{
			case States.Idle:
				NPC.frameCounter = 0;
				NPC.frame.Y = 0;
				break;

			case States.Nip:
				if (NPC.velocity.Y > 0)
				{
					NPC.frameCounter++;
					NPC.frame.Y = frameHeight * (int)Math.Min(NPC.frameCounter / 5, 2);
				}
				else
				{
					if (NPC.frameCounter > 10)
					{
						NPC.frameCounter = 10;
					}

					NPC.frameCounter--;
					NPC.frame.Y = frameHeight * (int)Math.Max(NPC.frameCounter / 5, 0);
				}

				break;

			case States.Spit:
				if (Timer < 15)
				{
					NPC.frameCounter++;
					NPC.frame.Y = frameHeight * (int)Math.Min(NPC.frameCounter / 5, 2);
				}
				else
				{
					if (NPC.frameCounter > 10)
					{
						NPC.frameCounter = 10;
					}

					NPC.frameCounter--;
					NPC.frame.Y = frameHeight * (int)Math.Max(NPC.frameCounter / 5, 0);
				}

				break;
		}

		if (!HasLava)
		{
			NPC.frame.Y += frameHeight * 3;
		}
	}

	public override bool? CanBeHitByProjectile(Projectile projectile)
	{
		return projectile.type == ProjectileID.GeyserTrap || projectile.ModProjectile is FallingAshBlock ? false : null;
	}
}
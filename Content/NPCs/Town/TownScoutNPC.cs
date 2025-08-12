using NPCUtils;
using PathOfTerraria.Common.NPCs.Components;
using PathOfTerraria.Common.NPCs.Effects;
using PathOfTerraria.Common.Subworlds;
using PathOfTerraria.Common.Subworlds.RavencrestContent;
using SubworldLibrary;
using Terraria.GameContent.Bestiary;
using Terraria.ID;

namespace PathOfTerraria.Content.NPCs.Town;

public sealed class TownScoutNPC : ModNPC
{
	private bool HasPlayerBeenNear
	{
		get => NPC.ai[0] == 1;
		set => NPC.ai[0] = value ? 1 : 0;
	}

	public override void SetStaticDefaults()
	{
		Main.npcFrameCount[Type] = 16;
	}

	public override void SetDefaults()
	{
		NPC.CloneDefaults(NPCID.GoblinScout);

		AnimationType = NPCID.GoblinScout;
		AIType = NPCID.GoblinScout;

		NPC.TryEnableComponent<NPCHitEffects>(c =>
		{
			c.AddDust(new(DustID.Blood, 4));
			c.AddDust(new(DustID.Blood, 15, NPCHitEffects.OnDeath));
		});
	}

	public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
	{
		bestiaryEntry.AddInfo(this, "Surface");
	}

	public override bool PreAI()
	{
		if (!HasPlayerBeenNear)
		{
			foreach (Player plr in Main.ActivePlayers)
			{
				if (plr.DistanceSQ(NPC.Center) < 400 * 400)
				{
					HasPlayerBeenNear = true;
					break;
				}
			}

			if (NPC.life < NPC.lifeMax)
			{
				HasPlayerBeenNear = true;
			}

			if (Math.Abs(NPC.Center.X / 16 - Main.maxTilesX / 2) > 220)
			{
				NPC.velocity.X = NPC.Center.X / 16 >= Main.maxTilesX / 2 ? -2 : 2;
			}
			else
			{
				NPC.velocity.X = 0;
			}
		}
		else
		{
			float target = NPC.Center.X < Main.maxTilesX * 8 ? -4 : 4;
			NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, target, 0.03f);
		}

		NPC.direction = NPC.spriteDirection = Math.Sign(NPC.velocity.X);
		ModContent.GetInstance<RavencrestSystem>().SpawnedScout = true;

		Collision.StepUp(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.stepSpeed, ref NPC.gfxOffY);

		if (NPC.velocity.Y != 0)
		{
			return false;
		}

		if (NPC.velocity.X < 0)
		{
			if (Collision.SolidCollision(NPC.position - new Vector2(6, 0), 6, 16))
			{
				NPC.velocity.Y = -6;
			}
		}
		else
		{
			if (Collision.SolidCollision(NPC.TopRight, 6, 16))
			{
				NPC.velocity.Y = -6;
			}
		}

		return false;
	}

	public override void OnKill()
	{
		Main.invasionSize = 20;
		Main.StartInvasion(InvasionID.GoblinArmy);
	}

	public override float SpawnChance(NPCSpawnInfo spawnInfo)
	{
		bool anyHealthyPlayer = true;

		foreach (Player plr in Main.ActivePlayers)
		{
			if (plr.ConsumedLifeCrystals > 5)
			{
				anyHealthyPlayer = true;
				break;
			}
		}

		if (!anyHealthyPlayer)
		{
			return 0;
		}
		
		float chance = NPC.downedGoblins ? 0.1f : 5;
		bool spawnedScout = ModContent.GetInstance<RavencrestSystem>().SpawnedScout;

		return SubworldSystem.Current is RavencrestSubworld && NPC.downedSlimeKing 
			&& (spawnInfo.SpawnTileX < 180 || spawnInfo.SpawnTileX > Main.maxTilesX - 180) && !spawnedScout ? chance : 0;
	}

	public override bool CheckActive()
	{
		return false;
	}
}
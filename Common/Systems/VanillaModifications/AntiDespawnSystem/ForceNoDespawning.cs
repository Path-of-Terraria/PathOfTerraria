using PathOfTerraria.Common.Subworlds;
using PathOfTerraria.Utilities;
using SubworldLibrary;
using System.Reflection;
using Terraria.ID;

namespace PathOfTerraria.Common.Systems.VanillaModifications.AntiDespawnSystem;

internal class ForceNoDespawning : GlobalNPC
{
	public static bool AnyPlayerIsAlive = false;
	public static int OldLife = 0;

	public override void Load()
	{
		ILUtils.EmitILDetour(typeof(Main).GetMethod("Update", BindingFlags.NonPublic | BindingFlags.Instance), SetPlayersAlive, null);
		ILUtils.EmitILDetour(typeof(NPC).GetMethod("UpdateNPC"), CacheLife, ForceSetNPCAlive);
		
		//On_Main.Update += EarlyUnsetPlayerFlag;
		//On_NPC.UpdateNPC += StopDespawnsByForce;
	}

	public static void CacheLife(NPC self, int i)
	{
		OldLife = self.life;
	}

	public static void ForceSetNPCAlive(NPC self, int i)
	{
		int life = OldLife;

		// Aggressively stop despawning by forcing bosses that have health and were active to continue to be active, if we're in a boss domain and any player is alive
		// Note - excludes many multi-segment or nonstandard NPCs, which caused a crazy bug that spammed exp and item drops infinitely
		if ((self.boss || NPCID.Sets.ShouldBeCountedAsBoss[self.type]) && SubworldSystem.Current is BossDomainSubworld && !self.active && life > 0 && AnyPlayerIsAlive
			&& !(self.type is NPCID.EaterofWorldsHead or NPCID.EaterofWorldsTail or NPCID.MoonLordCore or NPCID.MoonLordFreeEye or NPCID.MoonLordHead or NPCID.MoonLordHand
			or NPCID.LunarTowerVortex or NPCID.LunarTowerStardust or NPCID.LunarTowerSolar or NPCID.LunarTowerNebula))
		{
			self.active = true;
			self.life = life;
		}
	}

	public static void SetPlayersAlive(Main self, GameTime gameTime)
	{
		AnyPlayerIsAlive = false;

		foreach (Player player in Main.ActivePlayers)
		{
			if (!player.dead && !player.ghost)
			{
				AnyPlayerIsAlive = true;
				break;
			}
		}
	}

	//private void EarlyUnsetPlayerFlag(On_Main.orig_Update orig, Main self, GameTime gameTime)
	//{
	//	AnyPlayerIsAlive = false;

	//	foreach (Player player in Main.ActivePlayers)
	//	{
	//		if (!player.dead && !player.ghost)
	//		{
	//			AnyPlayerIsAlive = true;
	//			break;
	//		}
	//	}

	//	orig(self, gameTime);
	//}

	private void StopDespawnsByForce(On_NPC.orig_UpdateNPC orig, NPC self, int i)
	{
		int life = self.life;

		orig(self, i);

		// Aggressively stop despawning by forcing bosses that have health and were active to continue to be active, if we're in a boss domain and any player is alive
		// Note - excludes many multi-segment or nonstandard NPCs, which caused a crazy bug that spammed exp and item drops infinitely
		if ((self.boss || NPCID.Sets.ShouldBeCountedAsBoss[self.type]) && SubworldSystem.Current is BossDomainSubworld && !self.active && life > 0 && AnyPlayerIsAlive
			&& !(self.type is NPCID.EaterofWorldsHead or NPCID.EaterofWorldsTail or NPCID.MoonLordCore or NPCID.MoonLordFreeEye or NPCID.MoonLordHead or NPCID.MoonLordHand 
			or NPCID.LunarTowerVortex or NPCID.LunarTowerStardust or NPCID.LunarTowerSolar or NPCID.LunarTowerNebula))
		{
			self.active = true;
			self.life = life;
		}
	}

	public override bool PreAI(NPC npc)
	{
		if (npc.type == NPCID.BrainofCthulhu)
		{
			// Stop BoC from doing player too distant/targets dead behaviour
			npc.localAI[3] = 0;
		}
		else if (npc.type == NPCID.HallowBoss)
		{
			if (npc.ai[1] == 13) // Stops EoL from despawning from players being distant or dead
			{
				npc.alpha = 0;
				npc.ai[1] = 20;
			}
		}
		else if (npc.type == NPCID.SkeletronHead)
		{
			if (npc.ai[1] > 1) // Stops Skeletron from entering too distant/targets dead behaviour & forces it closer
			{
				npc.ai[1] = 0;
				npc.velocity = npc.DirectionTo(Main.player[npc.target].Center) * 8;
			}
		}

		return true;
	}
}

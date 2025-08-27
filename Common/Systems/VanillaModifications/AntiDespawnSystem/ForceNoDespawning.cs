using PathOfTerraria.Common.Subworlds;
using SubworldLibrary;
using Terraria.ID;

namespace PathOfTerraria.Common.Systems.VanillaModifications.AntiDespawnSystem;

internal class ForceNoDespawning : GlobalNPC
{
	public static bool AnyPlayerIsAlive = false;

	public override void Load()
	{
		On_Main.Update += EarlyUnsetPlayerFlag;
		On_NPC.UpdateNPC += StopDespawnsByForce;
	}

	private void EarlyUnsetPlayerFlag(On_Main.orig_Update orig, Main self, GameTime gameTime)
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

		orig(self, gameTime);
	}

	private void StopDespawnsByForce(On_NPC.orig_UpdateNPC orig, NPC self, int i)
	{
		int life = self.life;
		int target = self.target;

		orig(self, i);

		// Aggressively stop despawning by forcing bosses that have health and were active to continue to be active, if we're in a boss domain and any player is alive
		if ((self.boss || NPCID.Sets.ShouldBeCountedAsBoss[self.type]) && SubworldSystem.Current is BossDomainSubworld && !self.active && life > 0 && AnyPlayerIsAlive)
		{
			self.active = true;
			self.life = life;
		}
	}

	public override bool PreAI(NPC npc)
	{
		if (npc.type == NPCID.BrainofCthulhu)
		{
			// Stop BoC from doing player too distant/dead behaviour
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

		return true;
	}
}

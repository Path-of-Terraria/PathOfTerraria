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

		if (self.boss && SubworldSystem.Current is MappingWorld && !self.active && life > 0 && AnyPlayerIsAlive)
		{
			self.active = true;
			self.life = life;
		}
	}

	public override bool PreAI(NPC npc)
	{
		if (npc.type == NPCID.BrainofCthulhu)
		{
			npc.localAI[3] = 0;
		}

		return true;
	}
}

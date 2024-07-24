using Terraria.ID;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Core.Systems;

internal class BossTracker : ModSystem
{
	public static bool DownedEaterOfWorlds = false;
	public static bool DownedBrainOfCthulhu = false;

	public override void SaveWorldData(TagCompound tag)
	{
		tag.Add(nameof(DownedEaterOfWorlds), DownedEaterOfWorlds);
		tag.Add(nameof(DownedBrainOfCthulhu), DownedBrainOfCthulhu);
	}

	public override void LoadWorldData(TagCompound tag)
	{
		DownedEaterOfWorlds = tag.GetBool(nameof(DownedEaterOfWorlds));
		DownedBrainOfCthulhu = tag.GetBool(nameof(DownedBrainOfCthulhu));
	}

	public class BossTrackerNPC : GlobalNPC
	{
		public override void OnKill(NPC npc)
		{
			if (npc.type == NPCID.EaterofWorldsHead && NPC.CountNPCS(NPCID.EaterofWorldsBody) == 0)
			{
				// EoW is dumb and won't register as a boss before death so this is the workaround
				DownedEaterOfWorlds = true;
			}

			if (!npc.boss)
			{
				return;
			}

			if (npc.type == NPCID.BrainofCthulhu)
			{
				DownedBrainOfCthulhu = true;
			}
		}
	}
}

using SubworldLibrary;
using Terraria.ID;

namespace PathOfTerraria.Common.Subworlds.BossDomains.SkeleDomain;

internal class SpikeballNPC : GlobalNPC
{
	public override bool InstancePerEntity => true;

	public float? Length = null;
	public float? Direction = null;

	public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
	{
		return entity.type == NPCID.SpikeBall;
	}

	public override bool CheckActive(NPC npc)
	{
		return SubworldSystem.Current is not SkeletronDomain;
	}

	public override bool PreAI(NPC npc)
	{
		if (Direction is not null)
		{
			npc.ai[0] = Direction.Value;
		}

		if (Length is not null)
		{
			npc.ai[3] = Length.Value;
		}

		return true;
	}
}

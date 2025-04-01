using PathOfTerraria.Content.NPCs.Mapping.Forest;
using Terraria.ID;
using Terraria.Utilities;

namespace PathOfTerraria.Common.Subworlds;

internal class ShrineMobPools
{
	public static WeightedRandom<int> GetPool(int style)
	{
		return style switch
		{
			_ => ForestNPCs()
		};
	}

	public static WeightedRandom<int> ForestNPCs()
	{
		WeightedRandom<int> random = new();
		random.Add(ModContent.NPCType<Ent>(), 0.05f);
		random.Add(ModContent.NPCType<ClumsyEntling>(), 0.25f);
		random.Add(ModContent.NPCType<Entling>(), 0.25f);
		random.Add(ModContent.NPCType<EntlingAlt>(), 0.25f);
		random.Add(NPCID.Wraith, 0.4f);
		random.Add(NPCID.Raven, 0.3f);
		random.Add(NPCID.PossessedArmor, 0.2f);
		return random;
	}
}

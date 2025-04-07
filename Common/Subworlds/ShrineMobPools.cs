using PathOfTerraria.Content.NPCs.Mapping.Desert;
using PathOfTerraria.Content.NPCs.Mapping.Forest;
using PathOfTerraria.Content.Tiles.Maps;
using Terraria.ID;
using Terraria.Utilities;

namespace PathOfTerraria.Common.Subworlds;

internal class ShrineMobPools
{
	public static WeightedRandom<int> GetPool(ShrineType style)
	{
		return style switch
		{
			ShrineType.Desert => DesertNPCs(),
			_ => ForestNPCs()
		};
	}

	private static WeightedRandom<int> DesertNPCs()
	{
		WeightedRandom<int> random = new();
		random.Add(ModContent.NPCType<ScarabSwarmController>(), 0.2f);
		random.Add(ModContent.NPCType<HauntedHead>(), 0.4f);
		random.Add(NPCID.Mummy, 0.8f);
		random.Add(NPCID.DesertGhoul, 0.6f);
		random.Add(NPCID.DesertDjinn, 0.4f);
		return random;
	}

	private static WeightedRandom<int> ForestNPCs()
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

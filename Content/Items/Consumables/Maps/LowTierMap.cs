using PathOfTerraria.Core.Subworlds;
using SubworldLibrary;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Consumables.Maps;

public class LowTierMap : Map
{
	public LowTierMap(int tier) {
		Tier = tier;
	}
	
	public override void SetDefaults()
	{
		Item.rare = ItemRarityID.Green;
	}

	public override void ModifyItemLoot(ItemLoot itemLoot)
	{
	}

	public override bool? UseItem(Player player)
	{
		SubworldSystem.Enter<TestSubworld>();
		return true;
	}
}
using PathOfTerraria.Core.Subworlds;
using SubworldLibrary;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Consumables.Maps;

internal class LowTierMap : Map
{
	public override void SetDefaults()
	{
		base.SetDefaults();
		Item.value = 1000;
	}
	
	public override string GenerateName()
	{
		return "Low Tier Map";
	}
	
	public override bool? UseItem(Player player)
	{
		SubworldSystem.Enter<TestSubworld>();
		return true;
	}
}
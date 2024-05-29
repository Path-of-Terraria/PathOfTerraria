using PathOfTerraria.Core.Subworlds;
using PathOfTerraria.Core.Systems;
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
}
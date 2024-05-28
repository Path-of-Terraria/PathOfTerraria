using System.Collections.Generic;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Consumables.Maps;

public abstract class Map : ModItem
{
	public override string Texture => $"{PathOfTerraria.ModName}/Assets/Items/Consumables/Maps/Map";
	protected int Tier = 0;
	private List<MapAffix> _affixes = [];

	public override void SetDefaults()
	{
		Item.maxStack = Item.CommonMaxStack;
		Item.consumable = true;
		Item.width = 32;
		Item.height = 32;
		Item.rare = ItemRarityID.White;
	}
}
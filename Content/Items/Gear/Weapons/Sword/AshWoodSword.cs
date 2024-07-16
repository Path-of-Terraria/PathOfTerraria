using PathOfTerraria.Content.Items.Gear.VanillaItems;
using PathOfTerraria.Core;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Sword;

internal class AshWoodSword : VanillaClone
{
	protected override short VanillaItemId => ItemID.AshWoodSword;
	
	public override void Defaults()
	{
		ItemType = ItemType.Sword;
		Rarity = Rarity.Magic; //Showing we can force certain rarities
		base.Defaults();
	}
	
	public override void AddRecipes()
	{
		CloneRecipes(VanillaItemId,  ModContent.ItemType<AshWoodSword>());
	}
}
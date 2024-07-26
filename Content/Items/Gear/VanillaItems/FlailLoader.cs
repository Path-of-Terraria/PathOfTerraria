using PathOfTerraria.Core;
using PathOfTerraria.Core.Items;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.VanillaItems.Clones;

internal class FlailLoader : ModSystem
{
	public override void Load()
	{
		// Prehardmode
		ItemDatabase.RegisterUniqueVanillaItem(ItemID.Mace, ItemType.Melee);
		ItemDatabase.RegisterUniqueVanillaItem(ItemID.FlamingMace, ItemType.Melee);
		ItemDatabase.RegisterUniqueVanillaItem(ItemID.BallOHurt, ItemType.Melee);
		ItemDatabase.RegisterUniqueVanillaItem(ItemID.TheMeatball, ItemType.Melee);
		ItemDatabase.RegisterUniqueVanillaItem(ItemID.BlueMoon, ItemType.Melee);
		ItemDatabase.RegisterUniqueVanillaItem(ItemID.Sunfury, ItemType.Melee);
		ItemDatabase.RegisterUniqueVanillaItem(ItemID.ChainKnife, ItemType.Melee);

		// Hardmode
		ItemDatabase.RegisterUniqueVanillaItem(ItemID.DripplerFlail, ItemType.Ranged);
		ItemDatabase.RegisterUniqueVanillaItem(ItemID.DaoofPow, ItemType.Ranged);
		ItemDatabase.RegisterUniqueVanillaItem(ItemID.FlowerPow, ItemType.Ranged);
		ItemDatabase.RegisterUniqueVanillaItem(ItemID.Anchor, ItemType.Ranged);
		ItemDatabase.RegisterUniqueVanillaItem(ItemID.ChainGuillotines, ItemType.Ranged);
		ItemDatabase.RegisterUniqueVanillaItem(ItemID.Flairon, ItemType.Ranged);
	}
}

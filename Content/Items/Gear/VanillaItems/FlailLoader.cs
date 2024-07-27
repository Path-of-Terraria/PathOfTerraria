using PathOfTerraria.Common.Enums;
using PathOfTerraria.Core;
using PathOfTerraria.Core.Items;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.VanillaItems;

internal class FlailLoader : ModSystem
{
	public override void Load()
	{
		ItemDatabase.RegisterVanillaItem(ItemID.GolemFist, ItemType.MeleeFlail);
		ItemDatabase.RegisterVanillaItem(ItemID.KOCannon, ItemType.MeleeFlail);

		// Prehardmode
		ItemDatabase.RegisterUniqueVanillaItem(ItemID.Mace, ItemType.MeleeFlail);
		ItemDatabase.RegisterUniqueVanillaItem(ItemID.FlamingMace, ItemType.MeleeFlail);
		ItemDatabase.RegisterUniqueVanillaItem(ItemID.BallOHurt, ItemType.MeleeFlail);
		ItemDatabase.RegisterUniqueVanillaItem(ItemID.TheMeatball, ItemType.MeleeFlail);
		ItemDatabase.RegisterUniqueVanillaItem(ItemID.BlueMoon, ItemType.MeleeFlail);
		ItemDatabase.RegisterUniqueVanillaItem(ItemID.Sunfury, ItemType.MeleeFlail);
		ItemDatabase.RegisterUniqueVanillaItem(ItemID.ChainKnife, ItemType.MeleeFlail);

		// Hardmode
		ItemDatabase.RegisterUniqueVanillaItem(ItemID.DripplerFlail, ItemType.RangedFlail);
		ItemDatabase.RegisterUniqueVanillaItem(ItemID.DaoofPow, ItemType.RangedFlail);
		ItemDatabase.RegisterUniqueVanillaItem(ItemID.FlowerPow, ItemType.RangedFlail);
		ItemDatabase.RegisterUniqueVanillaItem(ItemID.Anchor, ItemType.RangedFlail);
		ItemDatabase.RegisterUniqueVanillaItem(ItemID.ChainGuillotines, ItemType.RangedFlail);
		ItemDatabase.RegisterUniqueVanillaItem(ItemID.Flairon, ItemType.RangedFlail);
	}
}

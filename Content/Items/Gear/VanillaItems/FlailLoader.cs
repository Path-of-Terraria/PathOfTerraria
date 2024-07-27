using PathOfTerraria.Common.Enums;
using PathOfTerraria.Core.Items;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.VanillaItems;

internal class FlailLoader : ModSystem
{
	public override void Load()
	{
		ItemDatabase.RegisterVanillaItemAsGear(ItemID.GolemFist, ItemType.MeleeFlail);
		ItemDatabase.RegisterVanillaItemAsGear(ItemID.KOCannon, ItemType.MeleeFlail);

		// Prehardmode
		ItemDatabase.RegisterUniqueVanillaItemAsGear(ItemID.Mace, ItemType.MeleeFlail);
		ItemDatabase.RegisterUniqueVanillaItemAsGear(ItemID.FlamingMace, ItemType.MeleeFlail);
		ItemDatabase.RegisterUniqueVanillaItemAsGear(ItemID.BallOHurt, ItemType.MeleeFlail);
		ItemDatabase.RegisterUniqueVanillaItemAsGear(ItemID.TheMeatball, ItemType.MeleeFlail);
		ItemDatabase.RegisterUniqueVanillaItemAsGear(ItemID.BlueMoon, ItemType.MeleeFlail);
		ItemDatabase.RegisterUniqueVanillaItemAsGear(ItemID.Sunfury, ItemType.MeleeFlail);
		ItemDatabase.RegisterUniqueVanillaItemAsGear(ItemID.ChainKnife, ItemType.MeleeFlail);

		// Hardmode
		ItemDatabase.RegisterUniqueVanillaItemAsGear(ItemID.DripplerFlail, ItemType.RangedFlail);
		ItemDatabase.RegisterUniqueVanillaItemAsGear(ItemID.DaoofPow, ItemType.RangedFlail);
		ItemDatabase.RegisterUniqueVanillaItemAsGear(ItemID.FlowerPow, ItemType.RangedFlail);
		ItemDatabase.RegisterUniqueVanillaItemAsGear(ItemID.Anchor, ItemType.RangedFlail);
		ItemDatabase.RegisterUniqueVanillaItemAsGear(ItemID.ChainGuillotines, ItemType.RangedFlail);
		ItemDatabase.RegisterUniqueVanillaItemAsGear(ItemID.Flairon, ItemType.RangedFlail);
	}
}

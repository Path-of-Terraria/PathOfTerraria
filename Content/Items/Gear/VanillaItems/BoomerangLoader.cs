using PathOfTerraria.Core;
using PathOfTerraria.Core.Items;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.VanillaItems;

internal class BoomerangLoader : ModSystem
{
	public override void Load()
	{
		// Prehardmode
		ItemDatabase.RegisterUniqueVanillaItem(ItemID.EnchantedBoomerang, ItemType.Boomerang);
		ItemDatabase.RegisterUniqueVanillaItem(ItemID.FruitcakeChakram, ItemType.Boomerang);
		ItemDatabase.RegisterUniqueVanillaItem(ItemID.BloodyMachete, ItemType.Boomerang);
		ItemDatabase.RegisterUniqueVanillaItem(ItemID.Shroomerang, ItemType.Boomerang);
		ItemDatabase.RegisterUniqueVanillaItem(ItemID.IceBoomerang, ItemType.Boomerang);
		ItemDatabase.RegisterUniqueVanillaItem(ItemID.ThornChakram, ItemType.Boomerang);
		ItemDatabase.RegisterUniqueVanillaItem(ItemID.CombatWrench, ItemType.Boomerang);
		ItemDatabase.RegisterUniqueVanillaItem(ItemID.Trimarang, ItemType.Boomerang);
		ItemDatabase.RegisterUniqueVanillaItem(ItemID.Flamarang, ItemType.Boomerang);

		// Hardmode
		ItemDatabase.RegisterUniqueVanillaItem(ItemID.FlyingKnife, ItemType.Boomerang);
		ItemDatabase.RegisterUniqueVanillaItem(ItemID.BouncingShield, ItemType.Boomerang);
		ItemDatabase.RegisterUniqueVanillaItem(ItemID.LightDisc, ItemType.Boomerang);
		ItemDatabase.RegisterUniqueVanillaItem(ItemID.Bananarang, ItemType.Boomerang);
		ItemDatabase.RegisterUniqueVanillaItem(ItemID.PossessedHatchet, ItemType.Boomerang);
		ItemDatabase.RegisterUniqueVanillaItem(ItemID.PaladinsHammer, ItemType.Boomerang);
	}
}

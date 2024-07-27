using PathOfTerraria.Common.Enums;
using PathOfTerraria.Core.Items;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.VanillaItems;

internal class BoomerangLoader : ModSystem
{
	public override void Load()
	{
		// Prehardmode
		ItemDatabase.RegisterUniqueVanillaItemAsGear(ItemID.EnchantedBoomerang, ItemType.Boomerang);
		ItemDatabase.RegisterUniqueVanillaItemAsGear(ItemID.FruitcakeChakram, ItemType.Boomerang);
		ItemDatabase.RegisterUniqueVanillaItemAsGear(ItemID.BloodyMachete, ItemType.Boomerang);
		ItemDatabase.RegisterUniqueVanillaItemAsGear(ItemID.Shroomerang, ItemType.Boomerang);
		ItemDatabase.RegisterUniqueVanillaItemAsGear(ItemID.IceBoomerang, ItemType.Boomerang);
		ItemDatabase.RegisterUniqueVanillaItemAsGear(ItemID.ThornChakram, ItemType.Boomerang);
		ItemDatabase.RegisterUniqueVanillaItemAsGear(ItemID.CombatWrench, ItemType.Boomerang);
		ItemDatabase.RegisterUniqueVanillaItemAsGear(ItemID.Trimarang, ItemType.Boomerang);
		ItemDatabase.RegisterUniqueVanillaItemAsGear(ItemID.Flamarang, ItemType.Boomerang);

		// Hardmode
		ItemDatabase.RegisterUniqueVanillaItemAsGear(ItemID.FlyingKnife, ItemType.Boomerang);
		ItemDatabase.RegisterUniqueVanillaItemAsGear(ItemID.BouncingShield, ItemType.Boomerang);
		ItemDatabase.RegisterUniqueVanillaItemAsGear(ItemID.LightDisc, ItemType.Boomerang);
		ItemDatabase.RegisterUniqueVanillaItemAsGear(ItemID.Bananarang, ItemType.Boomerang);
		ItemDatabase.RegisterUniqueVanillaItemAsGear(ItemID.PossessedHatchet, ItemType.Boomerang);
		ItemDatabase.RegisterUniqueVanillaItemAsGear(ItemID.PaladinsHammer, ItemType.Boomerang);
	}
}

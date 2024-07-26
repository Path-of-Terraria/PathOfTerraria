using PathOfTerraria.Core;
using PathOfTerraria.Core.Items;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.VanillaItems.Clones;

internal class BoomerangLoader : ModSystem
{
	public override void Load()
	{
		// Prehardmode
		ItemDatabase.RegisterUniqueVanillaItem(ItemID.EnchantedBoomerang, ItemType.Ranged);
		ItemDatabase.RegisterUniqueVanillaItem(ItemID.FruitcakeChakram, ItemType.Ranged);
		ItemDatabase.RegisterUniqueVanillaItem(ItemID.BloodyMachete, ItemType.Ranged);
		ItemDatabase.RegisterUniqueVanillaItem(ItemID.Shroomerang, ItemType.Ranged);
		ItemDatabase.RegisterUniqueVanillaItem(ItemID.IceBoomerang, ItemType.Ranged);
		ItemDatabase.RegisterUniqueVanillaItem(ItemID.ThornChakram, ItemType.Ranged);
		ItemDatabase.RegisterUniqueVanillaItem(ItemID.CombatWrench, ItemType.Ranged);
		ItemDatabase.RegisterUniqueVanillaItem(ItemID.Trimarang, ItemType.Ranged);
		ItemDatabase.RegisterUniqueVanillaItem(ItemID.Flamarang, ItemType.Ranged);

		// Hardmode
		ItemDatabase.RegisterUniqueVanillaItem(ItemID.FlyingKnife, ItemType.Ranged);
		ItemDatabase.RegisterUniqueVanillaItem(ItemID.BouncingShield, ItemType.Ranged);
		ItemDatabase.RegisterUniqueVanillaItem(ItemID.LightDisc, ItemType.Ranged);
		ItemDatabase.RegisterUniqueVanillaItem(ItemID.Bananarang, ItemType.Ranged);
		ItemDatabase.RegisterUniqueVanillaItem(ItemID.PossessedHatchet, ItemType.Ranged);
		ItemDatabase.RegisterUniqueVanillaItem(ItemID.PaladinsHammer, ItemType.Ranged);
	}
}

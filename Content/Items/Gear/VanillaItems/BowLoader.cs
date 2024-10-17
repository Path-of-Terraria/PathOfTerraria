using PathOfTerraria.Common.Enums;
using PathOfTerraria.Core.Items;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.VanillaItems;

internal class BowLoader : ModSystem
{
	public override void Load()
	{
		ItemDatabase.RegisterVanillaItemAsGear(ItemID.DD2BetsyBow, ItemType.Bow);
		ItemDatabase.RegisterVanillaItemAsGear(ItemID.BloodRainBow, ItemType.Bow);
		ItemDatabase.RegisterVanillaItemAsGear(ItemID.ChlorophyteShotbow, ItemType.Bow);
		ItemDatabase.RegisterVanillaItemAsGear(ItemID.DaedalusStormbow, ItemType.Bow);
		ItemDatabase.RegisterVanillaItemAsGear(ItemID.FairyQueenRangedItem, ItemType.Bow);
		ItemDatabase.RegisterVanillaItemAsGear(ItemID.HellwingBow, ItemType.Bow);
		ItemDatabase.RegisterVanillaItemAsGear(ItemID.IceBow, ItemType.Bow);
		ItemDatabase.RegisterVanillaItemAsGear(ItemID.Marrow, ItemType.Bow);
		ItemDatabase.RegisterVanillaItemAsGear(ItemID.MoltenFury, ItemType.Bow);
		ItemDatabase.RegisterVanillaItemAsGear(ItemID.Phantasm, ItemType.Bow);
		ItemDatabase.RegisterVanillaItemAsGear(ItemID.DD2PhoenixBow, ItemType.Bow);
		ItemDatabase.RegisterVanillaItemAsGear(ItemID.PulseBow, ItemType.Bow);
		ItemDatabase.RegisterVanillaItemAsGear(ItemID.ShadowFlameBow, ItemType.Bow);
		ItemDatabase.RegisterVanillaItemAsGear(ItemID.BeesKnees, ItemType.Bow);
		ItemDatabase.RegisterVanillaItemAsGear(ItemID.Tsunami, ItemType.Bow);

		// Prehardmode
		LoadBow(ItemID.BorealWoodBow);
		LoadBow(ItemID.CopperBow);
		LoadBow(ItemID.PalmWoodBow);
		LoadBow(ItemID.RichMahoganyBow);
		LoadBow(ItemID.TinBow);
		LoadBow(ItemID.EbonwoodBow);
		LoadBow(ItemID.IronBow);
		LoadBow(ItemID.ShadewoodBow);
		LoadBow(ItemID.LeadBow);
		LoadBow(ItemID.SilverBow);
		LoadBow(ItemID.TungstenBow);
		LoadBow(ItemID.AshWoodBow);
		LoadBow(ItemID.GoldBow);
		LoadBow(ItemID.PlatinumBow);
		LoadBow(ItemID.DemonBow);
		LoadBow(ItemID.TendonBow);

		// Hardmode
		LoadBow(ItemID.PearlwoodBow);
		LoadBow(ItemID.CobaltRepeater);
		LoadBow(ItemID.PalladiumRepeater);
		LoadBow(ItemID.MythrilRepeater);
		LoadBow(ItemID.OrichalcumRepeater);
		LoadBow(ItemID.AdamantiteRepeater);
		LoadBow(ItemID.TitaniumRepeater);
		LoadBow(ItemID.HallowedRepeater);
		LoadBow(ItemID.StakeLauncher);

		static void LoadBow(short itemId)
		{
			ItemDatabase.RegisterUniqueVanillaItemAsGear(itemId, ItemType.Bow);
		}
	}
}

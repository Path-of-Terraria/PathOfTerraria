using PathOfTerraria.Common.Enums;
using PathOfTerraria.Core.Items;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.VanillaItems;

internal class BowLoader : ModSystem
{
	public override void Load()
	{
		ItemDatabase.RegisterVanillaItem(ItemID.DD2BetsyBow, ItemType.Bow);
		ItemDatabase.RegisterVanillaItem(ItemID.BloodRainBow, ItemType.Bow);
		ItemDatabase.RegisterVanillaItem(ItemID.ChlorophyteShotbow, ItemType.Bow);
		ItemDatabase.RegisterVanillaItem(ItemID.DaedalusStormbow, ItemType.Bow);
		ItemDatabase.RegisterVanillaItem(ItemID.FairyQueenRangedItem, ItemType.Bow);
		ItemDatabase.RegisterVanillaItem(ItemID.HellwingBow, ItemType.Bow);
		ItemDatabase.RegisterVanillaItem(ItemID.IceBow, ItemType.Bow);
		ItemDatabase.RegisterVanillaItem(ItemID.Marrow, ItemType.Bow);
		ItemDatabase.RegisterVanillaItem(ItemID.MoltenFury, ItemType.Bow);
		ItemDatabase.RegisterVanillaItem(ItemID.Phantasm, ItemType.Bow);
		ItemDatabase.RegisterVanillaItem(ItemID.DD2PhoenixBow, ItemType.Bow);
		ItemDatabase.RegisterVanillaItem(ItemID.PulseBow, ItemType.Bow);
		ItemDatabase.RegisterVanillaItem(ItemID.ShadowFlameBow, ItemType.Bow);
		ItemDatabase.RegisterVanillaItem(ItemID.BeesKnees, ItemType.Bow);
		ItemDatabase.RegisterVanillaItem(ItemID.Tsunami, ItemType.Bow);

		// Prehardmode
		LoadBow(ItemID.BorealWoodBow, "BorealWoodBow");
		LoadBow(ItemID.CopperBow, "CopperBow");
		LoadBow(ItemID.PalmWoodBow, "PalmWoodBow");
		LoadBow(ItemID.RichMahoganyBow, "RichMahoganyBow");
		LoadBow(ItemID.TinBow, "TinBow");
		LoadBow(ItemID.EbonwoodBow, "EbonwoodBow");
		LoadBow(ItemID.IronBow, "IronBow");
		LoadBow(ItemID.ShadewoodBow, "ShadewoodBow");
		LoadBow(ItemID.LeadBow, "LeadBow");
		LoadBow(ItemID.SilverBow, "SilverBow");
		LoadBow(ItemID.TungstenBow, "TungstenBow");
		LoadBow(ItemID.AshWoodBow, "AshWoodBow");
		LoadBow(ItemID.GoldBow, "GoldBow");
		LoadBow(ItemID.PlatinumBow, "PlatinumBow");
		LoadBow(ItemID.DemonBow, "DemonBow");
		LoadBow(ItemID.TendonBow, "TendonBow");

		// Hardmode
		LoadBow(ItemID.PearlwoodBow, "PearlwoodBow");
		LoadBow(ItemID.CobaltRepeater, "CobaltRepeater");
		LoadBow(ItemID.PalladiumRepeater, "PalladiumRepeater");
		LoadBow(ItemID.MythrilRepeater, "MythrilRepeater");
		LoadBow(ItemID.OrichalcumRepeater, "OrichalcumRepeater");
		LoadBow(ItemID.AdamantiteRepeater, "AdamantiteRepeater");
		LoadBow(ItemID.TitaniumRepeater, "TitaniumRepeater");
		LoadBow(ItemID.HallowedRepeater, "HallowedRepeater");
		LoadBow(ItemID.StakeLauncher, "StakeLauncher");

		void LoadBow(short itemId, string name)
		{
			ItemDatabase.RegisterUniqueVanillaItem(itemId, ItemType.Bow);
		}
	}
}

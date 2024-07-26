﻿using PathOfTerraria.Core;
using PathOfTerraria.Core.Items;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.VanillaItems;

internal class BowLoader : ModSystem
{
	public override void Load()
	{
		ItemDatabase.RegisterVanillaItem(ItemID.DD2BetsyBow, ItemType.Ranged);
		ItemDatabase.RegisterVanillaItem(ItemID.BloodRainBow, ItemType.Ranged);
		ItemDatabase.RegisterVanillaItem(ItemID.ChlorophyteShotbow, ItemType.Ranged);
		ItemDatabase.RegisterVanillaItem(ItemID.DaedalusStormbow, ItemType.Ranged);
		ItemDatabase.RegisterVanillaItem(ItemID.FairyQueenRangedItem, ItemType.Ranged);
		ItemDatabase.RegisterVanillaItem(ItemID.HellwingBow, ItemType.Ranged);
		ItemDatabase.RegisterVanillaItem(ItemID.IceBow, ItemType.Ranged);
		ItemDatabase.RegisterVanillaItem(ItemID.Marrow, ItemType.Ranged);
		ItemDatabase.RegisterVanillaItem(ItemID.MoltenFury, ItemType.Ranged);
		ItemDatabase.RegisterVanillaItem(ItemID.Phantasm, ItemType.Ranged);
		ItemDatabase.RegisterVanillaItem(ItemID.DD2PhoenixBow, ItemType.Ranged);
		ItemDatabase.RegisterVanillaItem(ItemID.PulseBow, ItemType.Ranged);
		ItemDatabase.RegisterVanillaItem(ItemID.ShadowFlameBow, ItemType.Ranged);
		ItemDatabase.RegisterVanillaItem(ItemID.BeesKnees, ItemType.Ranged);
		ItemDatabase.RegisterVanillaItem(ItemID.Tsunami, ItemType.Ranged);

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
			ItemDatabase.RegisterUniqueVanillaItem(itemId, ItemType.Ranged);
		}
	}
}
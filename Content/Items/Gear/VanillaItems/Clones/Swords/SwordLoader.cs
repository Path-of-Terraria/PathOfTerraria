using PathOfTerraria.Core;
using Terraria;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.VanillaItems.Clones.Swords;

internal class SwordLoader : ModSystem
{
	public override void Load()
	{
		void AddShortsword(short itemId, string material, bool onlyMat = false)
		{
			PoTItem.ManuallyLoadPoTItem(Mod, new InstancedVanillaClone(itemId, ItemType.Sword, onlyMat ? material : material + "Shortsword"));
		}

		// Shortswords
		AddShortsword(ItemID.CopperShortsword, "Copper");
		AddShortsword(ItemID.IronShortsword, "Iron");
		AddShortsword(ItemID.SilverShortsword, "Silver");
		AddShortsword(ItemID.GoldShortsword, "Gold");
		AddShortsword(ItemID.TinShortsword, "Tin");
		AddShortsword(ItemID.LeadShortsword, "Lead");
		AddShortsword(ItemID.TungstenShortsword, "TungstenShortsword");
		AddShortsword(ItemID.PlatinumShortsword, "Platinum");
		AddShortsword(ItemID.Gladius, "Gladius", true);
		AddShortsword(ItemID.Ruler, "Ruler", true);

		void AddBroadsword(short itemId, string name)
		{
			PoTItem.ManuallyLoadPoTItem(Mod, new InstancedVanillaClone(itemId, ItemType.Sword, name));
		}

		// Broadswords, Prehardmode
		AddBroadsword(ItemID.AdamantiteSword, "AdamantiteSword");
		AddBroadsword(ItemID.AshWoodSword, "AshWoodSword");
		AddBroadsword(ItemID.BatBat, "BatBat");
		AddBroadsword(ItemID.BeamSword, "BeamSword");
		AddBroadsword(ItemID.BladedGlove, "BladedGlove"); // This has a "Only gets 50% of any melee speed bonus" effect I can't find
		AddBroadsword(ItemID.BoneSword, "BoneSword");
		AddBroadsword(ItemID.BorealWoodSword, "BorealWoodSword");
		AddBroadsword(ItemID.CactusSword, "CactusSword");
		AddBroadsword(ItemID.CandyCaneSword, "CandyCaneSword");
		AddBroadsword(ItemID.ChlorophyteClaymore, "ChlorophyteClaymore");
		AddBroadsword(ItemID.ChlorophyteSaber, "ChlorophyteSaber");
		AddBroadsword(ItemID.ChristmasTreeSword, "ChristmasTreeSword");
		AddBroadsword(ItemID.TaxCollectorsStickOfDoom, "ClassyCane");
		AddBroadsword(ItemID.CobaltSword, "CobaltSword");
		AddBroadsword(ItemID.Cutlass, "Cutlass");
		AddBroadsword(ItemID.DeathSickle, "DeathSickle");
		AddBroadsword(ItemID.EbonwoodSword, "EbonwoodSword");
		AddBroadsword(ItemID.EnchantedSword, "EnchantedSword");
		AddBroadsword(ItemID.DyeTradersScimitar, "DyeTradersScimitar");
		AddBroadsword(ItemID.FalconBlade, "FalconBlade");
		AddBroadsword(ItemID.FetidBaghnakhs, "FetidBaghnakhs");
		AddBroadsword(ItemID.DD2SquireBetsySword, "FlyingDragon");
		AddBroadsword(ItemID.GoldBroadsword, "GoldBroadsword");
		AddBroadsword(ItemID.IceSickle, "IceSickle");
		AddBroadsword(ItemID.InfluxWaver, "InfluxWaver");
		AddBroadsword(ItemID.LeadBroadsword, "LeadBroadsword");
	}

	public override void PostSetupContent()
	{
		Item.claw[Mod.Find<ModItem>("BladedGlove").Type] = true;
	}
}

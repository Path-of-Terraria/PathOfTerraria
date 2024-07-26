using PathOfTerraria.Core;
using PathOfTerraria.Core.Items;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.VanillaItems;

internal class SwordLoader : ModSystem
{
	public override void Load()
	{
		ItemDatabase.RegisterVanillaItem(ItemID.BeeKeeper, ItemType.Sword);
		ItemDatabase.RegisterVanillaItem(ItemID.BladeofGrass, ItemType.Sword);
		ItemDatabase.RegisterVanillaItem(ItemID.Bladetongue, ItemType.Sword);
		ItemDatabase.RegisterVanillaItem(ItemID.BloodButcherer, ItemType.Sword);
		ItemDatabase.RegisterVanillaItem(ItemID.DD2SquireDemonSword, ItemType.Sword);
		ItemDatabase.RegisterVanillaItem(ItemID.BreakerBlade, ItemType.Sword);
		ItemDatabase.RegisterVanillaItem(ItemID.BreathingReed, ItemType.Sword);
		ItemDatabase.RegisterVanillaItem(ItemID.Excalibur, ItemType.Sword);
		ItemDatabase.RegisterVanillaItem(ItemID.Flymeal, ItemType.Sword);
		ItemDatabase.RegisterVanillaItem(ItemID.Frostbrand, ItemType.Sword);
		ItemDatabase.RegisterVanillaItem(ItemID.HamBat, ItemType.Sword);
		ItemDatabase.RegisterVanillaItem(ItemID.IceBlade, ItemType.Sword);
		ItemDatabase.RegisterVanillaItem(ItemID.Keybrand, ItemType.Sword);
		ItemDatabase.RegisterVanillaItem(ItemID.LightsBane, ItemType.Sword);
		ItemDatabase.RegisterVanillaItem(ItemID.Meowmere, ItemType.Sword);
		ItemDatabase.RegisterVanillaItem(ItemID.Muramasa, ItemType.Sword);
		ItemDatabase.RegisterVanillaItem(ItemID.NightsEdge, ItemType.Sword);
		ItemDatabase.RegisterVanillaItem(ItemID.BluePhaseblade, ItemType.Sword);
		ItemDatabase.RegisterVanillaItem(ItemID.GreenPhaseblade, ItemType.Sword);
		ItemDatabase.RegisterVanillaItem(ItemID.RedPhaseblade, ItemType.Sword);
		ItemDatabase.RegisterVanillaItem(ItemID.YellowPhaseblade, ItemType.Sword);
		ItemDatabase.RegisterVanillaItem(ItemID.PurplePhaseblade, ItemType.Sword);
		ItemDatabase.RegisterVanillaItem(ItemID.WhitePhaseblade, ItemType.Sword);
		ItemDatabase.RegisterVanillaItem(ItemID.OrangePhaseblade, ItemType.Sword);
		ItemDatabase.RegisterVanillaItem(ItemID.BluePhasesaber, ItemType.Sword);
		ItemDatabase.RegisterVanillaItem(ItemID.GreenPhasesaber, ItemType.Sword);
		ItemDatabase.RegisterVanillaItem(ItemID.RedPhasesaber, ItemType.Sword);
		ItemDatabase.RegisterVanillaItem(ItemID.YellowPhasesaber, ItemType.Sword);
		ItemDatabase.RegisterVanillaItem(ItemID.PurplePhasesaber, ItemType.Sword);
		ItemDatabase.RegisterVanillaItem(ItemID.WhitePhasesaber, ItemType.Sword);
		ItemDatabase.RegisterVanillaItem(ItemID.OrangePhasesaber, ItemType.Sword);
		ItemDatabase.RegisterVanillaItem(ItemID.PsychoKnife, ItemType.Sword);
		ItemDatabase.RegisterVanillaItem(ItemID.Starfury, ItemType.Sword);
		ItemDatabase.RegisterVanillaItem(ItemID.StarWrath, ItemType.Sword);
		ItemDatabase.RegisterVanillaItem(ItemID.TentacleSpike, ItemType.Sword);
		ItemDatabase.RegisterVanillaItem(ItemID.TerraBlade, ItemType.Sword);
		ItemDatabase.RegisterVanillaItem(ItemID.TheHorsemansBlade, ItemType.Sword);
		ItemDatabase.RegisterVanillaItem(ItemID.TragicUmbrella, ItemType.Sword);
		ItemDatabase.RegisterVanillaItem(ItemID.TrueExcalibur, ItemType.Sword);
		ItemDatabase.RegisterVanillaItem(ItemID.TrueNightsEdge, ItemType.Sword);
		ItemDatabase.RegisterVanillaItem(ItemID.Umbrella, ItemType.Sword);
		ItemDatabase.RegisterVanillaItem(ItemID.FieryGreatsword, ItemType.Sword);
		ItemDatabase.RegisterVanillaItem(ItemID.WaffleIron, ItemType.Sword);

		void AddShortsword(short itemId, string material, bool onlyMat = false)
		{
			ItemDatabase.RegisterUniqueVanillaItem(itemId, ItemType.Sword);
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
			ItemDatabase.RegisterUniqueVanillaItem(itemId, ItemType.Sword);
		}

		// Broadswords, Alphabetically (by in-game name)
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
		AddBroadsword(ItemID.AntlionClaw, "MandibleBlade");
		AddBroadsword(ItemID.MythrilSword, "MythrilSword");
		AddBroadsword(ItemID.OrichalcumSword, "OrichalcumSword");
		AddBroadsword(ItemID.PalladiumSword, "PalladiumSword");
		AddBroadsword(ItemID.PalmWoodSword, "PalmWoodSword");
		AddBroadsword(ItemID.PearlwoodSword, "PearlwoodSword");
		AddBroadsword(ItemID.PlatinumBroadsword, "PlatinumBroadsword");
		AddBroadsword(ItemID.PurpleClubberfish, "PurpleClubberfish");
		AddBroadsword(ItemID.RichMahoganySword, "RichMahoganySword");
		AddBroadsword(ItemID.Seedler, "Seedler");
		AddBroadsword(ItemID.ShadewoodSword, "ShadewoodSword");
		AddBroadsword(ItemID.SilverBroadsword, "SilverBroadsword");
		AddBroadsword(ItemID.SlapHand, "SlapHand");
		AddBroadsword(ItemID.StylistKilLaKillScissorsIWish, "StylishScissors");
		AddBroadsword(ItemID.TinBroadsword, "TinBroadsword");
		AddBroadsword(ItemID.TitaniumSword, "TitaniumSword");
		AddBroadsword(ItemID.TungstenBroadsword, "TungstenBroadsword");
		AddBroadsword(ItemID.ZombieArm, "ZombieArm");
	}

	public override void PostSetupContent()
	{
		// Item.claw[Mod.Find<ModItem>("BladedGlove").Type] = true;
	}
}

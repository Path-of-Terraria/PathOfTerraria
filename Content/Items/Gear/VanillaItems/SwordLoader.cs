using PathOfTerraria.Common.Enums;
using PathOfTerraria.Core.Items;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.VanillaItems;

internal class SwordLoader : ModSystem
{
	public override void Load()
	{
		ItemDatabase.RegisterVanillaItemAsGear(ItemID.BeeKeeper, ItemType.Sword);
		ItemDatabase.RegisterVanillaItemAsGear(ItemID.BladeofGrass, ItemType.Sword);
		ItemDatabase.RegisterVanillaItemAsGear(ItemID.Bladetongue, ItemType.Sword);
		ItemDatabase.RegisterVanillaItemAsGear(ItemID.BloodButcherer, ItemType.Sword);
		ItemDatabase.RegisterVanillaItemAsGear(ItemID.DD2SquireDemonSword, ItemType.Sword);
		ItemDatabase.RegisterVanillaItemAsGear(ItemID.BreakerBlade, ItemType.Sword);
		ItemDatabase.RegisterVanillaItemAsGear(ItemID.BreathingReed, ItemType.Sword);
		ItemDatabase.RegisterVanillaItemAsGear(ItemID.Excalibur, ItemType.Sword);
		ItemDatabase.RegisterVanillaItemAsGear(ItemID.Flymeal, ItemType.Sword);
		ItemDatabase.RegisterVanillaItemAsGear(ItemID.Frostbrand, ItemType.Sword);
		ItemDatabase.RegisterVanillaItemAsGear(ItemID.HamBat, ItemType.Sword);
		ItemDatabase.RegisterVanillaItemAsGear(ItemID.IceBlade, ItemType.Sword);
		ItemDatabase.RegisterVanillaItemAsGear(ItemID.Keybrand, ItemType.Sword);
		ItemDatabase.RegisterVanillaItemAsGear(ItemID.LightsBane, ItemType.Sword);
		ItemDatabase.RegisterVanillaItemAsGear(ItemID.Meowmere, ItemType.Sword);
		ItemDatabase.RegisterVanillaItemAsGear(ItemID.Muramasa, ItemType.Sword);
		ItemDatabase.RegisterVanillaItemAsGear(ItemID.NightsEdge, ItemType.Sword);
		ItemDatabase.RegisterVanillaItemAsGear(ItemID.BluePhaseblade, ItemType.Sword);
		ItemDatabase.RegisterVanillaItemAsGear(ItemID.GreenPhaseblade, ItemType.Sword);
		ItemDatabase.RegisterVanillaItemAsGear(ItemID.RedPhaseblade, ItemType.Sword);
		ItemDatabase.RegisterVanillaItemAsGear(ItemID.YellowPhaseblade, ItemType.Sword);
		ItemDatabase.RegisterVanillaItemAsGear(ItemID.PurplePhaseblade, ItemType.Sword);
		ItemDatabase.RegisterVanillaItemAsGear(ItemID.WhitePhaseblade, ItemType.Sword);
		ItemDatabase.RegisterVanillaItemAsGear(ItemID.OrangePhaseblade, ItemType.Sword);
		ItemDatabase.RegisterVanillaItemAsGear(ItemID.BluePhasesaber, ItemType.Sword);
		ItemDatabase.RegisterVanillaItemAsGear(ItemID.GreenPhasesaber, ItemType.Sword);
		ItemDatabase.RegisterVanillaItemAsGear(ItemID.RedPhasesaber, ItemType.Sword);
		ItemDatabase.RegisterVanillaItemAsGear(ItemID.YellowPhasesaber, ItemType.Sword);
		ItemDatabase.RegisterVanillaItemAsGear(ItemID.PurplePhasesaber, ItemType.Sword);
		ItemDatabase.RegisterVanillaItemAsGear(ItemID.WhitePhasesaber, ItemType.Sword);
		ItemDatabase.RegisterVanillaItemAsGear(ItemID.OrangePhasesaber, ItemType.Sword);
		ItemDatabase.RegisterVanillaItemAsGear(ItemID.PsychoKnife, ItemType.Sword);
		ItemDatabase.RegisterVanillaItemAsGear(ItemID.Starfury, ItemType.Sword);
		ItemDatabase.RegisterVanillaItemAsGear(ItemID.StarWrath, ItemType.Sword);
		ItemDatabase.RegisterVanillaItemAsGear(ItemID.TentacleSpike, ItemType.Sword);
		ItemDatabase.RegisterVanillaItemAsGear(ItemID.TerraBlade, ItemType.Sword);
		ItemDatabase.RegisterVanillaItemAsGear(ItemID.TheHorsemansBlade, ItemType.Sword);
		ItemDatabase.RegisterVanillaItemAsGear(ItemID.TragicUmbrella, ItemType.Sword);
		ItemDatabase.RegisterVanillaItemAsGear(ItemID.TrueExcalibur, ItemType.Sword);
		ItemDatabase.RegisterVanillaItemAsGear(ItemID.TrueNightsEdge, ItemType.Sword);
		ItemDatabase.RegisterVanillaItemAsGear(ItemID.Umbrella, ItemType.Sword);
		ItemDatabase.RegisterVanillaItemAsGear(ItemID.FieryGreatsword, ItemType.Sword);
		ItemDatabase.RegisterVanillaItemAsGear(ItemID.WaffleIron, ItemType.Sword);

		void AddShortsword(short itemId, string material, bool onlyMat = false)
		{
			ItemDatabase.RegisterUniqueVanillaItemAsGear(itemId, ItemType.Sword);
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
			ItemDatabase.RegisterUniqueVanillaItemAsGear(itemId, ItemType.Sword);
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

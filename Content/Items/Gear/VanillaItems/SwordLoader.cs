using PathOfTerraria.Core;
using PathOfTerraria.Core.Items;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.VanillaItems;

internal class SwordLoader : ModSystem
{
	public override void Load()
	{
		ItemDatabase.RegisterVanillaItem(ItemID.BeeKeeper, ItemType.Melee);
		ItemDatabase.RegisterVanillaItem(ItemID.BladeofGrass, ItemType.Melee);
		ItemDatabase.RegisterVanillaItem(ItemID.Bladetongue, ItemType.Melee);
		ItemDatabase.RegisterVanillaItem(ItemID.BloodButcherer, ItemType.Melee);
		ItemDatabase.RegisterVanillaItem(ItemID.DD2SquireDemonSword, ItemType.Melee);
		ItemDatabase.RegisterVanillaItem(ItemID.BreakerBlade, ItemType.Melee);
		ItemDatabase.RegisterVanillaItem(ItemID.BreathingReed, ItemType.Melee);
		ItemDatabase.RegisterVanillaItem(ItemID.Excalibur, ItemType.Melee);
		ItemDatabase.RegisterVanillaItem(ItemID.Flymeal, ItemType.Melee);
		ItemDatabase.RegisterVanillaItem(ItemID.Frostbrand, ItemType.Melee);
		ItemDatabase.RegisterVanillaItem(ItemID.HamBat, ItemType.Melee);
		ItemDatabase.RegisterVanillaItem(ItemID.IceBlade, ItemType.Melee);
		ItemDatabase.RegisterVanillaItem(ItemID.Keybrand, ItemType.Melee);
		ItemDatabase.RegisterVanillaItem(ItemID.LightsBane, ItemType.Melee);
		ItemDatabase.RegisterVanillaItem(ItemID.Meowmere, ItemType.Melee);
		ItemDatabase.RegisterVanillaItem(ItemID.Muramasa, ItemType.Melee);
		ItemDatabase.RegisterVanillaItem(ItemID.NightsEdge, ItemType.Melee);
		ItemDatabase.RegisterVanillaItem(ItemID.BluePhaseblade, ItemType.Melee);
		ItemDatabase.RegisterVanillaItem(ItemID.GreenPhaseblade, ItemType.Melee);
		ItemDatabase.RegisterVanillaItem(ItemID.RedPhaseblade, ItemType.Melee);
		ItemDatabase.RegisterVanillaItem(ItemID.YellowPhaseblade, ItemType.Melee);
		ItemDatabase.RegisterVanillaItem(ItemID.PurplePhaseblade, ItemType.Melee);
		ItemDatabase.RegisterVanillaItem(ItemID.WhitePhaseblade, ItemType.Melee);
		ItemDatabase.RegisterVanillaItem(ItemID.OrangePhaseblade, ItemType.Melee);
		ItemDatabase.RegisterVanillaItem(ItemID.BluePhasesaber, ItemType.Melee);
		ItemDatabase.RegisterVanillaItem(ItemID.GreenPhasesaber, ItemType.Melee);
		ItemDatabase.RegisterVanillaItem(ItemID.RedPhasesaber, ItemType.Melee);
		ItemDatabase.RegisterVanillaItem(ItemID.YellowPhasesaber, ItemType.Melee);
		ItemDatabase.RegisterVanillaItem(ItemID.PurplePhasesaber, ItemType.Melee);
		ItemDatabase.RegisterVanillaItem(ItemID.WhitePhasesaber, ItemType.Melee);
		ItemDatabase.RegisterVanillaItem(ItemID.OrangePhasesaber, ItemType.Melee);
		ItemDatabase.RegisterVanillaItem(ItemID.PsychoKnife, ItemType.Melee);
		ItemDatabase.RegisterVanillaItem(ItemID.Starfury, ItemType.Melee);
		ItemDatabase.RegisterVanillaItem(ItemID.StarWrath, ItemType.Melee);
		ItemDatabase.RegisterVanillaItem(ItemID.TentacleSpike, ItemType.Melee);
		ItemDatabase.RegisterVanillaItem(ItemID.TerraBlade, ItemType.Melee);
		ItemDatabase.RegisterVanillaItem(ItemID.TheHorsemansBlade, ItemType.Melee);
		ItemDatabase.RegisterVanillaItem(ItemID.TragicUmbrella, ItemType.Melee);
		ItemDatabase.RegisterVanillaItem(ItemID.TrueExcalibur, ItemType.Melee);
		ItemDatabase.RegisterVanillaItem(ItemID.TrueNightsEdge, ItemType.Melee);
		ItemDatabase.RegisterVanillaItem(ItemID.Umbrella, ItemType.Melee);
		ItemDatabase.RegisterVanillaItem(ItemID.FieryGreatsword, ItemType.Melee);
		ItemDatabase.RegisterVanillaItem(ItemID.WaffleIron, ItemType.Melee);

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

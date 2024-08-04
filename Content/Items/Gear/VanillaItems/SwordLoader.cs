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

		static void AddShortsword(short itemId)
		{
			ItemDatabase.RegisterUniqueVanillaItemAsGear(itemId, ItemType.Sword);
		}

		// Shortswords
		AddShortsword(ItemID.CopperShortsword);
		AddShortsword(ItemID.IronShortsword);
		AddShortsword(ItemID.SilverShortsword);
		AddShortsword(ItemID.GoldShortsword);
		AddShortsword(ItemID.TinShortsword);
		AddShortsword(ItemID.LeadShortsword);
		AddShortsword(ItemID.TungstenShortsword);
		AddShortsword(ItemID.PlatinumShortsword);
		AddShortsword(ItemID.Gladius);
		AddShortsword(ItemID.Ruler);

		static void AddBroadsword(short itemId)
		{
			ItemDatabase.RegisterUniqueVanillaItemAsGear(itemId, ItemType.Sword);
		}

		// Broadswords, Alphabetically (by in-game name)
		AddBroadsword(ItemID.AdamantiteSword);
		AddBroadsword(ItemID.AshWoodSword);
		AddBroadsword(ItemID.BatBat);
		AddBroadsword(ItemID.BeamSword);
		AddBroadsword(ItemID.BladedGlove);
		AddBroadsword(ItemID.BoneSword);
		AddBroadsword(ItemID.BorealWoodSword);
		AddBroadsword(ItemID.CactusSword);
		AddBroadsword(ItemID.CandyCaneSword);
		AddBroadsword(ItemID.ChlorophyteClaymore);
		AddBroadsword(ItemID.ChlorophyteSaber);
		AddBroadsword(ItemID.ChristmasTreeSword);
		AddBroadsword(ItemID.TaxCollectorsStickOfDoom);
		AddBroadsword(ItemID.CobaltSword);
		AddBroadsword(ItemID.Cutlass);
		AddBroadsword(ItemID.DeathSickle);
		AddBroadsword(ItemID.EbonwoodSword);
		AddBroadsword(ItemID.EnchantedSword);
		AddBroadsword(ItemID.DyeTradersScimitar);
		AddBroadsword(ItemID.FalconBlade);
		AddBroadsword(ItemID.FetidBaghnakhs);
		AddBroadsword(ItemID.DD2SquireBetsySword);
		AddBroadsword(ItemID.GoldBroadsword);
		AddBroadsword(ItemID.IceSickle);
		AddBroadsword(ItemID.InfluxWaver);
		AddBroadsword(ItemID.LeadBroadsword);
		AddBroadsword(ItemID.AntlionClaw);
		AddBroadsword(ItemID.MythrilSword);
		AddBroadsword(ItemID.OrichalcumSword);
		AddBroadsword(ItemID.PalladiumSword);
		AddBroadsword(ItemID.PalmWoodSword);
		AddBroadsword(ItemID.PearlwoodSword);
		AddBroadsword(ItemID.PlatinumBroadsword);
		AddBroadsword(ItemID.PurpleClubberfish);
		AddBroadsword(ItemID.RichMahoganySword);
		AddBroadsword(ItemID.Seedler);
		AddBroadsword(ItemID.ShadewoodSword);
		AddBroadsword(ItemID.SilverBroadsword);
		AddBroadsword(ItemID.SlapHand);
		AddBroadsword(ItemID.StylistKilLaKillScissorsIWish);
		AddBroadsword(ItemID.TinBroadsword);
		AddBroadsword(ItemID.TitaniumSword);
		AddBroadsword(ItemID.TungstenBroadsword);
		AddBroadsword(ItemID.ZombieArm);
	}
}

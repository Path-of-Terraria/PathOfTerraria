using PathOfTerraria.Core;
using PathOfTerraria.Core.Items;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.VanillaItems;

internal sealed class GenericVanillaWeaponLoader : ILoadable
{
	void ILoadable.Load(Mod mod)
	{
		// Boomerangs

		// Bows
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

		// Flails
		ItemDatabase.RegisterVanillaItem(ItemID.GolemFist, ItemType.Melee);
		ItemDatabase.RegisterVanillaItem(ItemID.KOCannon, ItemType.Melee);

		// Guns
		ItemDatabase.RegisterVanillaItem(ItemID.Boomstick, ItemType.Ranged);
		ItemDatabase.RegisterVanillaItem(ItemID.ChainGun, ItemType.Ranged);
		ItemDatabase.RegisterVanillaItem(ItemID.Gatligator, ItemType.Ranged);
		ItemDatabase.RegisterVanillaItem(ItemID.Megashark, ItemType.Ranged);
		ItemDatabase.RegisterVanillaItem(ItemID.Minishark, ItemType.Ranged);
		ItemDatabase.RegisterVanillaItem(ItemID.OnyxBlaster, ItemType.Ranged);
		ItemDatabase.RegisterVanillaItem(ItemID.PewMaticHorn, ItemType.Ranged);
		ItemDatabase.RegisterVanillaItem(ItemID.QuadBarrelShotgun, ItemType.Ranged);
		ItemDatabase.RegisterVanillaItem(ItemID.SDMG, ItemType.Ranged);
		ItemDatabase.RegisterVanillaItem(ItemID.Shotgun, ItemType.Ranged);
		ItemDatabase.RegisterVanillaItem(ItemID.SniperRifle, ItemType.Ranged);
		ItemDatabase.RegisterVanillaItem(ItemID.TacticalShotgun, ItemType.Ranged);
		ItemDatabase.RegisterVanillaItem(ItemID.Uzi, ItemType.Ranged);
		ItemDatabase.RegisterVanillaItem(ItemID.VenusMagnum, ItemType.Ranged);
		ItemDatabase.RegisterVanillaItem(ItemID.VortexBeater, ItemType.Ranged);
		ItemDatabase.RegisterVanillaItem(ItemID.Xenopopper, ItemType.Ranged);

		// Launchers
		ItemDatabase.RegisterVanillaItem(ItemID.FireworksLauncher, ItemType.Ranged);
		ItemDatabase.RegisterVanillaItem(ItemID.Celeb2, ItemType.Ranged);

		// Miscellaneous
		ItemDatabase.RegisterVanillaItem(ItemID.SolarEruption, ItemType.Melee);
		ItemDatabase.RegisterVanillaItem(ItemID.PiercingStarlight, ItemType.Melee);
		ItemDatabase.RegisterVanillaItem(ItemID.VampireKnives, ItemType.Ranged);
		ItemDatabase.RegisterVanillaItem(ItemID.Zenith, ItemType.Melee);

		// Swords
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
	}

	void ILoadable.Unload() { }
}

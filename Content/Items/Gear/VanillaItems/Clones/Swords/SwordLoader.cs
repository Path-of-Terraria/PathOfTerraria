using PathOfTerraria.Core;
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
		AddBroadsword(ItemID.BeeKeeper, "BeeKeeper");
	}
}

using Terraria.ID;

namespace PathOfTerraria.Common.Systems.VanillaModifications.BossItemRemovals;

internal class RemovePowerCells : ModSystem
{
	public override void PostWorldGen()
	{
		foreach (Chest chest in Main.chest)
		{
			if (chest is null)
			{
				continue;
			}

			foreach (Item item in chest.item)
			{
				if (item.type == ItemID.LihzahrdPowerCell)
				{
					item.SetDefaults(ItemID.LunarTabletFragment);
					item.stack = WorldGen.genRand.Next(14, 30);
				}
			}
		}
	}
}

using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using Terraria.ID;

namespace PathOfTerraria.Content.Passives.Utility.Masteries;

internal class RockAndStoneMastery : Passive
{
	internal class RockAndStonePlayer : ModPlayer 
	{
		private int tilesMined = 0;

		public override void Load()
		{
			On_Player.PickTile += PickTile;
		}

		private static void PickTile(On_Player.orig_PickTile orig, Player self, int x, int y, int pickPower)
		{
			Tile tile = Main.tile[x, y];
			bool hasTile = tile.HasTile;

			orig(self, x, y, pickPower);

			if (hasTile && !tile.HasTile)
			{
				self.GetModPlayer<RockAndStonePlayer>().AddMine();
			}
		}

		private void AddMine()
		{
			if (!Player.HasBuff(BuffID.Mining))
			{
				tilesMined++;

				if (Player.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<RockAndStoneMastery>(out float value) && tilesMined % (int)value == 0)
				{
					Player.AddBuff(BuffID.Mining, 60 * 10);
				}
			}
		}
	}
}

using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Content.Passives.Utility.Masteries;

internal class SpelunkyMastery : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.AddBuff(BuffID.Spelunker, 2);
		player.AddBuff(BuffID.Mining, 2);
	}

	internal class SpelunkyPlayer : ModPlayer
	{
		public override void Load()
		{
			On_Player.PickTile += PickTile;
		}

		private static void PickTile(On_Player.orig_PickTile orig, Player self, int x, int y, int pickPower)
		{
			Tile tile = Main.tile[x, y];
			bool hasTile = tile.HasTile;
			int oldTileId = tile.TileType;

			orig(self, x, y, pickPower);

			if (hasTile && !tile.HasTile && TileID.Sets.Ore[oldTileId] && self.GetModPlayer<PassiveTreePlayer>().HasNode<SpelunkyMastery>())
			{
				HashSet<Point16> takenPositions = [];
				RecursiveMine(x, y, oldTileId, takenPositions);
			}
		}

		private static void RecursiveMine(int x, int y, int tileType, HashSet<Point16> takenPositions)
		{
			WorldGen.KillTile(x, y);
			takenPositions.Add(new Point16(x, y));

			TryMine(x, y - 1);
			TryMine(x, y + 1);
			TryMine(x + 1, y);
			TryMine(x - 1, y);

			void TryMine(int x, int y)
			{
				Tile tile = Main.tile[x, y];

				if (tile.TileType == tileType && tile.HasTile && !takenPositions.Contains(new(x, y)))
				{
					RecursiveMine(x, y, tileType, takenPositions);
				}
			}
		}
	}
}

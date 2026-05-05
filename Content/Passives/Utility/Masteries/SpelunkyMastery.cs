using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Common.Systems.Synchronization;
using System.Collections.Generic;
using System.IO;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;

namespace PathOfTerraria.Content.Passives.Utility.Masteries;

internal class SpelunkyMastery : Passive
{
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

			if (!hasTile || !TileID.Sets.Ore[oldTileId] || !self.GetModPlayer<PassiveTreePlayer>().HasNode<SpelunkyMastery>())
			{
				return;
			}

			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				SpelunkyVeinMineHandler.Send(x, y, oldTileId);
			}
			else if (!tile.HasTile)
			{
					MineVein(self, x, y, oldTileId, false);
			}
		}

		private static int GetTileDropItem(int tileType)
		{
			for (int itemType = 1; itemType < ItemLoader.ItemCount; itemType++)
			{
				Item item = ContentSamples.ItemsByType[itemType];

				if (item.createTile == tileType && item.placeStyle <= 0)
				{
					return itemType;
				}
			}

			return TileLoader.GetItemDropFromTypeAndStyle(tileType, 0);
		}

		internal static void MineVein(Player player, int originX, int originY, int tileType, bool includeOrigin)
		{
			HashSet<Point16> takenPositions = [];
			List<Point16> orePositions = [];

			if (includeOrigin)
			{
				TryMine(originX, originY);
			}
			else
			{
				takenPositions.Add(new Point16(originX, originY));
				TryMine(originX, originY - 1);
				TryMine(originX, originY + 1);
				TryMine(originX + 1, originY);
				TryMine(originX - 1, originY);
			}

			if (orePositions.Count == 0)
			{
				return;
			}

			int minX = orePositions[0].X;
			int maxX = orePositions[0].X;
			int minY = orePositions[0].Y;
			int maxY = orePositions[0].Y;

			foreach (Point16 position in orePositions)
			{
				WorldGen.KillTile(position.X, position.Y, noItem: true);
				minX = Math.Min(minX, position.X);
				maxX = Math.Max(maxX, position.X);
				minY = Math.Min(minY, position.Y);
				maxY = Math.Max(maxY, position.Y);
			}

			if (Main.netMode == NetmodeID.Server)
			{
				NetMessage.SendTileSquare(-1, minX, minY, maxX - minX + 1, maxY - minY + 1, TileChangeType.None);
			}

			int dropItem = GetTileDropItem(tileType);

			if (dropItem <= ItemID.None)
			{
				return;
			}

			Vector2 dropPosition = new Vector2(originX, originY).ToWorldCoordinates();
			int item = Item.NewItem(new EntitySource_TileBreak(originX, originY), dropPosition, dropItem, orePositions.Count, noBroadcast: Main.netMode == NetmodeID.Server);

			if (Main.netMode == NetmodeID.Server)
			{
				Main.item[item].playerIndexTheItemIsReservedFor = player.whoAmI;
				NetMessage.SendData(MessageID.SyncItem, -1, -1, null, item);
			}

			void TryMine(int x, int y)
			{
				if (!WorldGen.InWorld(x, y))
				{
					return;
				}

				Point16 position = new(x, y);

				if (takenPositions.Contains(position))
				{
					return;
				}

				takenPositions.Add(position);
				Tile tile = Main.tile[x, y];

				if (tile.TileType == tileType && tile.HasTile)
				{
					orePositions.Add(position);
					TryMine(x, y - 1);
					TryMine(x, y + 1);
					TryMine(x + 1, y);
					TryMine(x - 1, y);
				}
			}
		}
	}
}

internal sealed class SpelunkyVeinMineHandler : Handler
{
	public static void Send(int x, int y, int tileType)
	{
		ModPacket packet = Networking.GetPacket<SpelunkyVeinMineHandler>();
		packet.Write((short)x);
		packet.Write((short)y);
		packet.Write((ushort)tileType);
		packet.Send();
	}

	internal override void ServerReceive(BinaryReader reader, byte sender)
	{
		int x = reader.ReadInt16();
		int y = reader.ReadInt16();
		int tileType = reader.ReadUInt16();
		Player player = Main.player[sender];

		if (!WorldGen.InWorld(x, y) || !TileID.Sets.Ore[tileType] || !player.active || !player.GetModPlayer<PassiveTreePlayer>().HasNode<SpelunkyMastery>())
		{
			return;
		}

		Point16 playerTile = player.Center.ToTileCoordinates16();
		float maxDistance = Player.tileRangeX * Player.tileRangeX + Player.tileRangeY * Player.tileRangeY;

		if (Vector2.DistanceSquared(playerTile.ToVector2(), new Vector2(x, y)) > maxDistance)
		{
			return;
		}

		Tile origin = Main.tile[x, y];

		if (!origin.HasTile || origin.TileType != tileType)
		{
			return;
		}

		SpelunkyMastery.SpelunkyPlayer.MineVein(player, x, y, tileType, true);
	}
}

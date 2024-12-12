using PathOfTerraria.Common.World.Passes;
using SubworldLibrary;
using Terraria.ID;

namespace PathOfTerraria.Common.NPCs;

/// <summary>
/// Handles death effects for boss NPCs that are delayed. 
/// There is no hook for it in tMod, as <see cref="ModNPC.OnKill"/> is not blocked by the loot blocker.<br/>
/// This is static as it doesn't need to be instanced.
/// </summary>
internal static class OnDeathNPC
{
	public static void OnDeathEffects(NPC npc)
	{
		if (npc.type == NPCID.WallofFlesh)
		{
			int x = (int)npc.Center.X / 16;
			int y = (int)npc.Center.Y / 16;
			int width = npc.width / 2 / 16 + 1;

			for (int i = x - width; i <= x + width; i++)
			{
				for (int j = y - width; j <= y + width; j++)
				{
					Tile tile = Main.tile[i, j];

					if (i == x - width || i == x + width || j == y - width || j == y + width)
					{
						if (!tile.HasTile)
						{
							tile.TileType = (ushort)(WorldGen.crimson ? 347u : 140u);
							tile.HasTile = true;
						}
					}

					tile.LiquidType = LiquidID.Water;
					tile.LiquidAmount = 0;

					if (Main.netMode == NetmodeID.Server)
					{
						NetMessage.SendTileSquare(-1, i, j);
					}
					else
					{
						WorldGen.SquareTileFrame(i, j);
					}
				}
			}
		}
	}
}

// #define ENABLED

#if DEBUG && ENABLED
using System.Collections.Generic;
using Terraria.ID;
using Terraria.DataStructures;
using ReLogic.Content;
using PathOfTerraria.Utilities.Xna;

namespace PathOfTerraria.Core.Debugging;

/// <summary>
/// This command has once been once used for quickly sketching out structures in image editors before getting in the game and editing them further.
/// However, using DragonLens' copy-and-paint tools may be more worthwhile than meddling with this command's color tables.
/// </summary>
public sealed class LoadImageStructureCommand : ModCommand
{
	public override string Command => "potLoadDebugStructure";
	public override CommandType Type => CommandType.Chat;

	private record struct TileInfo()
	{
		public ushort? Tile = null;
		public ushort Wall = 0;
		public (byte Type, byte Amount) Liquid = default;
	}

	public override void Action(CommandCaller caller, string input, string[] args)
    {
		if (args.Length != 1) { throw new UsageException("Expected one argument, the texture path without extension."); }

		string path = $"PathOfTerraria/{args[1]}";
		if (!ModContent.HasAsset(path))
		{
			Main.NewText("No such asset.", Color.Red);
			return;
		}
		
        Point16 basePos = Main.LocalPlayer.Center.ToTileCoordinates16();
		Texture2D tex = ModContent.Request<Texture2D>(path, AssetRequestMode.ImmediateLoad).Value;
		Color[] colors = new Color[tex.Width * tex.Height];
		tex.GetData(colors);

		Dictionary<Color, TileInfo> mapping = new()
		{
			{ ColorUtils.FromHexRgb(0x45283c), new() { Tile = TileID.Obsidian, Wall = WallID.ObsidianBackUnsafe } },
			{ ColorUtils.FromHexRgb(0x222034), new() { Wall = WallID.ObsidianBackUnsafe } },
			{ ColorUtils.FromHexRgb(0x8f563b), new() { Tile = TileID.Platforms } },
			{ ColorUtils.FromHexRgb(0x663931), new() { Wall = WallID.Wood } },
			{ ColorUtils.FromHexRgb(0xffa433), new() { Liquid = (1, 255) } },
		};

		for (ushort x = 0; x < tex.Width; x++)
		{
			for (ushort y = 0; y < tex.Height; y++)
			{
				Color color = colors[y * tex.Width + x];
				Point16 point = new(basePos.X + x, basePos.Y + y);
				Tile tile = Main.tile[point];
				
				if (!mapping.TryGetValue(color, out TileInfo info)) { info = default; }

				if (info.Tile != null) { (tile.HasTile, tile.TileType) = (true, info.Tile.Value); }
				if (info.Wall != 0) { tile.WallType = info.Wall; }
				if (info.Liquid.Type != 0) { (tile.LiquidType, tile.LiquidAmount) = (info.Liquid.Type, info.Liquid.Amount); }
			}
		}

		for (ushort x = 0; x < tex.Width; x++)
		{
			for (ushort y = 0; y < tex.Height; y++)
			{
				Point16 point = new(basePos.X + x, basePos.Y + y);
				WorldGen.SquareTileFrame(point.X, point.Y);
				WorldGen.SquareWallFrame(point.X, point.Y);
			}
		}
    }
}
#endif

using PathOfTerraria.Common.World.Utilities;
using Terraria.GameContent;

namespace PathOfTerraria.Common.Tiles.FramingKinds;

/// <summary>
/// Defines a <see cref="ModTile"/> as "kelp" for framing purposes. Also contains helper methods for placing and framing.
/// </summary>
internal interface IKelpTile
{
	public class KelpFraming : GlobalTile
	{
		public override bool TileFrame(int i, int j, int type, ref bool resetFrame, ref bool noBreak)
		{
			if (ModContent.GetModTile(type) is IKelpTile framing)
			{
				return framing.Frame(i, j);
			}

			return true;
		}

		public override void PostDraw(int i, int j, int type, SpriteBatch spriteBatch)
		{
			if (ModContent.GetModTile(type) is not IKelpTile kelp)
			{
				return;
			}

			kelp.DrawAdditional(i, j, spriteBatch);
		}

		public override bool PreDraw(int i, int j, int type, SpriteBatch spriteBatch)
		{
			if (ModContent.GetModTile(type) is not IKelpTile kelp)
			{
				return true;
			}

			return kelp.Draw(i, j, spriteBatch);
		}
	}

	public ushort Type { get; }

	public static void Place<T>(int i, int j, int frame) where T : ModTile, IKelpTile
	{
		Place(i, j, frame, ModContent.TileType<T>());
	}

	public static void Place(int i, int j, int frame, int type)
	{
		if (TileInvalid(i, j) && TileInvalid(i, j + 1))
		{
			WorldGen.TileFrame(i, j + 1);
			return;
		}

		Tile weed = Main.tile[i, j];
		weed.HasTile = true;
		weed.TileType = (ushort)type;
		weed.TileFrameX = (short)(18 * frame);
		weed.TileFrameNumber = Main.rand.NextBool(70) ? 1 : 0;
		WorldGen.TileFrame(i, j);

		static bool TileInvalid(int i, int j)
		{
			Tile tile = Main.tile[i, j];
			return tile.HasTile || tile.LiquidAmount < 100;
		}
	}

	internal static bool DefaultFraming(int i, int j)
	{
		Tile tile = Main.tile[i, j];
		Tile above = Main.tile[i, j - 1];

		if (!above.HasTile || ModContent.GetModTile(above.TileType) is not IKelpTile)
		{
			tile.TileFrameY = (short)(54 + 18 * Main.rand.Next(2));
		}
		else
		{
			tile.TileFrameY = (short)(18 * Main.rand.Next(3));
		}

		return false;
	}

	public static Vector2 TileOffset(int i, int j)
	{
		int distance = j;

		for (int y = j; y < j + 3; ++y)
		{
			if (WorldUtilities.SolidTile(i, y))
			{
				distance = y;
			}
		}

		return new Vector2(MathF.Sin(i * 1.423f + j * 0.6f + Main.GameUpdateCount * 0.04f) * 4, 0) * (1 - (distance - j) / 3f);
	}

	public bool Frame(int i, int j)
	{
		return DefaultFraming(i, j);
	}

	public void DrawAdditional(int i, int j, SpriteBatch spriteBatch)
	{
		if (Main.tile[i, j].TileFrameNumber != 0)
		{
			Vector2 position = TileExtensions.DrawPosition(i, j) + TileOffset(i, j);
			spriteBatch.Draw(TextureAssets.Tile[Type].Value, position.Floor(), new Rectangle(56, 18 * ((i + j) % 3), 16, 16), Color.White);
		}
	}

	public bool Draw(int i, int j, SpriteBatch spriteBatch)
	{
		Tile tile = Main.tile[i, j];
		Vector2 position = TileExtensions.DrawPosition(i, j) + TileOffset(i, j);
		spriteBatch.Draw(TextureAssets.Tile[Type].Value, position.Floor(), new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, 16), Lighting.GetColor(i, j));
		return false;
	}
}

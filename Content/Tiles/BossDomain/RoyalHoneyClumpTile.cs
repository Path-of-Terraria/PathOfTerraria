using PathOfTerraria.Common.Systems.DisableBuilding;
using PathOfTerraria.Content.Items.BossDomain;
using System.Collections.Generic;
using Terraria.GameContent;
using Terraria.ID;

namespace PathOfTerraria.Content.Tiles.BossDomain;

internal class RoyalHoneyClumpTile : ModTile, ICanCutTile
{
	public override void SetStaticDefaults()
	{
		Main.tileCut[Type] = true;
		Main.tileSolid[Type] = false;
		Main.tileFrameImportant[Type] = true;

		DustType = DustID.Honey;

		AddMapEntry(new Color(255, 156, 12));
	}

	public override void HitWire(int i, int j)
	{
		Wiring.SkipWire(i, j);
		Tile tile = Main.tile[i, j];

		if (tile.TileFrameY == 0)
		{
			tile.TileFrameY = 18;

			if (Main.netMode == NetmodeID.Server)
			{
				NetMessage.SendTileSquare(-1, i, j);
			}
		}
	}

	bool ICanCutTile.CanCut(int i, int j)
	{
		return Main.tile[i, j].TileFrameY > 0;
	}

	public override IEnumerable<Item> GetItemDrops(int i, int j)
	{
		if (Main.tile[i, j].TileFrameY > 0)
		{
			yield return new Item(ModContent.ItemType<RoyalJellyClump>());
		}
	}

	public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
	{
		Tile tile = Main.tile[i, j];
		Texture2D tex = TextureAssets.Tile[Type].Value;
		Vector2 offScreen = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);
		Vector2 position = new Vector2(i, j).ToWorldCoordinates(0, 0) - Main.screenPosition + offScreen + new Vector2(8, 16);
		var source = new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, 16);
		float sine = MathF.Max(0, 3 * MathF.Sin(-i - j + 0.05f * Main.GameUpdateCount)) * 0.05f;
		var color = Color.Lerp(Lighting.GetColor(i, j), Color.White, 0.8f);

		for (int l = 0; l < 3; l++)
		{
			spriteBatch.Draw(tex, position, source, color * (1 - l / 3f), 0f, new Vector2(8, 16), 1f + sine * MathF.Pow(l, 2), SpriteEffects.None, 0);
		}

		return false;
	}
}
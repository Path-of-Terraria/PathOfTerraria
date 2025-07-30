using PathOfTerraria.Common.Systems.MiscUtilities;
using PathOfTerraria.Common.Tiles;
using PathOfTerraria.Content.Tiles.BossDomain.Mech;
using Terraria.GameContent;

namespace PathOfTerraria.Content.Tiles.BossDomain;

internal class BoneGate : MechGate
{
	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		Main.tileSolid[Type] = false;
	}

	public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
	{
		Tile tile = Main.tile[i, j];

		Rectangle frame = tile.BasicFrame();
		frame.X %= 36;
		spriteBatch.Draw(TextureAssets.Tile[Type].Value, TileExtensions.DrawPosition(i, j), frame, Lighting.GetColor(i, j));

		if (tile.HasUnactuatedTile)
		{
			BlockerSystem.DrawGlow(i, j, Type, spriteBatch, BlockerGlow.Value, Color.Red);
		}

		return false;
	}
}
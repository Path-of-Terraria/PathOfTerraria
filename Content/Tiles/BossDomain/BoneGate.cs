using PathOfTerraria.Common.Tiles;
using PathOfTerraria.Content.Tiles.BossDomain.Mech;
using Terraria.GameContent;
using Terraria.ID;

namespace PathOfTerraria.Content.Tiles.BossDomain;

internal class BoneGate : MechGate
{
	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		TileID.Sets.NotReallySolid[Type] = true;

		Main.tileSolid[Type] = true;
	}

	public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
	{
		Tile tile = Main.tile[i, j];

		Rectangle frame = tile.BasicFrame();
		frame.X %= 36;
		spriteBatch.Draw(TextureAssets.Tile[Type].Value, TileExtensions.DrawPosition(i, j), frame, Lighting.GetColor(i, j));

		return false;
	}
}
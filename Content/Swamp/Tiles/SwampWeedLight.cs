using PathOfTerraria.Common.Tiles.FramingKinds;
using Terraria.ID;

namespace PathOfTerraria.Content.Swamp.Tiles;

internal class SwampWeedLight : ModTile, IKelpTile
{
	public override void SetStaticDefaults()
	{
		Main.tileCut[Type] = true;
		Main.tileSolid[Type] = false;
		Main.tileLighted[Type] = true;

		DustType = DustID.Grass;

		AddMapEntry(new Color(30, 81, 62));
	}

	public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
	{
		float mul = MathF.Sin(i + j + Main.GameUpdateCount * 0.05f) * 0.25f + 1f;
		(r, g, b) = (1f * mul, 0.85f * mul, 0.85f * mul);
	}

	void IKelpTile.DrawAdditional(int i, int j, SpriteBatch spriteBatch)
	{
	}
}

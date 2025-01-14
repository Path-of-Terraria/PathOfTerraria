using PathOfTerraria.Common.Subworlds.BossDomains;
using PathOfTerraria.Common.Tiles;
using ReLogic.Content;
using Terraria.ID;

namespace PathOfTerraria.Content.Tiles.Maps.Forest;

internal class PoweredRunestone : ModTile
{
	public override void SetStaticDefaults()
	{
		Main.tileBrick[Type] = true;
		Main.tileSolid[Type] = true;
		Main.tileBlockLight[Type] = true;
		Main.tileLighted[Type] = true;

		AddMapEntry(new Color(56, 66, 66));

		DustType = DustID.Lead;
		HitSound = SoundID.Tink;
	}

	public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
	{
		(r, g, b) = (0.3f, 0.3f, 1f);
	}
}
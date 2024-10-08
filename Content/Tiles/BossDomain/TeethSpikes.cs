using Terraria.ID;

namespace PathOfTerraria.Content.Tiles.BossDomain;

internal class TeethSpikes : ModTile
{
	public override void SetStaticDefaults()
	{
		TileID.Sets.CanBeClearedDuringGeneration[Type] = false;
		TileID.Sets.CanBeClearedDuringOreRunner[Type] = false;
		TileID.Sets.TouchDamageImmediate[Type] = 60;
		TileID.Sets.TouchDamageBleeding[Type] = true;

		Main.tileSolid[Type] = true;
		Main.tileBlockLight[Type] = true;

		AddMapEntry(new Color(134, 114, 74));

		DustType = DustID.Bone;
		HitSound = SoundID.NPCHit2;
	}
}
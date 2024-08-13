using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ObjectData;

namespace PathOfTerraria.Content.Tiles.BossDomain;

internal class EmbeddedEye : ModTile
{
	public override void SetStaticDefaults()
	{
		Main.tileFrameImportant[Type] = true;

		TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
		TileObjectData.newTile.CoordinateHeights = [16, 18];
		TileObjectData.newTile.StyleHorizontal = true;
		TileObjectData.newTile.RandomStyleRange = 2;
		TileObjectData.addTile(Type);

		DustType = DustID.Blood;

		AddMapEntry(new Color(115, 18, 28));
	}

	public override void NearbyEffects(int i, int j, bool closer)
	{
		if (closer && Main.LocalPlayer.DistanceSQ(new Vector2(i, j).ToWorldCoordinates()) < 200 * 200)
		{
			WorldGen.KillTile(i, j);
		}
	}

	public override void KillMultiTile(int i, int j, int frameX, int frameY)
	{
		int npc = NPC.NewNPC(new EntitySource_TileBreak(i, j), (i + 1) * 16, (j + 1) * 16, NPCID.DemonEye, 0);
		Main.npc[npc].velocity = new Vector2(0, -6);
		Main.npc[npc].netUpdate = true;

		for (int k = 0; k < 16; k++)
		{
			Dust.NewDust(new Vector2(i, j + 1).ToWorldCoordinates(0, 0), 32, 16, DustID.Blood, Main.rand.NextFloat(-2, 2), Main.rand.NextFloat(-4, -1));
		}
	}
}

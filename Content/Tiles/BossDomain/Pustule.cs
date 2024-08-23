using PathOfTerraria.Content.NPCs.BossDomain.BrainDomain;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ObjectData;

namespace PathOfTerraria.Content.Tiles.BossDomain;

internal class Pustule : ModTile
{
	public override void SetStaticDefaults()
	{
		Main.tileFrameImportant[Type] = true;

		TileObjectData.newTile.CopyFrom(TileObjectData.Style2x1);
		TileObjectData.newTile.CoordinateHeights = [18];
		TileObjectData.addTile(Type);

		DustType = DustID.CrimsonPlants;

		AddMapEntry(new Color(183, 163, 152));
	}

	public override void NearbyEffects(int i, int j, bool closer)
	{
		if (closer && Main.LocalPlayer.DistanceSQ(new Vector2(i, j).ToWorldCoordinates()) < 60 * 60)
		{
			WorldGen.KillTile(i, j);
		}
	}

	public override void KillMultiTile(int i, int j, int frameX, int frameY)
	{
		for (int k = 0; k < 3; ++k)
		{
			int npc = NPC.NewNPC(new EntitySource_TileBreak(i, j), (i + 1) * 16, (j + 1) * 16, ModContent.NPCType<Minera>(), 1);
			Main.npc[npc].velocity = new(0, -Main.rand.NextFloat(5, 8));
			Main.npc[npc].netUpdate = true;

			for (int l = 0; l < 16; l++)
			{
				Dust.NewDust(new Vector2(i, j + 1).ToWorldCoordinates(0, 0), 32, 32, DustID.CrimsonPlants, Main.rand.NextFloat(-2, 2), Main.rand.NextFloat(-4, -1));
			}
		}
	}
}
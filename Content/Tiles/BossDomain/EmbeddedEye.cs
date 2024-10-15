using PathOfTerraria.Common.Systems.Networking.Handlers;
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
		Tile tile = Main.tile[i, j];

		if (tile.TileFrameX % 36 == 0 && tile.TileFrameY == 0 && closer && Main.LocalPlayer.DistanceSQ(new Vector2(i, j).ToWorldCoordinates()) < 200 * 200)
		{
			WorldGen.KillTile(i, j);

			if (Main.netMode != NetmodeID.SinglePlayer)
			{
				NetMessage.SendTileSquare(-1, i, j, 2, 2);
			}
		}
	}

	public override void KillMultiTile(int i, int j, int frameX, int frameY)
	{
		if (Main.netMode != NetmodeID.MultiplayerClient)
		{
			int npc = NPC.NewNPC(new EntitySource_TileBreak(i, j), (i + 1) * 16, (j + 1) * 16, NPCID.DemonEye, 0);
			Main.npc[npc].velocity = new Vector2(0, -6).RotatedByRandom(0.2f);
			Main.npc[npc].netUpdate = true;
		}
		else
		{
			SpawnNPCOnServerHandler.Send(NPCID.DemonEye, new((i + 1) * 16, (j + 1) * 16), new Vector2(0, -6).RotatedByRandom(0.2f));
		}

		for (int k = 0; k < 16; k++)
		{
			Dust.NewDust(new Vector2(i, j + 1).ToWorldCoordinates(0, 0), 32, 16, DustID.Blood, Main.rand.NextFloat(-2, 2), Main.rand.NextFloat(-4, -1));
		}
	}
}

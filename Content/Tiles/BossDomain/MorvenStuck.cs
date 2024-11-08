using PathOfTerraria.Common.Systems.Networking.Handlers;
using PathOfTerraria.Content.NPCs.Town;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ObjectData;

namespace PathOfTerraria.Content.Tiles.BossDomain;

internal class MorvenStuck : ModTile
{
	public override void SetStaticDefaults()
	{
		Main.tileCut[Type] = true;
		Main.tileFrameImportant[Type] = true;

		TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3);
		TileObjectData.newTile.CoordinateHeights = [16, 16, 18];
		TileObjectData.newTile.Origin = new Point16(1, 1);
		TileObjectData.newTile.StyleHorizontal = true;
		TileObjectData.addTile(Type);

		DustType = DustID.Stone;

		AddMapEntry(Color.LightPink);
	}

	public override void KillMultiTile(int i, int j, int frameX, int frameY)
	{
		if (Main.netMode != NetmodeID.MultiplayerClient)
		{
			NPC.NewNPC(new EntitySource_TileBreak(i, j), (i + 1) * 16 + 8, (j + 2) * 16 + 4, ModContent.NPCType<MorvenNPC>(), 0);
		}
		else
		{
			SpawnNPCOnServerHandler.Send((short)ModContent.NPCType<MorvenNPC>(), new((i + 1) * 16 + 4, (j + 1) * 16));
		}

		Vector2 basePos = new Vector2(i, j + 1).ToWorldCoordinates(0, 0);

		for (int k = 0; k < 16; k++)
		{
			Dust.NewDust(basePos, 54, 36, DustID.Corruption, Main.rand.NextFloat(-2, 2), Main.rand.NextFloat(-2, 2));
		}
	}
}

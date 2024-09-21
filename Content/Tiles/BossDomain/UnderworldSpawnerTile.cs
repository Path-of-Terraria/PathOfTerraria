using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ObjectData;

namespace PathOfTerraria.Content.Tiles.BossDomain;

internal class UnderworldSpawnerTile : ModTile
{
	private static int LastFrameX = 0;

	public override void SetStaticDefaults()
	{
		Main.tileFrameImportant[Type] = true;

		TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
		TileObjectData.newTile.CoordinateHeights = [16];
		TileObjectData.newTile.AnchorTop = new AnchorData(Terraria.Enums.AnchorType.SolidTile, 2, 0);
		TileObjectData.newTile.StyleHorizontal = true;
		TileObjectData.addTile(Type);

		DustType = DustID.Lava;

		AddMapEntry(new Color(183, 163, 152));
	}

	public override void HitWire(int i, int j)
	{
		LastFrameX = Main.tile[i, j].TileFrameX;

		WorldGen.KillTile(i, j);
		Wiring.SkipWire(i, j);
	}

	public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
	{
		int type = (LastFrameX / 18) switch
		{
			0 => NPCID.LavaSlime,
			1 => NPCID.Demon,
			2 => NPCID.FireImp,
			_ => NPCID.Lavabat
		};

		int npc = NPC.NewNPC(new EntitySource_TileBreak(i, j), (i + 1) * 16, (j + 3) * 16, type, 1);
		Main.npc[npc].netUpdate = true;
	}
}
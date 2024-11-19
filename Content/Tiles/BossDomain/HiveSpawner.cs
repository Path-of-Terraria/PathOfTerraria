using PathOfTerraria.Common.Subworlds.BossDomains.WoFDomain;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ObjectData;

namespace PathOfTerraria.Content.Tiles.BossDomain;

internal class HiveSpawner : ModTile
{
	private static int LastFrameX = 0;

	public override void SetStaticDefaults()
	{
		Main.tileFrameImportant[Type] = true;

		TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
		TileObjectData.newTile.CoordinateHeights = [16];
		TileObjectData.newTile.AnchorTop = new AnchorData(AnchorType.SolidTile | AnchorType.EmptyTile, 2, 0);
		TileObjectData.newTile.AnchorBottom = AnchorData.Empty;
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

		if (Main.netMode == NetmodeID.Server)
		{
			NetMessage.SendTileSquare(-1, i, j);
		}
	}

	public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
	{
		int type = (LastFrameX / 18) switch
		{
			0 => NPCID.Hornet,
			1 => NPCID.Bee,
			_ => NPCID.MossHornet,
		};

		int repeats = 1;

		if (type == NPCID.Bee)
		{
			repeats = 6;
		}

		for (int k = 0; k < repeats; ++k)
		{
			int npc = NPC.NewNPC(new EntitySource_TileBreak(i, j), (i + 1) * 16, (j + 1) * 16, type, 1);
			Main.npc[npc].GetGlobalNPC<ArenaEnemyNPC>().Arena = true;
			Main.npc[npc].netUpdate = true;

			if (type == NPCID.Bee)
			{
				Main.npc[npc].velocity = new Vector2(2, 0).RotatedByRandom(MathHelper.TwoPi);
			}
		}
	}

#if !DEBUG
	public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
	{
		return false;
	}
#endif
}
using PathOfTerraria.Common.Subworlds;
using PathOfTerraria.Common.Systems.MiscUtilities;
using PathOfTerraria.Content.NPCs.BossDomain.Mech;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ObjectData;

namespace PathOfTerraria.Content.Tiles.BossDomain.Mech;

internal class MechSpawnerTile : ModTile
{
	private static short LastFrameX = 0;

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

		AddMapEntry(new Color(74, 74, 74));
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
			0 => NPCID.Probe,
			1 => ModContent.NPCType<CircuitSkull>(),
			_ => ModContent.NPCType<SecurityDrone>(),
		};

		int npc = NPC.NewNPC(new EntitySource_TileBreak(i, j), (i + 1) * 16, (j + 1) * 16, type, 1);
		Main.npc[npc].netUpdate = true;
		Main.npc[npc].GetGlobalNPC<ArenaEnemyNPC>().Arena = true;

		if (Main.netMode != NetmodeID.MultiplayerClient)
		{
			SpawnerSystem.SpawnerRecord.Add(npc, new(Type, LastFrameX, new Point16(i, j)));
		}
	}

#if !DEBUG
	public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
	{
		return false;
	}
#endif
}
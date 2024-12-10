using PathOfTerraria.Common.Subworlds.BossDomains.WoFDomain;
using PathOfTerraria.Content.NPCs.HellEvent;
using Terraria.DataStructures;
using Terraria.Enums;
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
			0 => NPCID.LavaSlime,
			1 => NPCID.Demon,
			2 => NPCID.FireImp,
			3 => NPCID.Lavabat,
			4 => NPCID.RedDevil,
			_ => ModContent.NPCType<FireMaw>(),
		};

		int npc = NPC.NewNPC(new EntitySource_TileBreak(i, j), (i + 1) * 16, (j + 1) * 16, type, 1);
		Main.npc[npc].netUpdate = true;
		Main.npc[npc].GetGlobalNPC<ArenaEnemyNPC>().Arena = true;
	}

#if !DEBUG
	public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
	{
		return false;
	}
#endif
}
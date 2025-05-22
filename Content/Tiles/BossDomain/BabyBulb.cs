using PathOfTerraria.Common.Subworlds.BossDomains.Hardmode;
using PathOfTerraria.Content.NPCs.BossDomain.PlantDomain;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ObjectData;

namespace PathOfTerraria.Content.Tiles.BossDomain;

internal class BabyBulb : ModTile
{
	public override void SetStaticDefaults()
	{
		Main.tileCut[Type] = true;
		Main.tileFrameImportant[Type] = true;
		Main.tileLighted[Type] = true;

		TileID.Sets.BreakableWhenPlacing[Type] = false;
		TileID.Sets.PreventsSandfall[Type] = true;
		TileID.Sets.PreventsTileRemovalIfOnTopOfIt[Type] = true;
		TileID.Sets.PreventsTileReplaceIfOnTopOfIt[Type] = true;

		TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
		TileObjectData.newTile.CoordinateHeights = [16, 18];
		TileObjectData.newTile.Origin = new Point16(1, 1);
		TileObjectData.addTile(Type);

		DustType = DustID.JungleGrass;

		AddMapEntry(new Color(201, 134, 131));
	}

	public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
	{
		(r, g, b) = (0.6f, 1, 0.1f);
	}

	public override void KillMultiTile(int i, int j, int frameX, int frameY)
	{
		int type = ModContent.NPCType<Minitera>();

		if (Main.netMode != NetmodeID.MultiplayerClient)
		{
			int npc = NPC.NewNPC(new EntitySource_TileBreak(i, j), (i + 1) * 16, (j + 1) * 16, type, 0);

			PlanteraDomain.BulbsBroken++;
			PlanteraDomain.PlaceBulb(true);
		}
	}
}

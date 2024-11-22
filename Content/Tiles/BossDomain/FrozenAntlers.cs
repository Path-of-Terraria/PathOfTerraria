using PathOfTerraria.Common.Systems.Networking.Handlers;
using PathOfTerraria.Content.Items.Quest;
using PathOfTerraria.Content.NPCs.BossDomain.DeerDomain;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ObjectData;

namespace PathOfTerraria.Content.Tiles.BossDomain;

internal class FrozenAntlers : ModTile
{
	public override void SetStaticDefaults()
	{
		Main.tileCut[Type] = true;
		Main.tileFrameImportant[Type] = true;
		Main.tileMerge[TileID.SnowBlock][Type] = true;

		TileID.Sets.ChecksForMerge[Type] = true;
		TileID.Sets.BreakableWhenPlacing[Type] = false;
		TileID.Sets.PreventsSandfall[Type] = true;
		TileID.Sets.PreventsTileRemovalIfOnTopOfIt[Type] = true;
		TileID.Sets.PreventsTileReplaceIfOnTopOfIt[Type] = true;

		TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3);
		TileObjectData.newTile.CoordinateHeights = [16, 16, 18];
		TileObjectData.newTile.Origin = new Point16(1, 1);
		TileObjectData.addTile(Type);

		DustType = DustID.WoodFurniture;

		AddMapEntry(Color.White);
		RegisterItemDrop(ModContent.ItemType<Antlers>());
	}

	public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData)
	{
		if (Main.rand.NextBool(30))
		{
			Vector2 pos = new Vector2(i, j).ToWorldCoordinates(Main.rand.NextFloat(16), Main.rand.NextFloat(16));
			Vector2 vel = new(Main.rand.NextFloat(-2, 2), Main.rand.NextFloat(-4, -2));
			Dust.NewDustPerfect(pos, DustID.RedTorch, vel, Scale: Main.rand.NextFloat(1, 1.4f));
		}
	}

	public override void KillMultiTile(int i, int j, int frameX, int frameY)
	{
		int type = ModContent.NPCType<SkullApparition>();

		if (Main.netMode != NetmodeID.MultiplayerClient)
		{
			int npc = NPC.NewNPC(new EntitySource_TileBreak(i, j), (i + 1) * 16, (j + 1) * 16, type, 0);
			Main.npc[npc].velocity = new Vector2(0, 8).RotatedByRandom(0.5f);
			Main.npc[npc].netUpdate = true;
		}
		else
		{
			SpawnNPCOnServerHandler.Send((short)type, new((i + 1) * 16, (j + 1) * 16), new Vector2(0, 8).RotatedByRandom(0.5f));
		}
	}
}

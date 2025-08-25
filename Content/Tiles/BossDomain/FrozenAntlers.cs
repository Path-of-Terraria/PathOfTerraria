using PathOfTerraria.Content.Items.Quest;
using PathOfTerraria.Content.NPCs.BossDomain.DeerDomain;
using PathOfTerraria.Content.Projectiles.Utility;
using System.Collections.Generic;
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
		HashSet<Point16> positions = [];

		// This method runs only on Singleplayer/Server, so a netmode check isn't necessary
		int npc = NPC.NewNPC(new EntitySource_TileBreak(i, j), (i + 1) * 16, (j + 1) * 16, type, 0);
		Main.npc[npc].velocity = new Vector2(0, 8).RotatedByRandom(0.5f);
		Main.npc[npc].netUpdate = true;
		
		for (int k = 0; k < 15; ++k)
		{
			Vector2 target = GetTarget(i, j, positions).ToWorldCoordinates();
			Vector2 pos = new Vector2(i, j).ToWorldCoordinates();
			Projectile.NewProjectile(new EntitySource_TileBreak(i, j), pos, Vector2.Zero, ModContent.ProjectileType<AntlerShardProj>(), 0, 0, Main.myPlayer, target.X, target.Y);
		}
	}

	private static Point16 GetTarget(int i, int j, HashSet<Point16> positions)
	{
		int reps = 0;
		
		while (reps < 15000)
		{
			reps++;

			var pos = new Point16(i + Main.rand.Next(-30, 30), j + Main.rand.Next(-30, 30));
			Tile tile = Main.tile[pos];

			if (!positions.Contains(pos) && !tile.HasTile && tile.WallType == WallID.LeadBrick)
			{
				positions.Add(pos);
				return pos;
			}
		}

		return new Point16(i, j - 5);
	}
}

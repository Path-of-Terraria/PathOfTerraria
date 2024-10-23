using PathOfTerraria.Common.Systems.Networking.Handlers;
using PathOfTerraria.Content.NPCs.BossDomain.BrainDomain;
using PathOfTerraria.Content.Projectiles.Hostile;
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
		TileObjectData.newTile.AnchorBottom = new AnchorData(Terraria.Enums.AnchorType.SolidTile, 2, 0);
		TileObjectData.newTile.AnchorTop = AnchorData.Empty;
		TileObjectData.newTile.RandomStyleRange = 2;
		TileObjectData.newTile.StyleHorizontal = false;
		TileObjectData.addTile(Type);

		DustType = DustID.CrimsonPlants;

		AddMapEntry(new Color(183, 163, 152));
	}

	public override void NearbyEffects(int i, int j, bool closer)
	{
		if (closer && Main.LocalPlayer.DistanceSQ(new Vector2(i, j).ToWorldCoordinates()) < 200 * 200)
		{
			WorldGen.KillTile(i, j);
		}
	}

	public override void KillMultiTile(int i, int j, int frameX, int frameY)
	{
		if (frameY == 0)
		{
			int length = Main.rand.Next(2, 4);

			for (int k = 0; k < length; ++k)
			{
				if (Main.netMode != NetmodeID.MultiplayerClient)
				{
					int npc = NPC.NewNPC(new EntitySource_TileBreak(i, j), (i + 1) * 16, (j + 1) * 16, ModContent.NPCType<Minera>(), 1);
					Main.npc[npc].velocity = new Vector2(0, -Main.rand.NextFloat(5, 8)).RotatedByRandom(0.9f);
					Main.npc[npc].netUpdate = true;
				}
				else
				{
					Vector2 vel = new Vector2(0, -Main.rand.NextFloat(5, 8)).RotatedByRandom(0.9f);
					SpawnNPCOnServerHandler.Send((short)ModContent.NPCType<Minera>(), new Vector2((i + 1) * 16, (j + 1) * 16), vel);
				}
			}
		}
		else 
		{
			int length = 8;

			for (int k = 0; k < length; ++k)
			{
				int npc = Projectile.NewProjectile(new EntitySource_TileBreak(i, j), (i + 1) * 16, j * 16, 
					MathHelper.Lerp(-3, 3, k / (length - 1f)), Main.rand.NextFloat(-18, -16), ModContent.ProjectileType<FallingPusBlock>(), 1, 0);
			}
		}

		for (int l = 0; l < 16; l++)
		{
			Dust.NewDust(new Vector2(i, j + 1).ToWorldCoordinates(0, 0), 32, 32, DustID.CrimsonPlants, Main.rand.NextFloat(-2, 2), Main.rand.NextFloat(-4, -1));
		}
	}
}
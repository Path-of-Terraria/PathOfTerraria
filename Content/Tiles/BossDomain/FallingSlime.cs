using PathOfTerraria.Content.Projectiles.Hostile;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ObjectData;

namespace PathOfTerraria.Content.Tiles.BossDomain;

internal class FallingSlime : ModTile
{
	public override void SetStaticDefaults()
	{
		Main.tileFrameImportant[Type] = true;
		Main.tileCut[Type] = true;

		TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
		TileObjectData.newTile.CoordinateHeights = [18];
		TileObjectData.newTile.StyleHorizontal = true;
		TileObjectData.newTile.DrawYOffset = -2;
		TileObjectData.newTile.AnchorBottom = AnchorData.Empty;
		TileObjectData.newTile.AnchorTop = new AnchorData(Terraria.Enums.AnchorType.SolidTile, 1, 0);
		TileObjectData.addTile(Type);

		DustType = DustID.Stone;

		AddMapEntry(new Color(64, 116, 226));
	}

	public override void NearbyEffects(int i, int j, bool closer)
	{
		Player plr = Main.LocalPlayer;
		Vector2 position = plr.position;
		bool lineOfSight = Collision.CanHit(position, plr.width, plr.head, new Vector2(i, j).ToWorldCoordinates(4, 4), 8, 8);
		
		if (closer && MathF.Abs(plr.Center.X / 16 - i) < 1.5f && plr.position.Y / 16 > j && lineOfSight)
		{
			WorldGen.KillTile(i, j);
		}
	}

	public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
	{
		Vector2 projPos = new Vector2(i, j).ToWorldCoordinates(8, 16);
		int projType = ModContent.ProjectileType<FallingSlimeProj>();
		Projectile.NewProjectile(new EntitySource_TileBreak(i, j), projPos, new Vector2(0, Main.rand.NextFloat(0.5f, 2)), projType, 10, 0, Main.myPlayer);

		for (int k = 0; k < 5; k++)
		{
			short type = Main.rand.NextBool() ? DustID.t_Slime : DustID.Stone;
			Dust.NewDust(new Vector2(i, j).ToWorldCoordinates(0, 0), 16, 16, type, Main.rand.NextFloat(-2, 2), Main.rand.NextFloat(1, 2));
		}
	}
}

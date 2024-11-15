using PathOfTerraria.Content.Projectiles.Hostile;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ObjectData;

namespace PathOfTerraria.Content.Tiles.BossDomain;

internal class Icicle : ModTile
{
	public override void SetStaticDefaults()
	{
		Main.tileFrameImportant[Type] = true;
		Main.tileLighted[Type] = true;

		TileObjectData.newTile.CopyFrom(TileObjectData.Style1x2);
		TileObjectData.newTile.CoordinateHeights = [16, 16];
		TileObjectData.newTile.AnchorBottom = AnchorData.Empty;
		TileObjectData.newTile.AnchorTop = new AnchorData(AnchorType.SolidTile, 1, 0);
		TileObjectData.newTile.RandomStyleRange = 3;
		TileObjectData.newTile.StyleHorizontal = true;
		TileObjectData.addTile(Type);

		DustType = DustID.Ice;

		AddMapEntry(new Color(250, 250, 255));
	}

	public override void NearbyEffects(int i, int j, bool closer)
	{
		if (Main.tile[i, j].TileFrameX == 0)
		{
			return;
		}

		Player plr = Main.LocalPlayer;
		Vector2 pos = plr.Center + plr.velocity * 24;
		bool lineOfSight = Collision.CanHit(pos, plr.width, plr.height, new Vector2(i, j + 1).ToWorldCoordinates(4, 4), 2, 2);

		if (MathF.Abs(pos.X / 16 - i) < 1.5f && pos.Y / 16 > j && lineOfSight)
		{
			WorldGen.KillTile(i, j);
		}
	}

	public override void KillMultiTile(int i, int j, int frameX, int frameY)
	{
		Vector2 projPos = new Vector2(i, j).ToWorldCoordinates(8, 16);
		int projType = ModContent.ProjectileType<IcicleProjectile>();
		var velocity = new Vector2(0, Main.rand.NextFloat(0.5f, 2));
		int proj = Projectile.NewProjectile(new EntitySource_TileBreak(i, j), projPos, velocity, projType, 10, 0, Main.myPlayer);
		Main.projectile[proj].frame = frameX / 18;

		for (int k = 0; k < 5; k++)
		{
			short type = Main.rand.NextBool() ? DustID.Ice : DustID.Snow;
			Dust.NewDust(new Vector2(i, j).ToWorldCoordinates(0, 0), 16, 16, type, Main.rand.NextFloat(-2, 2), Main.rand.NextFloat(1, 3));
		}
	}
}
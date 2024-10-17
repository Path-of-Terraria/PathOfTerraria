using PathOfTerraria.Common.Subworlds.BossDomains;
using PathOfTerraria.Common.Subworlds.BossDomains.DeerDomain;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ObjectData;

namespace PathOfTerraria.Content.Tiles.BossDomain;

internal class PolarIceLantern : ModTile
{
	public override void SetStaticDefaults()
	{
		Main.tileFrameImportant[Type] = true;
		Main.tileLighted[Type] = true;

		TileObjectData.newTile.CopyFrom(TileObjectData.Style1x2);
		TileObjectData.newTile.CoordinateHeights = [16, 16];
		TileObjectData.newTile.AnchorBottom = AnchorData.Empty;
		TileObjectData.newTile.AnchorTop = new AnchorData(AnchorType.SolidTile, 1, 0);
		TileObjectData.addTile(Type);

		DustType = DustID.Ice;

		AddMapEntry(new Color(250, 250, 255));
	}

	public override void NearbyEffects(int i, int j, bool closer)
	{
		if (Main.tile[i, j].TileFrameY == 0)
		{
			return;
		}

		DeerclopsDomain.LightMultiplier = 1f;
		Lighting.AddLight(new Vector2(i, j).ToWorldCoordinates(), new Vector3(0.6f, 0.6f, 0.8f));
		DeerclopsDomain.LightMultiplier = 0f;
	}

	public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
	{
		if (Main.tile[i, j].TileFrameY == 18)
		{
			LightBallProjectile.DrawGlow(new Vector2(i, j).ToWorldCoordinates(), i + j, Color.White, true);
		}
	}
}
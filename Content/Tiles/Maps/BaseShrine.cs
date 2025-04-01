using PathOfTerraria.Content.Items.Placeable;
using PathOfTerraria.Content.Projectiles.Utility;
using Terraria.DataStructures;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.ObjectData;

namespace PathOfTerraria.Content.Tiles.Maps;

public abstract class BaseShrine : ModTile
{
	public abstract int AoE { get; }

	public override void SetStaticDefaults()
	{
		Main.tileFrameImportant[Type] = true;
		Main.tileBlockLight[Type] = true;
		Main.tileNoAttach[Type] = true;

		TileID.Sets.HasOutlines[Type] = true;

		TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
		TileObjectData.newTile.CoordinateHeights = [16, 18];
		TileObjectData.newTile.StyleHorizontal = true;
		TileObjectData.newTile.StyleWrapLimit = 3;
		TileObjectData.newTile.Origin = new Point16(1, 0);
		TileObjectData.addTile(Type);

		DustType = DustID.Stone;
		HitSound = SoundID.Dig;
	}

	public override void NumDust(int i, int j, bool fail, ref int num)
	{
		num = fail ? 1 : 3;
	}

	public virtual void Activate(int i, int j)
	{
		Tile tile = Main.tile[i, j];
		int type = tile.TileFrameY / 38;
		Projectile.NewProjectile(new EntitySource_SpawnNPC(), new Vector2(i, j).ToWorldCoordinates(16, 16), Vector2.Zero, AoE, 0, 0, Main.myPlayer, type);
	}

	public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings)
	{
		return true;
	}

	public override void MouseOver(int i, int j)
	{
		base.MouseOver(i, j);

		Main.LocalPlayer.cursorItemIconEnabled = true;
		Main.LocalPlayer.cursorItemIconID = ModContent.ItemType<ArcaneObeliskItem>();
	}

	public override bool RightClick(int i, int j)
	{
		WorldGen.KillTile(i, j);
		int buffId = (ContentSamples.ProjectilesByType[AoE].ModProjectile as ShrineAoE).BuffId;
		Vector2 worldPos = new Vector2(i, j).ToWorldCoordinates();
		Main.LocalPlayer.AddBuff(buffId, 20 * 60);

		foreach (Projectile projectile in Main.ActiveProjectiles)
		{
			if (projectile.DistanceSQ(worldPos) < 120 * 120)
			{
				projectile.active = false;
			}
		}

		return true;
	}
}
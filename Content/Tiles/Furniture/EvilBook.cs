using PathOfTerraria.Content.Items.Quest;
using PathOfTerraria.Content.Projectiles.Utility;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ObjectData;

namespace PathOfTerraria.Content.Tiles.Furniture;

internal class EvilBook : ModTile
{
	public override void SetStaticDefaults()
	{
		Main.tileFrameImportant[Type] = true;
		Main.tileSolid[Type] = false;
		Main.tileBlockLight[Type] = false;

		TileObjectData.newTile.CopyFrom(TileObjectData.StyleOnTable1x1);
		TileObjectData.newTile.RandomStyleRange = 3;
		TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<EvilBookEntity>().Hook_AfterPlacement, -1, 0, false);
		TileObjectData.newTile.StyleHorizontal = true;
		TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidWithTop | AnchorType.SolidTile | AnchorType.Table 
			| AnchorType.Platform, 1, 0);
		TileObjectData.newTile.AnchorValidTiles = [TileID.MetalBars]; 
		TileObjectData.addTile(Type);

		DustType = DustID.UnusedBrown;

		RegisterItemDrop(ModContent.ItemType<AncientEvilBook>());
	}

	public override void NumDust(int i, int j, bool fail, ref int num)
	{
		num = 3;
	}

	public override void SetSpriteEffects(int i, int j, ref SpriteEffects effects)
	{
		effects = (i % 2 == 0) ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
	}

	public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
	{
		ModContent.GetInstance<EvilBookEntity>().Kill(i, j);
	}
}

internal class EvilBookEntity : ModTileEntity
{
	public override bool IsTileValidForEntity(int x, int y)
	{
		return Main.tile[x, y].HasTile && Main.tile[x, y].TileType == ModContent.TileType<EvilBook>();
	}

	public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate)
	{
		Tile tile = Main.tile[i, j];

		if (Main.netMode == NetmodeID.MultiplayerClient)
		{
			NetMessage.SendTileSquare(Main.myPlayer, i, j, 3, 5);
			NetMessage.SendData(MessageID.TileEntityPlacement, number: i, number2: j, number3: Type);
			return -1;
		}

		int placedEntity = Place(i, j);
		return placedEntity;
	}

	public override void Update()
	{
		if (!NPC.downedBoss3)
		{
			CheckRitual();
		}
	}

	private void CheckRitual()
	{
		foreach (Projectile proj in Main.ActiveProjectiles)
		{
			if (proj.ModProjectile is SkeletronRitualProj)
			{
				return;
			}
		}

		int i = Position.X;
		int j = Position.Y;

		if (!Main.tile[i, j].HasTile || Main.tile[i, j].TileType != ModContent.TileType<EvilBook>())
		{
			return;
		}

		Tile leftCandle = Main.tile[i - 1, j];
		Tile rightCandle = Main.tile[i + 1, j];
		Tile bar = Main.tile[i, j + 1];

		if (!leftCandle.HasTile || !rightCandle.HasTile || !bar.HasTile)
		{
			return;
		}

		if (!IsValidCandle(leftCandle) || !IsValidCandle(rightCandle) || bar.TileType != TileID.MetalBars ||
			bar.TileFrameX != 144 && bar.TileFrameX != 342)
		{
			return;
		}

		Vector2 worldPos = new Vector2(i, j).ToWorldCoordinates();
		IEntitySource source = new EntitySource_TileInteraction(Main.player[Player.FindClosest(worldPos - new Vector2(8), 16, 16)], i, j);
		int newProj = Projectile.NewProjectile(source, worldPos, Vector2.Zero, ModContent.ProjectileType<SkeletronRitualProj>(), 0, 0, Main.myPlayer);

		if (Main.netMode == NetmodeID.Server)
		{
			NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, newProj);
		}

		return;

		static bool IsValidCandle(Tile tile)
		{
			return tile.TileType == TileID.Candles || tile.TileType == TileID.PlatinumCandle;
		}
	}
}
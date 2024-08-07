using PathOfTerraria.Content.Items.Consumables.Maps;
using ReLogic.Content;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;

namespace PathOfTerraria.Content.Tiles.Furniture;

public class MapDevicePlaceable : ModTile
{
	public override string Texture => $"{PoTMod.ModName}/Assets/Items/Placeable/MapDeviceBase";

	private const int FrameWidth = 18 * 3;
	private const int FrameHeight = 18 * 4;
	private const int HorizontalFrames = 1;

	private const int
		VerticalFrames =
			1; // Optional: Increase this number to match the amount of relics you have on your extra sheet, if you choose to use the Item.placeStyle approach

	private Asset<Texture2D> _relicTexture;

	/// This is the portal that will appear above
	protected virtual string Portal => $"{PoTMod.ModName}/Assets/Items/Placeable/Portal";

	public override void Load()
	{
		// Cache the extra texture displayed on the pedestal
		_relicTexture = ModContent.Request<Texture2D>(Portal);
	}

	public override void SetStaticDefaults()
	{
		Main.tileShine[Type] = 400;
		Main.tileFrameImportant[Type] = true;

		TileID.Sets.InteractibleByNPCs[Type] = true;

		TileObjectData.newTile.CopyFrom(TileObjectData.Style3x4);
		TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<MapDeviceEntity>().Hook_AfterPlacement, -1, 0, false);
		TileObjectData.newTile.LavaDeath = false;
		TileObjectData.newTile.DrawYOffset = 2;
		TileObjectData.newTile.Direction = TileObjectDirection.PlaceLeft;
		TileObjectData.newTile.StyleHorizontal = false;
		TileObjectData.newTile.StyleWrapLimitVisualOverride = 2;
		TileObjectData.newTile.StyleMultiplier = 2;
		TileObjectData.newTile.StyleWrapLimit = 2;
		TileObjectData.newTile.styleLineSkipVisualOverride = 0;
		TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
		TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceRight;
		TileObjectData.addAlternate(1);
		TileObjectData.addTile(Type);

		AddMapEntry(new Color(233, 207, 94), Language.GetText("MapObject.MapDevice"));
	}

	public override bool CreateDust(int i, int j, ref int type)
	{
		return false;
	}

	public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height,
		ref short tileFrameX, ref short tileFrameY)
	{
		// This forces the tile to draw the pedestal even if the placeStyle differs. 
		tileFrameX %= FrameWidth; // Clamps the frameX
		tileFrameY %= FrameHeight * 2; // Clamps the frameY (two horizontally aligned place styles, hence * 2)
	}

	public override void KillMultiTile(int i, int j, int frameX, int frameY)
	{
		Tile tile = Main.tile[i, j];
		i -= tile.TileFrameX / 18;
		j -= tile.TileFrameY / 18;
		ModContent.GetInstance<MapDeviceEntity>().Kill(i, j);
	}

	public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData)
	{
		if (!TryGetEntity(ref i, ref j, out MapDeviceEntity entity) || entity.StoredMap == null)
		{
			return;
		}

		Tile tile = Main.tile[i, j];

		if (tile.TileFrameX == 0 && tile.TileFrameY == 0)
		{
			Main.instance.TilesRenderer.AddSpecialLegacyPoint(i, j);
		}
	}

	public override void SpecialDraw(int i, int j, SpriteBatch spriteBatch)
	{
		if (!TryGetEntity(ref i, ref j, out MapDeviceEntity entity) || entity.StoredMap == null)
		{
			return;
		}
		
		// This is lighting-mode specific, always include this if you draw tiles manually
		var offScreen = new Vector2(Main.drawToScreen ? 0 : Main.offScreenRange);

		// Take the tile, check if it actually exists
		var p = new Point(i, j);
		Tile tile = Main.tile[p.X, p.Y];
		if (tile == null || !tile.HasTile)
		{
			return;
		}

		// Get the initial draw parameters
		Texture2D texture = _relicTexture.Value;
		int frameY = tile.TileFrameX / FrameWidth; // Picks the frame on the sheet based on the placeStyle of the item
		Rectangle frame = texture.Frame(HorizontalFrames, VerticalFrames, 0, frameY);
		Vector2 origin = frame.Size() / 2f;
		Vector2 worldPos = p.ToWorldCoordinates(24f, 64f);
		Color color = Lighting.GetColor(p.X, p.Y);
		bool direction =
			tile.TileFrameY / FrameHeight != 0; // This is related to the alternate tile data we registered before
		SpriteEffects effects = direction ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

		// Some math magic to make it smoothly move up and down over time
		float offset = (float)Math.Sin(Main.GlobalTimeWrappedHourly * MathHelper.TwoPi / 5f);
		Vector2 drawPos = worldPos + offScreen - Main.screenPosition + new Vector2(0f, -40f) +
		                  new Vector2(0f, offset * 4f);

		// Draw the main texture
		spriteBatch.Draw(texture, drawPos, frame, color, 0f, origin, 1f, effects, 0f);

		// Draw the periodic glow effect
		float scale = (float)Math.Sin(Main.GlobalTimeWrappedHourly * MathHelper.TwoPi / 2f) * 0.3f + 0.7f;
		Color effectColor = color;
		effectColor.A = 0;
		effectColor = effectColor * 0.1f * scale;
		for (float num5 = 0f; num5 < 1f; num5 += 355f / (678f * (float)Math.PI))
		{
			spriteBatch.Draw(texture, drawPos + (MathHelper.TwoPi * num5).ToRotationVector2() * (6f + offset * 2f), frame,
				effectColor, 0f, origin, 1f, effects, 0f);
		}
	}

	public override bool RightClick(int i, int j)
	{
		if (TryGetEntity(ref i, ref j, out MapDeviceEntity entity))
		{
			entity.TryPlaceMap();
		}

		return true;
	}

	private static bool TryGetEntity(ref int i, ref int j, out MapDeviceEntity entity)
	{
		Tile tile = Main.tile[i, j];
		i -= tile.TileFrameX / 18;
		j -= tile.TileFrameY / 18;

		if (TileEntity.ByPosition.TryGetValue(new(i, j), out TileEntity tileEntity) && tileEntity is MapDeviceEntity mapEntity)
		{
			entity = mapEntity;
			return true;
		}

		entity = null;
		return false;
	}
}

internal class MapDeviceEntity : ModTileEntity
{
	public Item StoredMap = null;

	public override bool IsTileValidForEntity(int x, int y)
	{
		return Main.tile[x, y].HasTile && Main.tile[x, y].TileType == ModContent.TileType<MapDevicePlaceable>();
	}

	public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate)
	{
		Tile tile = Main.tile[i, j];
		i -= tile.TileFrameX / 18;
		j -= tile.TileFrameY / 18;
		return Place(i, j);
	}

	public override void OnKill()
	{
		Item.NewItem(new EntitySource_TileBreak(Position.X, Position.Y), Position.ToWorldCoordinates(), StoredMap);
		StoredMap = null;
	}

	public override void SaveData(TagCompound tag)
	{
		if (StoredMap is not null)
		{
			tag.Add("item", ItemIO.Save(StoredMap));
		}
	}

	public override void LoadData(TagCompound tag)
	{
		if (tag.TryGet("item", out TagCompound itemTag))
		{
			StoredMap = ItemIO.Load(itemTag);
		}
	}

	internal void TryPlaceMap()
	{
		if (StoredMap is not null)
		{
			(StoredMap.ModItem as Map).OpenMap();
			StoredMap = null;
			return;
		}

		Item heldItem = Main.LocalPlayer.HeldItem;
		if (heldItem.ModItem is not Map)
		{
			return;
		}

		Item clone = heldItem.Clone();
		clone.stack = 1;
		StoredMap = clone;
		heldItem.stack--;

		// If the stack is empty, remove the item from the player's inventory
		if (heldItem.stack <= 0)
		{
			heldItem.TurnToAir();
		}
	}
}
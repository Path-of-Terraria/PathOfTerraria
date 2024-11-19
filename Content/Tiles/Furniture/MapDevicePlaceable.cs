using PathOfTerraria.Common.Systems.Networking.Handlers.MapDevice;
using PathOfTerraria.Content.Items.Consumables.Maps;
using PathOfTerraria.Content.Items.Placeable;
using ReLogic.Content;
using System.IO;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;

namespace PathOfTerraria.Content.Tiles.Furniture;

public class MapDevicePlaceable : ModTile
{
	private const int FrameWidth = 18 * 3;
	private const int FrameHeight = 18 * 4 + 20;

	private static Asset<Texture2D> _portalTex;

	/// This is the portal that will appear above
	protected virtual string Portal => $"{PoTMod.ModName}/Assets/Items/Placeable/Portal";

	public override void Load()
	{
		// Cache the extra texture displayed on the pedestal
		_portalTex = ModContent.Request<Texture2D>(Portal);
	}

	public override void SetStaticDefaults()
	{
		Main.tileFrameImportant[Type] = true;

		TileID.Sets.InteractibleByNPCs[Type] = true;
		
		TileObjectData.newTile.CopyFrom(TileObjectData.Style3x4);
		TileObjectData.newTile.Height = 5;
		TileObjectData.newTile.CoordinateHeights = [16, 16, 16, 16, 16];
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

	public override void KillMultiTile(int i, int j, int frameX, int frameY)
	{
		Tile tile = Main.tile[i, j];
		i -= tile.TileFrameX / 18;
		j -= tile.TileFrameY / 18 % 5;
		ModContent.GetInstance<MapDeviceEntity>().Kill(i, j);
	}

	public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
	{
		Tile tile = Main.tile[i, j];

		if (tile.TileFrameX != 0 || tile.TileFrameY != 0 && tile.TileFrameY != 90)
		{
			return;
		}

		if (!TryGetEntity(ref i, ref j, out MapDeviceEntity entity) || entity.StoredMap == null)
		{
			return;
		}

		// This is lighting-mode specific, always include this if you draw tiles manually
		var offScreen = new Vector2(Main.drawToScreen ? 0 : Main.offScreenRange);

		// Take the tile, check if it actually exists
		var p = new Point(i, j);
		tile = Main.tile[p.X, p.Y];
		if (tile == null || !tile.HasTile)
		{
			return;
		}

		// Get the initial draw parameters
		Texture2D texture = _portalTex.Value;
		int frameY = tile.TileFrameX / FrameWidth; // Picks the frame on the sheet based on the placeStyle of the item
		Rectangle frame = texture.Frame(1, 1, 0, frameY);
		Vector2 origin = frame.Size() / 2f;
		Vector2 worldPos = p.ToWorldCoordinates(24f, 64f);
		Color color = Lighting.GetColor(p.X, p.Y);
		bool direction =
			tile.TileFrameY / FrameHeight != 0; // This is related to the alternate tile data we registered before
		SpriteEffects effects = direction ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

		// Some math magic to make it smoothly move up and down over time
		float offset = MathF.Sin(Main.GlobalTimeWrappedHourly * MathHelper.TwoPi / 5f);
		Vector2 drawPos = worldPos + offScreen - Main.screenPosition + new Vector2(0f, -36f) + new Vector2(0f, offset * 7f);

		// Draw the main texture
		for (int k = 0; k < 3; ++k)
		{
			float rotation = Main.GlobalTimeWrappedHourly * 7f * (k % 2 == 0 ? -1 : 1);
			Color drawColor = color * (1 - k * 0.2f);
			spriteBatch.Draw(texture, drawPos, frame, drawColor with { A = 150 }, rotation, origin, 1f - k * 0.2f, effects, 0f);
		}

		Texture2D itemTex = TextureAssets.Item[entity.StoredMap.type].Value;
		spriteBatch.Draw(itemTex, drawPos, null, Color.White * 0.95f, 0f, itemTex.Size() / 2f, 0.5f, effects, 0f);

		// Draw the periodic glow effect
		float scale = (float)Math.Sin(Main.GlobalTimeWrappedHourly * MathHelper.TwoPi / 2f) * 0.3f + 0.7f;
		Color effectColor = color;
		effectColor.A = 0;
		effectColor *= 0.1f * scale;
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
			entity.TryPlaceMap(i, j);
		}

		return true;
	}

	public override void MouseOver(int i, int j)
	{
		if (TryGetEntity(ref i, ref j, out MapDeviceEntity entity))
		{
			Player player = Main.LocalPlayer;
			player.noThrow = 2;
			player.cursorItemIconEnabled = true;
			player.cursorItemIconID = ModContent.ItemType<MapDevice>();

			if (entity.StoredMap != null)
			{
				player.cursorItemIconID = entity.StoredMap.type;
			}
		}
	}

	private static bool TryGetEntity(ref int i, ref int j, out MapDeviceEntity entity)
	{
		Tile tile = Main.tile[i, j];
		i -= tile.TileFrameX / 18;
		j -= tile.TileFrameY / 18 % 5;

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
		j -= tile.TileFrameY / 18 % 5;

		if (Main.netMode == NetmodeID.MultiplayerClient)
		{
			// Sync the entire multitile's area.
			NetMessage.SendTileSquare(Main.myPlayer, i, j, 3, 5);

			// Sync the placement of the tile entity with other clients. Needs to match Place() coordinates
			// The "type" parameter refers to the tile type which placed the tile entity, so "Type" (the type of the tile entity) needs to be used here instead
			NetMessage.SendData(MessageID.TileEntityPlacement, number: i, number2: j, number3: Type);
			return -1;
		}

		// ModTileEntity.Place() handles checking if the entity can be placed, then places it for you
		// Set "tileOrigin" to the same value you set TileObjectData.newTile.Origin to in the ModTile
		int placedEntity = Place(i, j);

		return placedEntity;
	}

	public override void OnKill()
	{
		if (StoredMap is not null)
		{
			Item.NewItem(new EntitySource_TileBreak(Position.X, Position.Y), Position.ToWorldCoordinates(), StoredMap);
			StoredMap = null;
		}
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

	public override void NetSend(BinaryWriter writer)
	{
		writer.Write(StoredMap is not null);
		
		if (StoredMap is not null)
		{
			writer.Write((short)StoredMap.type);
			writer.Write((byte)(StoredMap.ModItem as Map).RemainingUses);
		}
	}

	public override void NetReceive(BinaryReader reader)
	{
		bool hasMap = reader.ReadBoolean();

		if (hasMap)
		{
			short type = reader.ReadInt16();
			int remainingUses = reader.ReadByte();

			SetItem(type, remainingUses);
		}
	}

	internal void SetItem(short itemId, int remainingUses = -1)
	{
		if (ContentSamples.ItemsByType[itemId].ModItem is not Map map)
		{
			return;
		}

		StoredMap = new Item(itemId);
		(StoredMap.ModItem as Map).RemainingUses = remainingUses == -1 ? map.MaxUses : remainingUses;
	}

	internal void TryPlaceMap(int i, int j)
	{
		if (StoredMap is not null)
		{
			if (Main.netMode != NetmodeID.SinglePlayer)
			{
				ConsumeMapDeviceHandler.Send((byte)Main.myPlayer, new Point16(i, j));
			}

			var map = StoredMap.ModItem as Map;

			if (map is null)
			{
				StoredMap = null;
				return;
			}

			map.OpenMap();
			map.RemainingUses--;

			if (map.RemainingUses <= 0)
			{
				StoredMap = null;
			}

			return;
		}

		Item heldItem = Main.LocalPlayer.HeldItem;
		if (heldItem.ModItem is not Map heldMap)
		{
			return;
		}

		Item clone = heldItem.Clone();
		clone.stack = 1;
		StoredMap = clone;
		(StoredMap.ModItem as Map).RemainingUses = heldMap.MaxUses;
		heldItem.stack--;

		// If the stack is empty, remove the item from the player's inventory
		if (heldItem.stack <= 0)
		{
			heldItem.TurnToAir();
		}

		if (Main.netMode != NetmodeID.SinglePlayer)
		{
			PlaceMapInDeviceHandler.Send((byte)Main.myPlayer, (short)clone.type, new Point16(i, j));
		}
	}

	internal void ConsumeMap()
	{
		var map = StoredMap.ModItem as Map;
		map.RemainingUses--;

		if (map.RemainingUses <= 0)
		{
			StoredMap = null;
		}
	}
}
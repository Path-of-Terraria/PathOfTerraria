using PathOfTerraria.Common.Systems;
using PathOfTerraria.Content.Items.Consumables.Maps;
using ReLogic.Content;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ObjectData;

namespace PathOfTerraria.Content.Tiles.Furniture;

public class MapDevicePlaceable : ModTile
{
	public override string Texture => $"{nameof(PathOfTerraria)}/Assets/Items/Placeable/MapDeviceBase";
	private Map InsertedMap { get; set; }

	private const int FrameWidth = 18 * 3;
	private const int FrameHeight = 18 * 4;
	private const int HorizontalFrames = 1;

	private const int
		VerticalFrames =
			1; // Optional: Increase this number to match the amount of relics you have on your extra sheet, if you choose to use the Item.placeStyle approach

	private Asset<Texture2D> _relicTexture;

	/// This is the portal that will appear above
	protected virtual string Portal => $"{nameof(PathOfTerraria)}/Assets/Items/Placeable/Portal";

	public override void Load()
	{
		// Cache the extra texture displayed on the pedestal
		_relicTexture = ModContent.Request<Texture2D>(Portal);
	}

	public override void SetStaticDefaults()
	{
		Main.tileShine[Type] = 400; // Responsible for golden particles
		Main.tileFrameImportant[Type] = true; // Any multitile requires this
		TileID.Sets.InteractibleByNPCs[Type] = true; // Town NPCs will palm their hand at this tile

		TileObjectData.newTile.CopyFrom(TileObjectData.Style3x4);
		TileObjectData.newTile.LavaDeath = false; // Does not break when lava touches it
		TileObjectData.newTile.DrawYOffset = 2; // So the tile sinks into the ground
		TileObjectData.newTile.Direction = TileObjectDirection.PlaceLeft; // Player faces to the left
		TileObjectData.newTile.StyleHorizontal = false; // Based on how the alternate sprites are positioned on the sprite (by default, true)

		// This controls how styles are laid out in the texture file. This tile is special in that all styles will use the same texture section to draw the pedestal.
		TileObjectData.newTile.StyleWrapLimitVisualOverride = 2;
		TileObjectData.newTile.StyleMultiplier = 2;
		TileObjectData.newTile.StyleWrapLimit = 2;
		TileObjectData.newTile.styleLineSkipVisualOverride =
			0; // This forces the tile preview to draw as if drawing the 1st style.

		// Register an alternate tile data with flipped direction
		TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile); // Copy everything from above, saves us some code
		TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceRight; // Player faces to the right
		TileObjectData.addAlternate(1);

		// Register the tile data itself
		TileObjectData.addTile(Type);

		// Register map name and color
		// "MapObject.Relic" refers to the translation key for the vanilla "Relic" text
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

	public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData)
	{
		if (InsertedMap == null)
		{
			return;
		}
		
		if (drawData.tileFrameX % FrameWidth == 0 && drawData.tileFrameY % FrameHeight == 0)
		{
			Main.instance.TilesRenderer.AddSpecialLegacyPoint(i, j);
		}
	}

	public override void SpecialDraw(int i, int j, SpriteBatch spriteBatch)
	{
		if (InsertedMap == null)
		{
			return;
		}
		
		// This is lighting-mode specific, always include this if you draw tiles manually
		var offScreen = new Vector2(Main.offScreenRange);
		if (Main.drawToScreen)
		{
			offScreen = Vector2.Zero;
		}

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
		const float twoPi = (float)Math.PI * 2f;
		float offset = (float)Math.Sin(Main.GlobalTimeWrappedHourly * twoPi / 5f);
		Vector2 drawPos = worldPos + offScreen - Main.screenPosition + new Vector2(0f, -40f) +
		                  new Vector2(0f, offset * 4f);

		// Draw the main texture
		spriteBatch.Draw(texture, drawPos, frame, color, 0f, origin, 1f, effects, 0f);

		// Draw the periodic glow effect
		float scale = (float)Math.Sin(Main.GlobalTimeWrappedHourly * twoPi / 2f) * 0.3f + 0.7f;
		Color effectColor = color;
		effectColor.A = 0;
		effectColor = effectColor * 0.1f * scale;
		for (float num5 = 0f; num5 < 1f; num5 += 355f / (678f * (float)Math.PI))
		{
			spriteBatch.Draw(texture, drawPos + (twoPi * num5).ToRotationVector2() * (6f + offset * 2f), frame,
				effectColor, 0f, origin, 1f, effects, 0f);
		}
	}

	public override bool RightClick(int i, int j)
	{
		if (InsertedMap != null)
		{
			InsertedMap = null;
			MappingSystem.EnterMap(InsertedMap);
			return true;
		}

		Item heldItem = Main.LocalPlayer.HeldItem;
		if (heldItem.ModItem is not Map)
		{
			return false;
		}

		InsertMap();
		return true;
	}

	private void InsertMap()
	{
		Player player = Main.LocalPlayer;
		Item heldItem = player.HeldItem;
		InsertedMap = (Map)heldItem.ModItem;
		heldItem.stack--;

		// If the stack is empty, remove the item from the player's inventory
		if (heldItem.stack <= 0)
		{
			heldItem.TurnToAir();
		}
	}
}
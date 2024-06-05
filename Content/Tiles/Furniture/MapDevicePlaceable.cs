using PathOfTerraria.Content.Dusts;
using PathOfTerraria.Content.Items.Consumables.Maps;
using PathOfTerraria.Core.Systems;
using Terraria.ID;
using Terraria.ObjectData;

namespace PathOfTerraria.Content.Tiles.Furniture;

public class MapDevicePlaceable : ModTile
{
	public override string Texture => $"{PathOfTerraria.ModName}/Assets/Items/Placeable/MapDevice";
	private Map InsertedMap { get; set; }
	
	
	public override void SetStaticDefaults() {
		// Properties
		Main.tileSolidTop[Type] = true;
		Main.tileFrameImportant[Type] = true;
		Main.tileNoAttach[Type] = true;
		Main.tileContainer[Type] = true;
		Main.tileLavaDeath[Type] = true;
		TileID.Sets.DisableSmartCursor[Type] = true;
		TileID.Sets.BasicDresser[Type] = true;
		TileID.Sets.AvoidedByNPCs[Type] = true;

		AdjTiles = [TileID.Dressers];
		DustType = ModContent.DustType<Sparkle>();

		// Placement
		TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
		TileObjectData.newTile.Height = 5;
		TileObjectData.newTile.Width = 3;
		TileObjectData.newTile.CoordinateHeights = [16, 16, 16, 16, 16];
		TileObjectData.newTile.LavaDeath = false;
		TileObjectData.addTile(Type);
	}

	public override bool RightClick(int i, int j)
	{
		if (InsertedMap != null)
		{
			Main.NewText("Enter portal");
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
	
	public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
	{
		Texture2D texture = ModContent.Request<Texture2D>($"{PathOfTerraria.ModName}/Assets/Items/Placeable/MapDevice").Value;
		if (InsertedMap != null)
		{
			texture = ModContent.Request<Texture2D>($"{PathOfTerraria.ModName}/Assets/Items/Placeable/MapDeviceOn").Value;
		}
		
		Vector2 zero = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange, Main.offScreenRange);
		Main.spriteBatch.Draw(texture, new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y) + zero, null, Lighting.GetColor(i, j));
		return false;
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
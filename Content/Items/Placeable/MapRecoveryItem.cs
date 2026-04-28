using PathOfTerraria.Common.Systems.Questing;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ObjectData;

namespace PathOfTerraria.Content.Items.Placeable;

public class MapRecoveryItem : ModItem
{
	public class MapRecoveryTile : ModTile
	{
		public override void SetStaticDefaults()
		{
			Main.tileFrameImportant[Type] = true;
			Main.tileLighted[Type] = true;

			TileID.Sets.HasOutlines[Type] = true;
			TileID.Sets.InteractibleByNPCs[Type] = true;
			TileID.Sets.BreakableWhenPlacing[Type] = false;
			TileID.Sets.PreventsTileRemovalIfOnTopOfIt[Type] = true;
			TileID.Sets.PreventsTileReplaceIfOnTopOfIt[Type] = true;

			TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
			TileObjectData.newTile.LavaDeath = false;
			TileObjectData.newTile.CoordinateHeights = [16, 16];
			TileObjectData.addTile(Type);

			AddMapEntry(new Color(56, 54, 66));
		}

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
		{
			if (Main.LocalPlayer.GetModPlayer<QuestModPlayer>().HasAnyRecoveryItem)
			{
				const float ReductionFactor = 280f;
				(r, g, b) = (Main.DiscoR / ReductionFactor, Main.DiscoG / ReductionFactor, Main.DiscoB / ReductionFactor);
			}
		}

		public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY)
		{
			offsetY = (int)(MathF.Sin(Main.GameUpdateCount * 0.06f) * 4) - 4;

			if (!Main.LocalPlayer.GetModPlayer<QuestModPlayer>().HasAnyRecoveryItem)
			{
				offsetY = 0;
				tileFrameY += 36;
			}
		}

		public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings)
		{
			return Main.LocalPlayer.GetModPlayer<QuestModPlayer>().HasAnyRecoveryItem;
		}

		public override bool RightClick(int i, int j)
		{
			if (Main.LocalPlayer.GetModPlayer<QuestModPlayer>().HasAnyRecoveryItem)
			{
				Main.LocalPlayer.GetModPlayer<QuestModPlayer>().SpawnRecoveryItems(i, j, false, out int emblematicItemToDisplay);
				return true;
			}

			return false;
		}

		public override void MouseOver(int i, int j)
		{
			Player player = Main.LocalPlayer;
			player.GetModPlayer<QuestModPlayer>().SpawnRecoveryItems(i, j, true, out int emblematicItemToDisplay);

			if (emblematicItemToDisplay == -1)
			{
				return;
			}

			player.cursorItemIconID = emblematicItemToDisplay;
			player.noThrow = 2;
			player.cursorItemIconEnabled = true;
		}
	}

	public override void SetDefaults()
	{
		Item.DefaultToPlaceableTile(ModContent.TileType<MapRecoveryTile>());
		Item.rare = ItemRarityID.Blue;
		Item.width = 30;
		Item.height = 30;
		Item.value = Item.buyPrice(0, 2, 0, 0);
	}
}
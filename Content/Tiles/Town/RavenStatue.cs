using PathOfTerraria.Common.Subworlds;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Tiles;
using PathOfTerraria.Common.UI.Guide;
using PathOfTerraria.Core.UI;
using SubworldLibrary;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ObjectData;

namespace PathOfTerraria.Content.Tiles.Town;

internal class RavenStatue : ModTile
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

		TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3);
		TileObjectData.newTile.LavaDeath = false;
		TileObjectData.newTile.CoordinateHeights = [16, 16, 18];
		TileObjectData.addTile(Type);

		AddMapEntry(new Color(56, 54, 66));
	}

	public override bool CanKillTile(int i, int j, ref bool blockDamaged)
	{
		return false;
	}

	public override bool CanExplode(int i, int j)
	{
		return false;
	}

	public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
	{
		Tile tile = Main.tile[i, j];

		if (tile.TileFrameX == 0 && tile.TileFrameY == 18)
		{
			(r, g, b) = (0.6f * Main.rand.NextFloat(0.8f, 1), 0.4f * Main.rand.NextFloat(0.8f, 1), 0f);
		}
	}

	public override bool RightClick(int i, int j)
	{
		Main.LocalPlayer.GetModPlayer<PersistentReturningPlayer>().ReturnPosition = Main.LocalPlayer.Center;
		SubworldSystem.Enter<RavencrestSubworld>();
		return true;
	}

	public override void MouseOver(int i, int j)
	{
		Player player = Main.LocalPlayer;
		player.cursorItemIconID = -1;
		player.cursorItemIconText = "Enter Ravencrest";
		player.noThrow = 2;
		player.cursorItemIconEnabled = true;
	}

	public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
	{
		if (!UIManager.TryGet("Tutorial UI", out UIManager.UIStateData data) || data.UserInterface.CurrentState is not TutorialUIState tut || 
			!tut.Visible || tut.Step is not 10 and not 11)
		{
			return;
		}

		Texture2D outline = TextureAssets.HighlightMask[Type].Value;
		Tile tile = Main.tile[i, j];

		spriteBatch.Draw(outline, TileExtensions.DrawPosition(i, j), tile.BasicFrame(), Color.White);
	}
}

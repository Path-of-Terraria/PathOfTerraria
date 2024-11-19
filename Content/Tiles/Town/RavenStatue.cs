using PathOfTerraria.Common.Subworlds;
using PathOfTerraria.Common.Systems.ModPlayers;
using SubworldLibrary;
using Terraria.ID;
using Terraria.ObjectData;

namespace PathOfTerraria.Content.Tiles.Town;

internal class RavenStatue : ModTile
{
	public override void SetStaticDefaults()
	{
		Main.tileFrameImportant[Type] = true;
		Main.tileLighted[Type] = true;

		TileID.Sets.InteractibleByNPCs[Type] = true;

		TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3);
		TileObjectData.newTile.LavaDeath = false;
		TileObjectData.newTile.CoordinateHeights = [16, 16, 18];
		TileObjectData.addTile(Type);

		AddMapEntry(new Color(56, 54, 66));
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
}

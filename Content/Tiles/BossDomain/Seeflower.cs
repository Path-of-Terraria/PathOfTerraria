using PathOfTerraria.Common.Subworlds.BossDomains.Hardmode;
using PathOfTerraria.Common.Tiles;
using ReLogic.Content;
using SubworldLibrary;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ObjectData;

namespace PathOfTerraria.Content.Tiles.BossDomain;

internal class Seeflower : ModTile
{
	/// <summary>
	/// Manually track rotation since tile entities don't run on multiplayer clients.
	/// </summary>
	internal static Dictionary<Point16, float> AngleByPosition = [];

	private static Asset<Texture2D> WorldTexture = null;

	public override void SetStaticDefaults()
	{
		WorldTexture = ModContent.Request<Texture2D>(Texture + "World");

		Main.tileFrameImportant[Type] = true;
		Main.tileLighted[Type] = true;

		TileID.Sets.FramesOnKillWall[Type] = true;

		TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
		TileObjectData.newTile.RandomStyleRange = 1;
		TileObjectData.newTile.AnchorWall = true;
		TileObjectData.newTile.AnchorBottom = AnchorData.Empty;
		TileObjectData.newTile.StyleHorizontal = true;
		TileObjectData.addTile(Type);

		DustType = DustID.CorruptGibs;

		AddMapEntry(new Color(233, 235, 103));
	}

	public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
	{
		(r, g, b) = (0.6f, 0.5f, 0.0f);
	}

	public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData)
	{
		Main.instance.TilesRenderer.AddSpecialPoint(i, j, Terraria.GameContent.Drawing.TileDrawing.TileCounterType.CustomNonSolid);
	}

	public override void SpecialDraw(int i, int j, SpriteBatch spriteBatch)
	{
		if (!AngleByPosition.ContainsKey(new Point16(i, j)))
		{
			AngleByPosition.Add(new Point16(i, j), 0);
		}

		Texture2D tex = WorldTexture.Value;
		Tile tile = Main.tile[i, j];
		Vector2 position = TileExtensions.DrawPosition(i - 11, j - 11) - new Vector2(8);
		float angle = AngleByPosition[new Point16(i, j)];
		float sine = MathF.Sin(i + j + (float)Main.timeForVisualEffects * 0.01f) * 0.2f;

		spriteBatch.Draw(tex, position, null, Lighting.GetColor(i, j), angle + sine, tex.Size() / 2f, 1f, SpriteEffects.None, 0);
	}
}

public class SeeflowerSystem : ModSystem
{
	public override void PostUpdatePlayers()
	{
		if (SubworldSystem.Current is PlanteraDomain)
		{
			foreach (Point16 point in Seeflower.AngleByPosition.Keys)
			{
				float targetAngle = point.ToWorldCoordinates().AngleTo(PlanteraDomain.BulbPosition.ToWorldCoordinates()) - 0.6f;
				Seeflower.AngleByPosition[point] = Utils.AngleLerp(Seeflower.AngleByPosition[point], targetAngle, 0.03f);
			}
		}
	}
}
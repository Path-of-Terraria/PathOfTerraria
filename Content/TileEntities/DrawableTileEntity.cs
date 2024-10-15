using Terraria.DataStructures;

namespace PathOfTerraria.Content.TileEntities;

/// <summary>
/// Defines a Tile Entity that has drawing code directly on it, instead of through a dummy NPC or projectile.
/// </summary>
internal abstract class DrawableTileEntity : ModTileEntity
{
	/// <summary>Acts as the "hitbox" for if the entity is on-screen.</summary>
	protected virtual Point Size => Point.Zero;

	protected Vector2 WorldPosition => Position.ToWorldCoordinates();

	internal abstract void Draw(SpriteBatch draw);

	/// <summary>
	/// Determines if the Tile Entity can draw. Defaults to checking if the entity is on-screen.
	/// </summary>
	/// <returns></returns>
	public virtual bool CanDraw()
	{
		return OnScreen(new Rectangle((int)(WorldPosition.X - Main.screenPosition.X), (int)(WorldPosition.Y - Main.screenPosition.Y), Size.X, Size.Y));
	}

	public static bool OnScreen(Rectangle rect)
	{
		return rect.Intersects(new Rectangle(0, 0, Main.screenWidth, Main.screenHeight));
	}
}

internal class DrawDrawables : ILoadable
{
	public void Load(Mod mod)
	{
		On_Main.DrawNPCs += DrawTEs;
	}

	void ILoadable.Unload() { }

	private void DrawTEs(On_Main.orig_DrawNPCs orig, Main self, bool behind)
	{
		if (behind)
		{
			orig(self, behind);
			return;
		}

		foreach (System.Collections.Generic.KeyValuePair<int, TileEntity> item in TileEntity.ByID)
		{
			if (item.Value is DrawableTileEntity te && te.CanDraw())
			{
				te.Draw(Main.spriteBatch);
			}
		}

		orig(self, behind);
	}
}
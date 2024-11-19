using ReLogic.Content;
using Terraria.ID;
using Terraria.ObjectData;

namespace PathOfTerraria.Content.Tiles.Town;

public class WallClock : ModTile
{
	private sealed class WallClockSystemImpl : ModSystem
	{
		/// <summary>
		///		The rotation of the wall clock's hour hand in radians.
		/// </summary>
		public static float HourRotation;
		
		/// <summary>
		///		The rotation of the wall clock's minute hand in radians.
		/// </summary>
		public static float MinuteRotation;

		public override void OnWorldLoad()
		{
			CalculateRotation(out HourRotation, out MinuteRotation);
		}

		public override void PreUpdateWorld()
		{
			CalculateRotation(out float targetHourRotation, out float targetMinuteRotation);

			HourRotation = LerpAngle(HourRotation, targetHourRotation, 0.1f);
			MinuteRotation = LerpAngle(MinuteRotation, targetMinuteRotation, 0.1f);
		}

		private static void CalculateRotation(out float hourRotation, out float minuteRotation)
		{
			double time = Main.time;
		
			int hours = (Main.dayTime ? (int)(time / 3600) : (int)((time + 54000) / 3600)) - 1;
			int minutes = (int)(time % 3600) / 60;

			hourRotation = MathHelper.ToRadians((float)(hours * 30f));
			minuteRotation = MathHelper.ToRadians((float)(minutes * 6f));
		}
		
		// For some reason, Utils.AngleLerp does not behave properly. Maybe we should create our own utility?
		private static float LerpAngle(float from, float to, float amount)
		{
			float difference = MathHelper.WrapAngle(to - from);
			
			return from + difference * amount;
		}
	}
	
	private static readonly Asset<Texture2D> HourPointerTexture = 
		ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/Tiles/Town/WallClock_Hour");

	private static readonly Asset<Texture2D> MinutePointerTexture =
		ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/Tiles/Town/WallClock_Minute");

	public override void SetStaticDefaults()
	{
		Main.tileFrameImportant[Type] = true;
		Main.tileLavaDeath[Type] = true;

		TileID.Sets.Clock[Type] = true;
		TileID.Sets.FramesOnKillWall[Type] = true;

		TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3Wall);
		TileObjectData.addTile(Type);

		DustType = DustID.WoodFurniture;
		HitSound = SoundID.Dig;

		AdjTiles = new[] { (int)TileID.GrandfatherClocks };
	}

	public override void NumDust(int i, int j, bool fail, ref int num)
	{
		num = fail ? 1 : 3;
	}

	public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
	{
		Tile tile = Framing.GetTileSafely(i, j);

		if (tile.TileFrameX != 36 || tile.TileFrameY != 36)
		{
			return;
		}
		
		Vector2 screenOffset = Main.drawToScreen ? Vector2.Zero : new(Main.offScreenRange);
		Vector2 tileOffset = new(8f);
		
		Vector2 position = new Vector2(i, j) * 16f - Main.screenPosition + screenOffset - tileOffset;

		Color color = Lighting.GetColor(i, j);

		spriteBatch.Draw(
			HourPointerTexture.Value,
			position,
			null,
			color,
			WallClockSystemImpl.HourRotation,
			new Vector2(HourPointerTexture.Width() / 2f, 0f),
			1f,
			SpriteEffects.None,
			0f
		);

		spriteBatch.Draw(
			MinutePointerTexture.Value,
			position,
			null,
			color,
			WallClockSystemImpl.MinuteRotation,
			new Vector2(MinutePointerTexture.Width() / 2f, 0f),
			1f,
			SpriteEffects.None,
			0f
		);
	}
}
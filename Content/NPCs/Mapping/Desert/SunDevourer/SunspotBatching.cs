using PathOfTerraria.Content.NPCs.Mapping.Desert.SunDevourer.Projectiles;
using ReLogic.Content;
using System.Collections.Generic;

namespace PathOfTerraria.Content.NPCs.Mapping.Desert.SunDevourer;

/// <summary>
/// Handles batching <see cref="SunspotAura"/> draw calls to safe a bit of performance.
/// </summary>
internal class SunspotBatching : ModSystem
{
	/// <summary>
	/// Individual sunspot instance to be drawn in batch.
	/// </summary>
	/// <param name="Position">Position of the sunspot.</param>
	/// <param name="Scale">Scale of the sunspot.</param>
	/// <param name="Rotation">Rotation of the sunspot.</param>
	/// <param name="Who">whoAmI of the sunspot. Used for randomization.</param>
	public readonly record struct Sunspot(Vector2 Position, float Scale, float Rotation, int Who);

	private static readonly Asset<Texture2D>[] Sunspots = new Asset<Texture2D>[3];

	public static Queue<Sunspot> FullSunspots = new();
	public static Queue<Sunspot> SparseSunspots = new();

	private static Asset<Texture2D> RGBDisplacement;
	private static Asset<Texture2D> RGDisplacement;
	private static Asset<Effect> OffsetEffect;

	public override void Load()
	{
		On_Main.DrawProjectiles += DrawSunspots;

		RGBDisplacement = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/Misc/RGBDisplacement");
		RGDisplacement = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/Misc/RGDisplacement");
		OffsetEffect = ModContent.Request<Effect>($"{PoTMod.ModName}/Assets/Effects/SunspotOffsetting");

		for (int i = 0; i < 3; ++i)
		{
			Sunspots[i] = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/NPCs/Mapping/Desert/SunDevourer/Projectiles/SunspotAura" + (i == 0 ? "" : i + 1));
		}
	}

	private void DrawSunspots(On_Main.orig_DrawProjectiles orig, Main self)
	{
		orig(self);

		DrawSunspots(FullSunspots, false);
		DrawSunspots(SparseSunspots, true);
	}

	private static void DrawSunspots(Queue<Sunspot> sunspots, bool sparse)
	{
		if (sunspots.Count <= 0)
		{
			return;
		}

		Matrix trans = Main.GameViewMatrix.TransformationMatrix;
		Effect effect = OffsetEffect.Value;
		effect.Parameters["noiseTexture"].SetValue((sparse ? RGDisplacement : RGBDisplacement).Value);
		effect.Parameters["scroll"].SetValue(new Vector2(1) * (float)Main.timeForVisualEffects * 0.013f);
		Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, effect, trans);

		while (sunspots.Count > 0)
		{
			Sunspot spot = sunspots.Dequeue();
			Vector2 position = spot.Position - Main.screenPosition;
			Texture2D texture = Sunspots[spot.Who % 3].Value;
			
			Main.EntitySpriteDraw(texture, position, null, Color.White, spot.Rotation, texture.Size() / 2f, spot.Scale, SpriteEffects.None);
		}

		Main.spriteBatch.End();
	}
}

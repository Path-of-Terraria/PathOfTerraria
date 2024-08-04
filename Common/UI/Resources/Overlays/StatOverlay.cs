namespace PathOfTerraria.Common.UI.Resources.Overlays;

public sealed class StatOverlay : ModResourceOverlay
{
	public override bool PreDrawResource(ResourceOverlayDrawContext context)
	{
		if (context.texture.Name == @"Images\UI\PlayerResourceSets\FancyClassic\Heart_Fill")
		{
			var texture = (Texture2D)ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/Heart_Fill_Replace");

			float life = Main.LocalPlayer.statLife;
			int bars = (int)(life / 400);

			float thisBarLife = life - 400 * bars;
			float thisHeartLife = thisBarLife - context.resourceNumber * 20f;

			Color bar = GetBarColor(bars);

			if (bars > 0 && thisHeartLife < 20f)
			{
				Color subBar = GetBarColor(bars - 1);
				Main.spriteBatch.Draw(texture, context.position, null, subBar, 0f, context.origin, 1f, SpriteEffects.None, 0f);
			}

			if (thisHeartLife >= 20f)
			{
				Main.spriteBatch.Draw(texture, context.position, null, bar, 0f, context.origin, 1f, SpriteEffects.None, 0f);
			}
			else if (thisHeartLife > 0f)
			{
				Main.spriteBatch.Draw(texture, context.position, null, bar, 0f, context.origin, thisHeartLife / 20f, SpriteEffects.None, 0f);
			}

			return false;
		}

		return true;
	}

	private static Color GetBarColor(int bar)
	{
		bar += 6 * (bar % 2);

		return bar switch
		{
			0 => new(1, 0.65f, 0.55f),
			1 => new(0.75f, 0f, 0.75f),
			2 => new(0.75f, 1f, 0.75f),
			_ => Main.hslToRgb(bar % 12 / 12f, 0.5f, 0.5f),
		};
	}
}

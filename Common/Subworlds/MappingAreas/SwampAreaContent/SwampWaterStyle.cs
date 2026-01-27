using Terraria.ID;

namespace PathOfTerraria.Common.Subworlds.MappingAreas.SwampAreaContent;

public class SwampWaterStyle : ModWaterStyle
{
	public class SwampWaterfallStyle : ModWaterfallStyle
	{
		public override string Texture => "PathOfTerraria/Assets/BiomeContent/SwampWaterfallStyle";
	}

	public override string Texture => "PathOfTerraria/Assets/BiomeContent/SwampWaterStyle";

	private static float _waterTransparence = 0;

	public override void Load()
	{
		On_Main.DrawLiquid += DrawLiquidMoreOpaque;
	}

	private void DrawLiquidMoreOpaque(On_Main.orig_DrawLiquid orig, Main self, bool bg, int waterStyle, float Alpha, bool drawSinglePassLiquids)
	{
		int slot = ModContent.GetInstance<SwampWaterStyle>().Slot;
		Player plr = Main.LocalPlayer;

		if (!bg && waterStyle == slot)
		{
			bool wet = Collision.WetCollision(plr.position, plr.width, plr.height);
			_waterTransparence = MathHelper.Lerp(_waterTransparence, wet ? 1f : 1.8f, wet ? 0.1f : 0.02f);
			Alpha = _waterTransparence;
		}

		orig(self, bg, waterStyle, Alpha, drawSinglePassLiquids);
	}

	public override int ChooseWaterfallStyle()
	{
		return ModContent.GetInstance<SwampWaterfallStyle>().Slot;
	}

	public override int GetSplashDust()
	{
		return -1;//Mod.Find<ModDust>("VerdantWaterSplash").Type;
	}

	public override int GetDropletGore()
	{
		return GoreID.WaterDrip;
	}

	public override void LightColorMultiplier(ref float r, ref float g, ref float b)
    {
		r = 0.98f;
		g = 1.02f;
		b = 0.9f;
	}

	public override Color BiomeHairColor()
	{
		return new Color(112, 78, 109);
	}
}
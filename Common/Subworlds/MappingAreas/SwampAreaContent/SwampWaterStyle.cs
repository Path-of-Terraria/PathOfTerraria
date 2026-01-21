using Terraria.ID;

namespace PathOfTerraria.Common.Subworlds.MappingAreas.SwampAreaContent;

public class SwampWaterStyle : ModWaterStyle
{
	public class SwampWaterfallStyle : ModWaterfallStyle
	{
		public override string Texture => "PathOfTerraria/Assets/BiomeContent/SwampWaterfallStyle";
	}

	public override string Texture => "PathOfTerraria/Assets/BiomeContent/SwampWaterStyle";

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
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;

namespace PathOfTerraria.Content.NPCs.Mapping.Desert.SunDevourer;

public sealed class SunDevourerFilter : ILoadable
{
	public const string FILTER_NAME = $"{PoTMod.ModName}:{nameof(SunDevourerFilter)}";

	void ILoadable.Load(Mod mod)
	{
		Filters.Scene[FILTER_NAME] = new Filter(new ScreenShaderData("FilterMoonLord"), EffectPriority.VeryHigh);
		Filters.Scene[FILTER_NAME].Load();

		SkyManager.Instance[FILTER_NAME] = new SunDevourerSky();
	}

	void ILoadable.Unload() { }
}
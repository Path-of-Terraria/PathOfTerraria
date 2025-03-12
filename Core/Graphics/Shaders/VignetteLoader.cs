using System.Linq;
using System.Reflection;
using ReLogic.Content;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;

namespace PathOfTerraria.Core.Graphics.Shaders;

[Autoload(Side = ModSide.Client)]
public sealed class VignetteLoader : ILoadable
{
	/// <summary>
	///		The path to the vignette shader, qualified by the mod name.
	/// </summary>
	public const string EFFECT_PATH = $"{PoTMod.ModName}/Assets/Effects/Vignette";
	
	/// <summary>
	///		The name of the filter for the vignette shader.
	/// </summary>
	public const string FILTER_NAME = $"{PoTMod.ModName}:Vignette";

	/// <summary>
	///		The name of the pass for the vignette shader.
	/// </summary>
	public const string PASS_NAME = "VignettePass";

	/// <summary>
	///		Gets or sets the asset for the vignette shader.
	/// </summary>
	public static Asset<Effect> Vignette { get; private set; }

	void ILoadable.Load(Mod mod)
	{
		Vignette = ModContent.Request<Effect>(EFFECT_PATH, AssetRequestMode.ImmediateLoad);

		var shader = Vignette.Value;

		Filters.Scene[FILTER_NAME] = new Filter(new ScreenShaderData(new Ref<Effect>(shader), PASS_NAME));
		Filters.Scene[FILTER_NAME].Load();
	}

	void ILoadable.Unload()
	{
		Vignette?.Dispose();
		Vignette = null;
	}
}
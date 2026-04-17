#nullable enable

using ReLogic.Content;

namespace PathOfTerraria.Core.Lighting;

internal static class LitShaders 
{
    private static Asset<Effect>? litSprite;

	public static Effect LitSprite(in Matrix matrix)
	{
		return Prepare(ref litSprite, in matrix, $"{nameof(PathOfTerraria)}/Assets/Effects/LitSprite");
	}

	private static Effect Prepare(ref Asset<Effect>? cache, in Matrix matrix, string path)
    {
        cache ??= ModContent.Request<Effect>(path, AssetRequestMode.ImmediateLoad);

        cache.Value.Parameters["Lighting"].SetValue(LightingBuffer.GetOrWhite());
        cache.Value.Parameters["MatrixTransform"].SetValue(matrix);
        cache.Value.Parameters["ScreenResolution"]?.SetValue(Main.ScreenSize.ToVector2());

        return cache.Value;
    }
}

using ReLogic.Content;

namespace PathOfTerraria.Utilities.Terraria;

public static class AssetUtils
{
	public static Asset<T> Immediate<T>(string path) where T : class
    {
		return ModContent.Request<T>(path, AssetRequestMode.ImmediateLoad);
    }
	public static Asset<T> Immediate<T>(string path, ref Asset<T> cache) where T : class
    {
		return (cache ??= ModContent.Request<T>(path, AssetRequestMode.ImmediateLoad));
    }
	public static T ImmediateValue<T>(string path) where T : class
    {
		return Immediate<T>(path).Value;
    }
	public static T ImmediateValue<T>(string path, ref Asset<T> cache) where T : class
    {
		return Immediate(path, ref cache).Value;
    }

	public static bool Async<T>(string path, out Asset<T> result) where T : class
    {
		if (ModContent.Request<T>(path) is { IsLoaded: true } asset)
		{
            result = asset;
			return true;
		}
        
        result = default;
        return false;
    }
	public static bool Async<T>(string path, ref Asset<T> cache, out Asset<T> result) where T : class
    {
		if ((cache ??= ModContent.Request<T>(path)) is { IsLoaded: true } asset)
		{
            result = asset;
			return true;
		}
        
        result = default;
        return false;
    }
	public static bool AsyncValue<T>(string path, out T value) where T : class
    {
		if (Async(path, out Asset<T> asset))
        {
            value = asset.Value;
            return true;
        }

        value = default;
        return false;
    }
	public static bool AsyncValue<T>(string path, ref Asset<T> cache, out T value) where T : class
    {
		if (Async(path, ref cache, out Asset<T> asset))
        {
            value = asset.Value;
            return true;
        }

        value = default;
        return false;
    }
}

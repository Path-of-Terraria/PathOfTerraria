using ReLogic.Content;
using Terraria.GameContent;

namespace PathOfTerraria.Common.Utilities;

public static class TextureUtils
{
	public static void InitializeWithColor(Texture2D texture, Color color)
	{
		var data = new Color[texture.Width * texture.Height];
		Array.Fill(data, color);
		texture.SetData(data);
	}

	public static Asset<Texture2D> LoadAndGetItem(int itemId)
	{
		Main.instance.LoadItem(itemId);
		
		return TextureAssets.Item[itemId];
	}
}

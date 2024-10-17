using ReLogic.Content;
using Terraria.GameContent;

namespace PathOfTerraria.Common.Utilities;

public static class TextureUtils
{
	public static Asset<Texture2D> LoadAndGetItem(int itemId)
	{
		Main.instance.LoadItem(itemId);
		
		return TextureAssets.Item[itemId];
	}
}

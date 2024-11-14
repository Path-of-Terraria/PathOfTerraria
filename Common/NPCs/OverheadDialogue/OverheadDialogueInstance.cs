using PathOfTerraria.Common.UI.Utilities;
using Terraria.GameContent;
using Terraria.UI.Chat;

namespace PathOfTerraria.Common.NPCs.OverheadDialogue;

internal class OverheadDialogueInstance(string text, int maxLifeTime = 600)
{
	public readonly string FullText = text;
	public readonly int MaxLifeTime = maxLifeTime;
	
	public string Text = string.Empty;
	public int LifeTime = 0;

	public void Update()
	{
		LifeTime++;

		if (LifeTime < MaxLifeTime / 2)
		{
			int length = (int)MathHelper.Lerp(0, FullText.Length, MathF.Min(LifeTime / MathF.Min(MaxLifeTime * 0.5f, FullText.Length * 6f), 1));
			Text = FullText[..length];
		}
		else
		{
			Text = FullText;
		}
	}

	public void Draw(Vector2 position)
	{
		string text = UISimpleWrappableText.WrapText(FontAssets.ItemStack.Value, Text, 300, 1);
		Vector2 size = ChatManager.GetStringSize(FontAssets.ItemStack.Value, text, Vector2.One);
		ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, FontAssets.ItemStack.Value, text, position - new Vector2(0, size.Y / 2), Color.White, 0f, size / 2f, Vector2.One, 300);
	}
}

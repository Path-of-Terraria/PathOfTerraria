using ReLogic.Content;
using Terraria.Localization;
using Terraria.UI;

namespace PathOfTerraria.Content.GUI.Elements;

/// <summary>
///		Provides a <see cref="UIItemSlot"/> with a localized hover tooltip.
/// </summary>
public class UITooltipItemSlot : UIItemSlot
{
	/// <summary>
	///		The localization key of the element's tooltip.
	/// </summary>
	public readonly string Key;

	public UITooltipItemSlot(
		Asset<Texture2D> backgroundTexture,
		Asset<Texture2D> iconTexture,
		string key,
		int context = ItemSlot.Context.InventoryItem
	) : base(backgroundTexture, iconTexture, context)
	{
		Key = key;
	}

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);

		if (!IsMouseHovering)
		{
			return;
		}
		
		Main.instance.MouseText(LanguageManager.Instance.GetOrRegister(Key, () => Key).Value);
	}
}
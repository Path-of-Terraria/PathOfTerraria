using System.Collections.Generic;
using PathOfTerraria.Common.UI.Hotbar.Components;
using Terraria.GameContent;
using Terraria.GameContent.UI;
using Terraria.UI;
using Terraria.UI.Chat;

namespace PathOfTerraria.Common.UI.Hotbar;

/// <summary>
///		The hotbar, which may render multiple hotbar sets.
/// </summary>
internal sealed class HotbarUI : UIElement
{
	public HotbarSet CurrentSet => Sets[selectedSet] ?? throw new InvalidOperationException($"Tried to get hotbar set with invalid index: '{selectedSet}' (set count: {Sets.Count})");

	public SpecialSet SpecialSet { get; }

	public float PaddingBetweenSpecialAndSet { get; set; } = 6f;

	public List<HotbarSet> Sets { get; } = [];

	private int selectedSet;

	public HotbarUI()
	{
		SpecialSet = new SpecialSet(this);
	}

	public void AddSet(HotbarSet set)
	{
		Sets.Add(set);
	}

	protected override void DrawSelf(SpriteBatch spriteBatch)
	{
		base.DrawSelf(spriteBatch);

		DrawMainHotbar(spriteBatch);
		DrawSelectedHotbarSlot(spriteBatch);

		DrawHeldItemName(spriteBatch);
	}

	private void DrawMainHotbar(SpriteBatch sb)
	{
		// Render regular special set.
	}

	private void DrawSelectedHotbarSlot(SpriteBatch sb)
	{
		SpecialS
	}

	private void DrawHeldItemName(SpriteBatch sb)
	{
		// "Item" when no item is held.
		string text = Lang.inter[37].Value;

		if (!string.IsNullOrWhiteSpace(Main.LocalPlayer.HeldItem.Name))
		{
			text = Main.LocalPlayer.HeldItem.AffixName();
		}

		Color nameColor = ItemRarity.GetColor(Main.LocalPlayer.HeldItem.rare);
		var namePos = new Vector2(GetFullWidth() / 2f, 6f);
		namePos.X -= FontAssets.MouseText.Value.MeasureString(text).X * 0.9f / 2f;

		ChatManager.DrawColorCodedStringWithShadow(sb, FontAssets.MouseText.Value, text, namePos, nameColor, 0f, Vector2.Zero, new Vector2(0.9f));
	}

	private float GetFullWidth()
	{
		return SpecialSet.GetFullWidth() + PaddingBetweenSpecialAndSet + CurrentSet.GetFullWidth(); // TODO
	}
}

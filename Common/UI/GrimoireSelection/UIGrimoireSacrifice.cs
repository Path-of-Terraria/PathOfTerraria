using System.Collections.Generic;
using System.Linq;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.UI;

namespace PathOfTerraria.Common.UI.GrimoireSelection;

internal class UIGrimoireSacrifice : UIElement
{
	private Dictionary<int, int> _showItems = null;
	private int _showTime = 0;

	public UIGrimoireSacrifice()
	{
		Width = StyleDimension.FromPixels(350);
		Height = StyleDimension.FromPixels(150);

		Append(new UIImage(ModContent.Request<Texture2D>("PathOfTerraria/Assets/UI/SacrificeBack"))
		{
			Width = StyleDimension.Fill,
			Height = StyleDimension.Fill,
			HAlign = 0.5f,
			VAlign = 0.5f
		});
	}

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);

		_showTime--;
	}

	public void SetHint(Dictionary<int, int> showItems)
	{
		_showItems = showItems;
		_showTime = 180;
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		base.Draw(spriteBatch);

		if (_showTime > 0 && _showItems is not null)
		{
			var dims = GetDimensions().ToRectangle();
			var showItems = new Dictionary<int, int>(_showItems);
			int slot = 0;

			while (showItems.Count > 0)
			{
				KeyValuePair<int, int> first = showItems.First();
				Main.DrawItemIcon(spriteBatch, ContentSamples.ItemsByType[first.Key], dims.Center() + GetSlotPosition(slot), Color.White * (_showTime / 180f), 32);

				showItems[first.Key]--;
				slot++;

				if (showItems[first.Key] <= 0)
				{
					showItems.Remove(first.Key);
				}

				if (slot >= 5)
				{
					break;
				}
			}
		}
	}

	public static Vector2 GetSlotPosition(int slot)
	{
		return slot switch
		{
			0 => new Vector2(0, 50),
			1 => new Vector2(80, 20),
			2 => new Vector2(-80, 20),
			3 => new Vector2(52, -34),
			_ => new Vector2(-52, -34)
		};
	}
}

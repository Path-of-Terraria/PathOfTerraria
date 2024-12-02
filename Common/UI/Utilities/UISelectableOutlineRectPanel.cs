using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace PathOfTerraria.Common.UI.Utilities;

public class UISelectableOutlineRectPanel : UIPanel
{
	public bool IsSelected { get; set; }
	public bool DrawFilled { get; set; } = true;
	public bool DrawBorder { get; set; } = false;

	public int BorderThickness { get; set; } = 1;

	public Color HoverFillColour { get; set; }
	public Color SelectedFillColour { get; set; }

	public Color NormalOutlineColour { get; set; }
	public Color HoverOutlineColour { get; set; }
	public Color SelectedOutlineColour { get; set; }

	public override void MouseOver(UIMouseEvent evt)
	{
		SoundEngine.PlaySound(Terraria.ID.SoundID.MenuTick);
		base.MouseOver(evt);
	}

	protected override void DrawSelf(SpriteBatch spriteBatch)
	{
		if (IsMouseHovering || IsSelected)
		{
			if (DrawFilled)
			{
				spriteBatch.Draw(TextureAssets.BlackTile.Value, GetDimensions().ToRectangle(), IsSelected ? SelectedFillColour : HoverFillColour);
			}

			if (DrawBorder)
			{
				DrawRectangleBorder(spriteBatch, GetDimensions().ToRectangle(), IsSelected ? SelectedOutlineColour : HoverOutlineColour, BorderThickness);
			}
		}
		else if (DrawBorder)
		{
			DrawRectangleBorder(spriteBatch, GetDimensions().ToRectangle(), NormalOutlineColour, BorderThickness);
		}
	}

	private static void DrawRectangleBorder(SpriteBatch spriteBatch, Rectangle rect, Color colour, int thick)
	{
		spriteBatch.Draw(TextureAssets.BlackTile.Value, new Rectangle(rect.X, rect.Y, rect.Width, thick), null, colour);
		spriteBatch.Draw(TextureAssets.BlackTile.Value, new Rectangle(rect.X, rect.Y + 1, thick, rect.Height - 2), null, colour);
		spriteBatch.Draw(TextureAssets.BlackTile.Value, new Rectangle(rect.Right - 1, rect.Y + 1, thick, rect.Height - 2), null, colour);
		spriteBatch.Draw(TextureAssets.BlackTile.Value, new Rectangle(rect.X, rect.Bottom - 1, rect.Width, thick), null, colour);
	}
}
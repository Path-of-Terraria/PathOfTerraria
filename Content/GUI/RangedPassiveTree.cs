using System.Linq;
using PathOfTerraria.Content.Items.Gear;

namespace PathOfTerraria.Content.GUI;

internal class RangedPassiveTree : PassiveTree
{
	public override void Draw(SpriteBatch spriteBatch)
	{
		if (Main.LocalPlayer.controlInv)
		{
			IsVisible = false;
		}

		if (!Populated)
		{
			DrawPanel();
			DrawCloseButton();
			DrawInnerPanel();

			TreeSystem.Nodes
				.Where(x => x.Classes.Contains(PlayerClass.Ranged))
				.ToList()
				.ForEach(n => Inner.Append(new PassiveElement(n)));
			Populated = true;
		}

		Recalculate();
		base.Draw(spriteBatch);
		DrawPanelText(spriteBatch);
	}
}
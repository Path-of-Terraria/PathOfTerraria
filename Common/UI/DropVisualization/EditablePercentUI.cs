﻿using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace PathOfTerraria.Common.UI.DropVisualization;

internal class EditablePercentUI : UIElement
{
	public string DisplayText => $"{Prefix}: {Percent * 100:#0.#}%";
	
	public double Percent { get; private set; }

	private readonly UIText Display = null;
	private readonly string Prefix = null;
	private readonly bool Cap = false;

	private int holdTime = 0;
	private bool resetHold = true;

	public EditablePercentUI(string displayPrefix, float defaultPercent, bool cap)
	{
		Width = StyleDimension.FromPixels(110);
		Height = StyleDimension.FromPixels(70);

		Prefix = displayPrefix;
		Percent = defaultPercent;
		Cap = cap;

		Display = new(DisplayText);
		Append(Display);

		var add = new UIButton<string>("+")
		{
			Width = StyleDimension.FromPixels(40),
			Height = StyleDimension.FromPixels(30),
			Left = StyleDimension.FromPixels(0),
			Top = StyleDimension.FromPixels(30),
		};

		add.OnUpdate += ele => CheckHovering(ele, true);
		Append(add);

		var sub = new UIButton<string>("-")
		{
			Width = StyleDimension.FromPixels(40),
			Height = StyleDimension.FromPixels(30),
			Left = StyleDimension.FromPixels(44),
			Top = StyleDimension.FromPixels(30),
		};

		sub.OnUpdate += ele => CheckHovering(ele, false);
		Append(sub);
	}

	public override void Update(GameTime gameTime)
	{
		resetHold = true;

		base.Update(gameTime);

		Display.SetText(DisplayText);

		if (resetHold)
		{
			holdTime = 0;
		}
	}

	private void CheckHovering(UIElement affectedElement, bool add)
	{
		if (affectedElement.GetOuterDimensions().ToRectangle().Contains(Main.MouseScreen.ToPoint()))
		{
			if (Main.mouseLeft)
			{
				holdTime++;
				resetHold = false;

				if (holdTime == 1 || holdTime > 20 && holdTime % 2 == 0)
				{
					if (add)
					{
						Percent += 0.005;
					}
					else
					{
						Percent -= 0.005;
					}

					Percent = MathHelper.Clamp((float)Percent, 0, Cap ? 1 : float.MaxValue);
				}
			}
		}
	}

	public void SetPercent(double percent)
	{
		Percent = percent;
	}
}

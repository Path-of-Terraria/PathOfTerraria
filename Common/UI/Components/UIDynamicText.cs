using PathOfTerraria.Core.UI;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader.UI;
using Terraria.UI;

#nullable enable

namespace PathOfTerraria.Common.UI.Components;

/// <summary>
/// Makes an element update its text value in real time using the provided delegate in a cached manner.
/// <br/> Supports UIText, UITextBox, UIButton, UITextPanel(string), UIAutoScaleTextTextPanel(string).
/// </summary>
internal sealed class UIDynamicText(Func<string> textGetter) : UIComponent
{
	private readonly Func<string> textGetter = textGetter;

	protected override void OnAttach(UIElement element)
	{
		if (element is not UIText and not UITextPanel<string> and not UIAutoScaleTextTextPanel<string>)
		{
			throw new InvalidOperationException($"{GetType().Name} cannot be used on element of type '{element.GetType().Name}'.");
		}

		element.OnUpdate += OnUpdate;
	}
	protected override void OnDetach(UIElement element)
	{
		element.OnUpdate -= OnUpdate;
	}

	public string GetText()
	{
		return textGetter();
	}

	private void OnUpdate(UIElement element)
	{
		string newText = GetText();

		switch (element)
		{
			case UIText ui:
				if (newText != ui.Text)
				{
					ui.SetText(newText);
				}

				break;
			case UITextPanel<string> ui:
				if (newText != ui.Text)
				{
					ui.SetText(newText);
				}

				break;
			case UIAutoScaleTextTextPanel<string> ui:
				if (newText != ui.Text)
				{
					ui.SetText(newText);
				}

				break;
		}
	}
}

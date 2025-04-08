using System.Collections.Generic;
using System.Linq;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.UI;

namespace PathOfTerraria.Common.UI.Utilities;

// ReSharper disable once InconsistentNaming
public class UITabsPanel : UICloseablePanel
{
	private int _uiDelay = -1;

	private readonly UIPanel _header;
	private readonly Dictionary<string, UIPanelTab> _menus;
	public string ActiveTab = "";
	public event Action OnActiveTabChanged;

	public UITabsPanel(bool stopItemUse, bool showCloseButton, IEnumerable<(string key, LocalizedText text)> menuOptions, int panelHeight)
		: base(stopItemUse, showCloseButton, false, false)
	{
		StopItemUse = stopItemUse;

		SetPadding(0);

		_header = new UIPanel();
		_header.SetPadding(0);
		_header.Width.Set(0, 1f);
		_header.Height.Set(panelHeight, 0f);
		_header.BackgroundColor.A = 255;
		Append(_header);

		if (showCloseButton)
		{
			var closeButton = new UITextPanel<char>('X');
			closeButton.SetPadding(7);
			closeButton.Width.Set(40, 0);
			closeButton.Left.Set(-40, 1);
			closeButton.BackgroundColor.A = 255;
			_header.Append(closeButton);
		}

		UIPanel viewArea = new();
		viewArea.Top.Set(38, 0);
		viewArea.Width.Set(0, 1f);
		viewArea.Height.Set(-38, 1f);
		viewArea.BackgroundColor = Color.Transparent;
		viewArea.BorderColor = Color.Transparent;
		Append(viewArea);

		_menus = new Dictionary<string, UIPanelTab>();

		float left = 0;

		foreach ((string key, LocalizedText text) in menuOptions)
		{
			UIPanelTab menu;
			_menus.Add(key, menu = new UIPanelTab(key, text));
			menu.SetPadding(7);
			menu.Left.Set(left, 0f);
			menu.BackgroundColor.A = 255;
			menu.Recalculate();
			menu.UseImmediateMode = true;
			menu.OnLeftClick += (evt, element) =>
			{
				SoundEngine.PlaySound(SoundID.MenuTick);
				SetActivePage(key);
			};

			left += menu.GetDimensions().Width + 10;

			_header.Append(menu);
		}

		MinSize.X = Math.Max(MinSize.X, left);

		SetActivePage(_menus.Keys.First());
	}

	public void SetActivePage(string page)
	{
		foreach (UIPanelTab tab in _menus.Values)
		{
			tab.TextColor = Color.White;
		}

		if (_menus.TryGetValue(page, out UIPanelTab menuText))
		{
			menuText.TextColor = Color.Yellow;
		}
		
		ActiveTab = page;
		OnActiveTabChanged?.Invoke();
	}

	public void HideTab(string page)
	{
		if (_menus.TryGetValue(page, out UIPanelTab tab))
		{
			tab.Remove();
			RecalculateTabPositions();
		}
	}

	public void ShowTab(string page)
	{
		if (_menus.TryGetValue(page, out UIPanelTab tab) && tab.Parent is null)
		{
			tab.Remove();
			_header.Append(tab);

			RecalculateTabPositions();
		}
	}

	private void RecalculateTabPositions()
	{
		float left = 0;

		foreach (UIPanelTab tab in _menus.Values)
		{
			if (tab.Parent is null)
			{
				continue;
			}

			tab.Left.Set(left, 0f);
			tab.Recalculate();

			left += tab.GetDimensions().Width + 10;
		}
	}

	public override void Update(GameTime gameTime)
	{
		if (_uiDelay > 0)
		{
			_uiDelay--;
		}

		// clicks on this UIElement dont cause the player to use current items. 
		if (ContainsPoint(Main.MouseScreen) && StopItemUse)
		{
			Main.LocalPlayer.mouseInterface = true;
		}

		base.Update(gameTime); // don't remove.

		Blocked = Main.blockMouse;
	}
}
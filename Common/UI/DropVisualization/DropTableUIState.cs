#if DEBUG
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using PathOfTerraria.Common.UI.DropVisualization;
using PathOfTerraria.Common.UI.Utilities;
using PathOfTerraria.Core.UI.SmartUI;
using ReLogic.Localization.IME;
using ReLogic.OS;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace PathOfTerraria.Common.UI.GrimoireSelection;

internal class DropTablePlayer : ModPlayer
{
	public static ModKeybind ToggleDropTableUIKey = null;

	public override void Load()
	{
		if (Main.dedServ)
		{
			return;
		}

		ToggleDropTableUIKey = KeybindLoader.RegisterKeybind(Mod, "DropTableKey", Keys.OemBackslash);
	}

	public override void ProcessTriggers(TriggersSet triggersSet)
	{
		if (ToggleDropTableUIKey.JustPressed)
		{
			SmartUiLoader.GetUiState<DropTableUIState>().Toggle();
		}
	}
}

internal class DropTableUIState : CloseableSmartUi
{
	public static readonly Point MainPanelSize = new(900, 550);

	public override bool IsCentered => true;

	private EditablePercentUI _gearRate = null;
	private EditablePercentUI _currencyRate = null;
	private EditablePercentUI _mapRate = null;

	public override int InsertionIndex(List<GameInterfaceLayer> layers)
	{
		return layers.FindIndex(x => x.Name == "Vanilla: Hotbar");
	}

	internal void Toggle()
	{
		RemoveAllChildren();

		Width = StyleDimension.Fill;
		Height = StyleDimension.Fill;

		IsVisible = !IsVisible;

		if (!IsVisible)
		{
			return;
		}

		Main.playerInventory = true;

		CreateMainPanel(false, MainPanelSize, false, true);
		Panel.VAlign = 0.7f;

		BuildChanceSelector(Panel);
		BuildDisplay(Panel);

		CloseButton = new UIImageButton(ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/CloseButton"));
		CloseButton.Left.Set(-PointsAndExitPadding - 38, 1);
		CloseButton.Top.Set(10, 0f);
		CloseButton.Width.Set(38, 0);
		CloseButton.Height.Set(38, 0);
		CloseButton.OnLeftClick += (a, b) =>
		{
			Toggle();
			SoundEngine.PlaySound(SoundID.MenuClose, Main.LocalPlayer.Center);
		};
		CloseButton.SetVisibility(1, 1);
		Panel.Append(CloseButton);

		Recalculate();
	}

	private void BuildDisplay(UICloseablePanel panel)
	{
		var topPanel = new UIPanel()
		{
			Width = StyleDimension.Fill,
			Height = StyleDimension.FromPixelsAndPercent(-84, 1),
			VAlign = 1f
		};

		panel.Append(topPanel);
	}

	private void BuildChanceSelector(UICloseablePanel panel)
	{
		var topPanel = new UIPanel()
		{
			Width = StyleDimension.Fill,
			Height = StyleDimension.FromPixels(80),
		};

		panel.Append(topPanel);

		_gearRate = new("Gear", 0.80f, false);
		topPanel.Append(_gearRate);

		_currencyRate = new("Currency", 0.15f, false)
		{
			Left = StyleDimension.FromPixels(120)
		};
		topPanel.Append(_currencyRate);

		_mapRate = new("Map", 0.05f, false)
		{
			Left = StyleDimension.FromPixels(256)
		};
		topPanel.Append(_mapRate);

		var normalize = new UIButton<string>("Normalize")
		{
			Width = StyleDimension.FromPixels(100),
			Height = StyleDimension.FromPixels(60),
			Left = StyleDimension.FromPixels(350)
		};

		normalize.OnLeftClick += ClickNormalize;
		topPanel.Append(normalize);

		var run = new UIButton<string>("Run")
		{
			Width = StyleDimension.FromPixels(100),
			Height = StyleDimension.FromPixels(60),
			Left = StyleDimension.FromPixels(454)
		};

		topPanel.Append(run);
	}

	private void ClickNormalize(UIMouseEvent evt, UIElement listeningElement)
	{
		double total = _gearRate.Percent + _currencyRate.Percent + _mapRate.Percent;
		_gearRate.SetPercent(_gearRate.Percent / total);
		_currencyRate.SetPercent(_currencyRate.Percent / total);
		_mapRate.SetPercent(_mapRate.Percent / total);
	}
}
#endif

using System.Diagnostics;
using PathOfTerraria.Common.UI.Utilities;
using PathOfTerraria.Core.UI;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace PathOfTerraria.Common.UI.Misc;

internal class OnlineModPopupUI : UIState
{
	public const string Identifier = "PathOfTerraria Online Popup";
	private const string WorkshopUrl = "https://steamcommunity.com/sharedfiles/filedetails/?id=3399082901";

	public override void OnInitialize()
	{
		UIPanel panel = new()
		{
			VAlign = 0.25f,
			HAlign = 0.5f,
			Width = StyleDimension.FromPixels(700),
			Height = StyleDimension.FromPixels(200),
		};
		Append(panel);

		UIText title = new(Language.GetText("Mods.PathOfTerraria.UI.OnlineModPopup.Title"))
		{
			Top = StyleDimension.FromPixels(12),
			HAlign = 0.5f,
		};
		panel.Append(title);

		UISimpleWrappableText description = new(Language.GetTextValue("Mods.PathOfTerraria.UI.OnlineModPopup.Description"), 0.9f)
		{
			Top = StyleDimension.FromPixels(46),
			Left = StyleDimension.FromPixels(24),
			Width = StyleDimension.FromPixels(652),
			Height = StyleDimension.FromPixels(88),
		};
		description.Wrappable = true;
		panel.Append(description);

		UIButton<string> openWorkshopButton = new(Language.GetText("Mods.PathOfTerraria.UI.OnlineModPopup.OpenWorkshop").Value)
		{
			Width = StyleDimension.FromPixels(220),
			Height = StyleDimension.FromPixels(36),
			Top = StyleDimension.FromPixels(148),
			Left = StyleDimension.FromPixels(24),
			TextColor = Color.LightGreen
		};

		openWorkshopButton.OnLeftClick += OpenWorkshop;
		panel.Append(openWorkshopButton);

		UIButton<string> dismissButton = new(Language.GetText("Mods.PathOfTerraria.UI.OnlineModPopup.Decline").Value)
		{
			Width = StyleDimension.FromPixels(140),
			Height = StyleDimension.FromPixels(36),
			Left = StyleDimension.FromPixels(252),
			Top = StyleDimension.FromPixels(148),
			TextColor = Color.IndianRed
		};

		dismissButton.OnLeftClick += ClosePopup;
		panel.Append(dismissButton);
	}

	private static void OpenWorkshop(UIMouseEvent evt, UIElement listeningElement)
	{
		TryOpenWorkshopPage();
		ClosePopup(evt, listeningElement);
	}

	private static void ClosePopup(UIMouseEvent evt, UIElement listeningElement)
	{
		_ = evt;
		_ = listeningElement;

		Main.LocalPlayer.GetModPlayer<OnlineModPopupPlayer>().DismissedPopup = true;
		UIManager.TryDisable(Identifier);
	}

	private static void TryOpenWorkshopPage()
	{
		try
		{
			Process.Start(new ProcessStartInfo
			{
				FileName = WorkshopUrl,
				UseShellExecute = true
			});
		}
		catch (Exception exception)
		{
			PoTMod.Instance.Logger.Warn("Failed to open PathOfTerrariaOnline workshop page.", exception);
		}
	}

	public override void OnDeactivate()
	{
		RemoveAllChildren();
	}
}

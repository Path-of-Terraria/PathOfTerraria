using PathOfTerraria.Content.GUI.Utilities;
using PathOfTerraria.Content.Items.Gear.Weapons.Battleaxe;
using PathOfTerraria.Content.Items.Gear.Weapons.Grimoire;
using PathOfTerraria.Core.Systems.ModPlayers;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;

namespace PathOfTerraria.Content.GUI.GrimoireSelection;

internal class GrimoireSelectionUIState : CloseableSmartUi
{
	public override bool IsCentered => true;

	public override int InsertionIndex(List<GameInterfaceLayer> layers)
	{
		return layers.FindIndex(x => x.Name == "Vanilla: Hotbar");
	}

	private static UIGrid _storageGrid = null;

	internal void Toggle()
	{
		RemoveAllChildren();

		IsVisible = !IsVisible;

		if (!IsVisible)
		{
			_storageGrid.Clear();
			_storageGrid = null;
			return;
		}

		CreateMainPanel(false, new Point(900, 550), false, true);
		BuildSummonSelect(Panel);
		BuildStorage(Panel);
		BuildSacrifice(Panel);

		CloseButton = new UIImageButton(ModContent.Request<Texture2D>($"{PathOfTerraria.ModName}/Assets/GUI/CloseButton"));
		CloseButton.Left.Set(-38 - PointsAndExitPadding, 1f);
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

	private static void BuildSacrifice(UICloseablePanel panel)
	{
		panel.Append(new UIImage(ModContent.Request<Texture2D>("PathOfTerraria/Assets/GUI/SacrificeBack"))
		{
			Width = StyleDimension.FromPixels(350),
			Height = StyleDimension.FromPixels(150),
			HAlign = 0.86f,
			VAlign = 0.9f
		});
	}

	private static void BuildSummonSelect(UICloseablePanel panel)
	{
		var mainPanel = new UIPanel()
		{
			Width = StyleDimension.FromPixels(498),
			Height = StyleDimension.FromPixels(300),
			HAlign = 1f,
		};
		panel.Append(mainPanel);

		mainPanel.Append(new UIImage(TextureAssets.Item[ModContent.ItemType<GrimoireItem>()])
		{
			VAlign = -0.1f,
			HAlign = 0.5f
		});
	}

	private static void BuildStorage(UICloseablePanel panel)
	{
		var mainPanel = new UIPanel()
		{
			Width = StyleDimension.FromPixels(398),
			Height = StyleDimension.Fill,
		};
		panel.Append(mainPanel);

		mainPanel.Append(new UIText("Storage", 1, true)
		{
			HAlign = 0.5f,
		});

		var storagePanel = new UIPanel()
		{
			Height = StyleDimension.FromPixelsAndPercent(-54, 1f),
			Width = StyleDimension.FromPixelsAndPercent(-24, 1),
			VAlign = 1f
		};
		mainPanel.Append(storagePanel);

		_storageGrid = new UIGrid()
		{
			Width = StyleDimension.FromPixelsAndPercent(0, 1),
			Height = StyleDimension.Fill,
		};
		storagePanel.Append(_storageGrid);

		var scrollBar = new UIScrollbar()
		{
			Width = StyleDimension.FromPixels(20),
			Height = StyleDimension.FromPixelsAndPercent(-54, 1f),
			HAlign = 1f,
			VAlign = 1f
		};
		mainPanel.Append(scrollBar);
		_storageGrid.SetScrollbar(scrollBar);

		UpdateGrid();
	}

	internal static void UpdateGrid()
	{
		_storageGrid.Clear();

		foreach (Item item in Main.LocalPlayer.GetModPlayer<GrimoireStoragePlayer>().Storage)
		{
			_storageGrid.Add(new UIItemIcon(item, false)
			{
				Width = StyleDimension.FromPixels(31),
				Height = StyleDimension.FromPixels(31)
			});
		}

		_storageGrid.Recalculate();
		_storageGrid.Recalculate();
	}
}

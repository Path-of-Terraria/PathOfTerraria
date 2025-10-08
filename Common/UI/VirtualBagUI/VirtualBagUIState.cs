using PathOfTerraria.Common.Systems;
using PathOfTerraria.Core.Items;
using PathOfTerraria.Core.UI;
using SubworldLibrary;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;

namespace PathOfTerraria.Common.UI.VirtualBagUI;

internal class VirtualBagUIState : UIState
{
	public const string Identifier = "Virtual Bag UI";
	private UIGrid _storageGrid;

	public override void OnActivate()
	{
		const string TexturePath = "PathOfTerraria/Assets/UI/SquarePanel";
		const int GridBuffer = 50;
		const string Path = "Mods.PathOfTerraria.UI.VirtualBag.";

		UIPanel panel = new();
		panel.SetDimensions((0f, 0), (0, 0), (0, 800), (0, 500));
		panel.VAlign = 0.5f;
		panel.HAlign = 0.5f;
		panel.OnUpdate += BlockClicks;
		Append(panel);

		panel.Append(new UIText(Language.GetText(Path + "Title"), 0.6f, true) { Top = StyleDimension.FromPixels(6), Left = StyleDimension.FromPixels(6) });

		string desc = Main.LocalPlayer.GetModPlayer<VirtualBagStoragePlayer>().ConfirmedExit ? "Review" : "Description";
		panel.Append(new UIText(Language.GetText(Path + desc), 0.9f) { HAlign = 1f, Left = StyleDimension.FromPixels(-50), Top =	StyleDimension.FromPixels(6) });

		var close = new UIImageButton(ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/CloseButton")) { HAlign = 1f };
		close.SetVisibility(1, 0.6f);
		close.SetDimensions((0, 0), (0, 0), (0, 38), (0, 38));
		close.OnLeftClick += Close;
		panel.Append(close);

		UIPanel gridPanel = new(ModContent.Request<Texture2D>(TexturePath + "Background"), ModContent.Request<Texture2D>(TexturePath + "Outline"));
		gridPanel.SetDimensions((0, 0), (0, GridBuffer), (1, -30), (1, -GridBuffer));
		panel.Append(gridPanel);

		_storageGrid = [];
		_storageGrid.SetDimensions((0, 0), (0, 0), (1, 0), (1, 0));
		gridPanel.Append(_storageGrid);

		UIScrollbar bar = new();
		bar.SetDimensions((0, 0), (0, GridBuffer), (0, 20), (1, -GridBuffer));
		bar.HAlign = 1f;
		_storageGrid.SetScrollbar(bar);
		panel.Append(bar);

		RefreshStorage();
	}

	private void Close(UIMouseEvent evt, UIElement listeningElement)
	{
		SoundEngine.PlaySound(SoundID.MenuClose, Main.LocalPlayer.Center);
		UIManager.TryToggleOrRegister(Identifier, "Vanilla: Mouse Text", new VirtualBagUIState(), 0, InterfaceScaleType.UI);
	}

	private void BlockClicks(UIElement affectedElement)
	{
		if (affectedElement.ContainsPoint(Main.MouseScreen))
		{
			Main.LocalPlayer.mouseInterface = true;
		}
	}

	public override void OnDeactivate()
	{
		RemoveAllChildren();

		if (Main.LocalPlayer.GetModPlayer<VirtualBagStoragePlayer>().ConfirmedExit)
		{
			SubworldSystem.Exit();
		}
	}

	internal void RefreshStorage()
	{
		_storageGrid.Clear();

		foreach (Item item in Main.LocalPlayer.GetModPlayer<VirtualBagStoragePlayer>().Storage)
		{
			var storageIcon = new UIItemIcon(item, false)
			{
				Width = StyleDimension.FromPixels(36),
				Height = StyleDimension.FromPixels(36)
			};
			_storageGrid.Add(storageIcon);

			var src = new Rectangle(20 * (int)item.GetInstanceData().Rarity, 0, 18, 18);
			var itemRarity = new UIImageFramed(ModContent.Request<Texture2D>("PathOfTerraria/Assets/UI/ItemRarityIcons"), src) 
			{ 
				Left = StyleDimension.FromPixels(16),
				Top = StyleDimension.FromPixels(20)
			};
			
			storageIcon.Append(itemRarity);

			storageIcon.OnRightClick += (_, self) => AddItemToInv(item);
			storageIcon.OnUpdate += (self) =>
			{
				if (self.ContainsPoint(Main.MouseScreen))
				{
					UpdateSlot(item);
				}
			};
		}

		_storageGrid.Recalculate();
		_storageGrid.Recalculate();
	}

	private void AddItemToInv(Item item)
	{
		Player plr = Main.LocalPlayer;
		bool hasModified = false;

		for (int i = 0; i < Main.InventoryItemSlotsCount; i++)
		{
			ref Item invItem = ref plr.inventory[i];

			if (invItem.IsAir)
			{
				invItem = item.Clone();
				hasModified = true;
				break;
			}
			else if (invItem.type == item.type && invItem.stack < invItem.maxStack)
			{
				int newStack = Math.Min(invItem.maxStack, invItem.stack + item.stack);
				item.stack = newStack - invItem.stack;
				invItem.stack = newStack;

				if (item.stack <= 0)
				{
					item.TurnToAir();
					hasModified = true;
					break;
				}
			}
		}
		
		if (!hasModified)
		{
			return;
		}

		plr.GetModPlayer<VirtualBagStoragePlayer>().Storage.Remove(item);
		RefreshStorage();
		SoundEngine.PlaySound(SoundID.Grab);
	}

	public static void UpdateSlot(Item item)
	{
		if (item.IsAir || item.type == ItemID.None)
		{
			return;
		}

		if (ItemSlot.ShiftInUse)
		{
			Main.cursorOverride = 9;
		}

		List<DrawableTooltipLine> lines = ItemTooltipBuilder.BuildTooltips(item, Main.LocalPlayer);
		float factor = MathF.Sin(Main.GameUpdateCount * 0.08f) * 0.5f + 0.5f;
		var color = Color.Lerp(Color.White, ItemTooltips.Colors.Positive, factor);
		var baseTip = new TooltipLine(PoTMod.Instance, "ClickNotice", Language.GetTextValue("Mods.PathOfTerraria.UI.RightClick"));
		lines.Add(new DrawableTooltipLine(baseTip, lines.Count, 0, 0, color) { BaseScale = new(0.9f) });

		int nameLine = lines.FindIndex(x => x.Name == "ItemName");
		DrawableTooltipLine newLine = new(new TooltipLine(PoTMod.Instance, "ItemName", lines[nameLine].Text + $" (x{item.stack})"), 0, 0, 0, lines[nameLine].Color);
		lines[nameLine] = newLine;

		Tooltip.Create(new TooltipDescription
		{
			Identifier = "VirtualBagTooltip",
			AssociatedItem = item,
			Lines = lines,
		});
	}
}

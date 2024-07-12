﻿using PathOfTerraria.Content.GUI.Utilities;
using PathOfTerraria.Content.Items.Gear.Weapons.Grimoire;
using PathOfTerraria.Content.Projectiles.Summoner;
using PathOfTerraria.Core.Systems.ModPlayers;
using ReLogic.Content;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
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

	public static Asset<Texture2D> EmptySummonTexture = null;

	public static Item EmptyItem
	{
		get 
		{
			var item = new Item(ItemID.None);
			item.TurnToAir();
			return item;
		}
	}

	private readonly static Item[] _parts = [EmptyItem, EmptyItem, EmptyItem, EmptyItem, EmptyItem];

	private static UIGrid _storageGrid = null;
	private static UIGrid _summonGrid = null;
	private static UIGrimoireSacrifice _sacrificePanel = null;
	private static UIImage _currentSummon = null;

	public override void SafeUpdate(GameTime gameTime)
	{
		base.SafeUpdate(gameTime);

		foreach (var item in _parts)
		{

		}

		if (IsVisible && (Main.LocalPlayer.HeldItem.ModItem is not GrimoireItem || !Main.playerInventory))
		{
			Toggle();
		}
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		float invSize = Main.inventoryScale;
		Main.inventoryScale = 0.8f;
		base.Draw(spriteBatch);
		Main.inventoryScale = invSize;
	}

	internal void Toggle()
	{
		RemoveAllChildren();

		EmptySummonTexture ??= ModContent.Request<Texture2D>("PathOfTerraria/Assets/Projectiles/Summoner/GrimoireSummons/Empty_Icon");
		IsVisible = !IsVisible;

		for (int i = 0; i < _parts.Length; i++)
		{
			_parts[i] = Main.LocalPlayer.GetModPlayer<GrimoireSummonPlayer>().StoredParts[i];
		}

		if (!IsVisible)
		{
			_storageGrid.Clear();
			_storageGrid = null;
			return;
		}

		Main.playerInventory = true;

		CreateMainPanel(false, new Point(900, 550), false, true);
		Panel.VAlign = 0.7f;
		BuildSummonSelect(Panel);
		BuildStorage(Panel);
		BuildSacrifice(Panel);

		CloseButton = new UIImageButton(ModContent.Request<Texture2D>($"{PathOfTerraria.ModName}/Assets/GUI/CloseButton"));
		CloseButton.Left.Set(PointsAndExitPadding, 0);
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
		_sacrificePanel = new UIGrimoireSacrifice()
		{
			HAlign = 0.86f,
			VAlign = 0.9f
		};
		panel.Append(_sacrificePanel);

		_currentSummon = new UIImage(EmptySummonTexture)
		{
			Width = StyleDimension.FromPixels(32),
			Height = StyleDimension.FromPixels(32),
			HAlign = 0.5f,
			VAlign = -0.2f
		};
		_sacrificePanel.Append(_currentSummon);
		SetCurrentSummonImage();

		for (int i = 0; i < 5; ++i)
		{
			var pos = UIGrimoireSacrifice.GetSlotPosition(i).ToPoint();
			var slot = new UIItemSlot(_parts, i, ItemSlot.Context.ChestItem)
			{
				Width = StyleDimension.FromPixels(32),
				Height = StyleDimension.FromPixels(32),
				HAlign = 0.5f,
				VAlign = 0.5f,
				Left = StyleDimension.FromPixels(pos.X),
				Top = StyleDimension.FromPixels(pos.Y),
			};

			slot.OnLeftClick += PutSlotItemInStorage;
			_sacrificePanel.Append(slot);
		}
	}

	private static void PutSlotItemInStorage(UIMouseEvent evt, UIElement listeningElement)
	{
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

		mainPanel.Append(new UIImage(ModContent.Request<Texture2D>($"{PathOfTerraria.ModName}/Assets/GUI/GrimoireButton"))
		{
			VAlign = -0.24f,
			HAlign = 0.5f,
			Width = StyleDimension.FromPixels(64),
			Height = StyleDimension.FromPixels(64),
		});

		var gridPanel = new UIPanel()
		{
			Width = StyleDimension.FromPixelsAndPercent(-24, 1),
			Height = StyleDimension.FromPixelsAndPercent(-20, 1),
			VAlign = 1f
		};
		mainPanel.Append(gridPanel);

		_summonGrid = new()
		{
			Width = StyleDimension.Fill,
			Height = StyleDimension.Fill
		};
		gridPanel.Append(_summonGrid);

		var scrollBar = new UIScrollbar()
		{
			Width = StyleDimension.FromPixels(20),
			Height = StyleDimension.FromPixelsAndPercent(-20, 1f),
			HAlign = 1f,
			VAlign = 1f
		};
		mainPanel.Append(scrollBar);
		_summonGrid.SetScrollbar(scrollBar);

		foreach (int item in ModContent.GetInstance<GrimoireSummonLoader>().SummonIds)
		{
			var summon = ContentSamples.ProjectilesByType[item].ModProjectile as GrimoireSummon;

			var summonIcon = new UIColoredImageButton(GrimoireSummon.IconsById[summon.Type])
			{
				Width = StyleDimension.FromPixels(36),
				Height = StyleDimension.FromPixels(36)
			};
			_summonGrid.Add(summonIcon);

			summonIcon.OnUpdate += (self) => UpdateSummonIcon(self as UIColoredImageButton, summon);
			summonIcon.OnLeftClick += (_, _) => ClickSummon(summon);
		}

		_summonGrid.Recalculate();
	}

	private static void ClickSummon(GrimoireSummon summon)
	{
		GrimoireSummonPlayer summoner = Main.LocalPlayer.GetModPlayer<GrimoireSummonPlayer>();

		if (summoner.HasSummon(summon.Type))
		{
			summoner.CurrentSummonId = summoner.CurrentSummonId == summon.Type ? -1 : summon.Type;
			SetCurrentSummonImage();
			return;
		}

		// Kinda messy code that checks for unlocks
		Dictionary<int, int> storage = Main.LocalPlayer.GetModPlayer<GrimoireStoragePlayer>().GetStoredCount();
		bool cantUnlock = false;
		Dictionary<int, int> requirements = ModContent.GetInstance<GrimoireSummonLoader>().RequiredPartsByProjectileId[summon.Type];

		foreach (KeyValuePair<int, int> item in requirements)
		{
			if (!storage.TryGetValue(item.Key, out int value) || value < item.Value)
			{
				cantUnlock = true;
				break;
			}
		}

		if (cantUnlock)
		{
			_sacrificePanel.SetHint(requirements);
		}
		else
		{
			summoner.UnlockSummon(summon.Type);

			for (int i = 0; i < 16; ++i)
			{
				Dust.NewDust(Main.LocalPlayer.position, Player.defaultWidth, Player.defaultHeight, DustID.Confetti + Main.rand.Next(4), 0, -6);
			}
		}
	}

	private static void SetCurrentSummonImage()
	{
		int id = Main.LocalPlayer.GetModPlayer<GrimoireSummonPlayer>().CurrentSummonId;
		_currentSummon.SetImage(id == -1 ? EmptySummonTexture : GrimoireSummon.IconsById[id]);
		_currentSummon.Recalculate();
	}

	private static void UpdateSummonIcon(UIColoredImageButton self, GrimoireSummon item)
	{
		if (Main.LocalPlayer.GetModPlayer<GrimoireSummonPlayer>().HasSummon(item.Type))
		{
			self.SetColor(Color.White);
		}
		else
		{
			self.SetColor(new Color(100, 100, 100));
		}

		if (!self.GetDimensions().ToRectangle().Contains(Main.MouseScreen.ToPoint()))
		{
			return;
		}

		Tooltip.SetName(item.DisplayName.Value);
		Tooltip.SetTooltip(Language.GetTextValue($"Mods.{item.Mod.Name}.Projectiles.{item.Name}.Description"));
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

		RefreshStorage();
	}

	internal static void RefreshStorage()
	{
		_storageGrid.Clear();

		foreach (Item item in Main.LocalPlayer.GetModPlayer<GrimoireStoragePlayer>().Storage)
		{
			var storageIcon = new UIItemIcon(item, false)
			{
				Width = StyleDimension.FromPixels(31),
				Height = StyleDimension.FromPixels(31)
			};
			_storageGrid.Add(storageIcon);

			storageIcon.OnUpdate += (self) => HoverOverStorage(self, item);
			storageIcon.OnLeftClick += (_, self) => ClickStorageItem(item);
		}

		_storageGrid.Recalculate();
		_storageGrid.Recalculate();
	}

	private static void ClickStorageItem(Item item)
	{
		GrimoireSummonPlayer grimSummoner = Main.LocalPlayer.GetModPlayer<GrimoireSummonPlayer>();
		GrimoireStoragePlayer storagePlayer = Main.LocalPlayer.GetModPlayer<GrimoireStoragePlayer>();

		for (int i = 0; i < _parts.Length; ++i)
		{
			Item part = _parts[i];

			if (part == item)
			{
				_parts[i] = EmptyItem.Clone();
				break;
			}

			if (part.IsAir)
			{
				storagePlayer.Storage.Remove(item);
				_parts[i] = item;
				RefreshStorage();
				break;
			}
		}

		_parts.CopyTo(grimSummoner.StoredParts, 0);
	}

	private static void HoverOverStorage(UIElement self, Item item)
	{
		if (!self.GetDimensions().ToRectangle().Contains(Main.MouseScreen.ToPoint()))
		{
			return;
		}

		Tooltip.DrawWidth = 500;
		Tooltip.SetName(item.Name);
		string tooltip = string.Empty;

		List<TooltipLine> moddedTooltips = [];
		item.ModItem.ModifyTooltips(moddedTooltips);

		foreach (TooltipLine line in moddedTooltips)
		{
			if (line.Text.Trim() == string.Empty || line.FullName == "PathOfTerraria/Name")
			{
				continue;
			}

			tooltip += line.Text + "\n";
		}

		for (int i = 0; i < item.ToolTip.Lines; ++i)
		{
			string line = item.ToolTip.GetLine(i);

			if (line.Trim() == string.Empty)
			{
				continue;
			}

			tooltip += line + "\n";
		}

		Tooltip.SetTooltip(tooltip);
	}
}

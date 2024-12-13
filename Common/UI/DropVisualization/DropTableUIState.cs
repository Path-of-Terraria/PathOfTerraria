#if DEBUG
using System.Collections.Generic;
using System.Linq;
using log4net.Core;
using Microsoft.Xna.Framework.Input;
using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.UI.DropVisualization;
using PathOfTerraria.Common.UI.Utilities;
using PathOfTerraria.Core.Items;
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

internal class DropResult(int count, float percent)
{
	public int Count = count;
	public float Percent = percent;
	public bool IsUnique = false;
	public Dictionary<ItemRarity, int> CountsPerRarity = [];

	public void IncrementRarityCount(ItemRarity rarity)
	{
		if (!CountsPerRarity.ContainsKey(rarity))
		{
			CountsPerRarity.Add(rarity, 0);
		}

		CountsPerRarity[rarity]++;
	}
}

internal class DropTableUIState : CloseableSmartUi
{
	public static readonly Point MainPanelSize = new(900, 550);

	public override bool IsCentered => true;

	private EditableValueUI _gearRate = null;
	private EditableValueUI _currencyRate = null;
	private EditableValueUI _mapRate = null;
	private EditableValueUI _count = null;
	private EditableValueUI _level = null;
	private UIList _resultList = null;

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

		BuildModificationPanel(Panel);
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
		var bottomPanel = new UIPanel()
		{
			Width = StyleDimension.Fill,
			Height = StyleDimension.FromPixelsAndPercent(-84, 1),
			VAlign = 1f
		};

		panel.Append(bottomPanel);

		_resultList = new UIList()
		{
			Width = StyleDimension.Fill,
			Height = StyleDimension.Fill,
			VAlign = 1f
		};

		bottomPanel.Append(_resultList);

		UIScrollbar bar = new()
		{
			Width = StyleDimension.FromPixels(20),
			Height = StyleDimension.Fill,
			HAlign = 1f,
		};

		_resultList.SetScrollbar(bar);
		bottomPanel.Append(bar);
	}

	private void BuildModificationPanel(UICloseablePanel panel)
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

		_count = new("Count", 10f, false, 0.05, false)
		{
			Left = StyleDimension.FromPixels(484)
		};
		topPanel.Append(_count);

		_level = new("Level", 0.05f, false, 0.01, false)
		{
			Left = StyleDimension.FromPixels(588)
		};
		topPanel.Append(_level);

		var run = new UIButton<string>("Run")
		{
			Width = StyleDimension.FromPixels(100),
			Height = StyleDimension.FromPixels(60),
			Left = StyleDimension.FromPixels(770 - PointsAndExitPadding * 3)
		};

		run.OnLeftClick += RunDatabase;
		topPanel.Append(run);
	}

	private void RunDatabase(UIMouseEvent evt, UIElement listeningElement)
	{
		_resultList.Clear();

		int count = (int)(_count.Value * 100);
		float dropRarityMod = 0f;
		Dictionary<int, DropResult> resultsById = [];
		int checkCount = 0;

		var filteredGear = ItemDatabase.AllItems.Where(g =>
		{
			PoTStaticItemData staticData = ContentSamples.ItemsByType[g.ItemId].GetStaticData();
			return staticData.MinDropItemLevel <= (_level.Value * 100);
		}).ToList();

		for (int i = 0; i < count; ++i)
		{
			float dropChanceSum = filteredGear.Sum((ItemDatabase.ItemRecord x) =>
				ItemDatabase.ApplyRarityModifier(x.DropChance, dropRarityMod));
			float choice = Main.rand.NextFloat(dropChanceSum);
			float cumulativeChance = 0;

			foreach (ItemDatabase.ItemRecord item in filteredGear)
			{
				cumulativeChance += ItemDatabase.ApplyRarityModifier(item.DropChance, dropRarityMod);

				if (choice < cumulativeChance)
				{
					if (resultsById.TryGetValue(item.ItemId, out DropResult result))
					{
						result.Count++;
					}
					else
					{
						resultsById.Add(item.ItemId, result = new DropResult(1, 0));
					}

					if (item.Rarity == ItemRarity.Unique)
					{
						result.IsUnique = true;
					}
					else
					{
						result.IncrementRarityCount(item.Rarity);
					}

					checkCount++;
					break;
				}

				if (checkCount >= count)
				{
				}
			}

			if (checkCount >= count)
			{
				break;
			}
		}

		foreach (KeyValuePair<int, DropResult> item in resultsById)
		{
			var text = new UIText($"[i:{item.Key}] {Lang.GetItemNameValue(item.Key)}:");
			_resultList.Add(text);
			text.Append(new UIText($"#: {item.Value.Count}") { Left = StyleDimension.FromPixels(220) });
			text.Append(new UIText($"%: {item.Value.Count / (float)count * 100f:#0.#}%") { Left = StyleDimension.FromPixels(280) });

			int xOff = 370;

			if (item.Value.IsUnique)
			{
				text.Append(new UIText("(Unique Item)") { Left = StyleDimension.FromPixels(xOff) });
				xOff += 120;
			}
			else
			{
				foreach (KeyValuePair<ItemRarity, int> value in item.Value.CountsPerRarity)
				{
					text.Append(new UIText($"{value.Key.ToString()[..3]}: {value.Value / (float)item.Value.Count * 100f:#0.#}%") { Left = StyleDimension.FromPixels(xOff) });
					xOff += 120;
				}
			}
		}
	}

	private void ClickNormalize(UIMouseEvent evt, UIElement listeningElement)
	{
		double total = _gearRate.Value + _currencyRate.Value + _mapRate.Value;
		_gearRate.SetPercent(_gearRate.Value / total);
		_currencyRate.SetPercent(_currencyRate.Value / total);
		_mapRate.SetPercent(_mapRate.Value / total);
	}
}
#endif

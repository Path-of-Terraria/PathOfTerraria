#if DEBUG
using System.Collections.Generic;
using System.Linq;
using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Common.UI.Elements;
using PathOfTerraria.Common.UI.Utilities;
using PathOfTerraria.Content.Items.Consumables.Maps;
using PathOfTerraria.Content.Items.Consumables.Maps.ExplorableMaps;
using PathOfTerraria.Content.Tiles.Furniture;
using PathOfTerraria.Core.Items;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.UI;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;

namespace PathOfTerraria.Common.Mapping;

/// <summary>
/// Debug-only map builder opened from the map device. It creates real map items so tier, rarity,
/// affixes, persistence, and multiplayer synchronization all exercise the production item paths.
/// </summary>
internal sealed class MapDeviceDebugMapMenu : UIPanel
{
	private enum MapCategory : byte
	{
		BossDomains,
		ExplorableMaps,
	}

	private const string LocalizationPath = "Mods.PathOfTerraria.UI.MapDevice.DebugMapBuilder.";
	private readonly MapDeviceEntity entity;
	private readonly Action close;

	private MapCategory activeCategory = MapCategory.BossDomains;
	private int selectedType;
	private int tier = 1;
	private ItemRarity rarity = ItemRarity.Magic;
	private Item preview = new();

	internal MapDeviceDebugMapMenu(MapDeviceEntity entity, Action close)
	{
		this.entity = entity;
		this.close = close;

		Width = StyleDimension.FromPixels(700f);
		Height = StyleDimension.FromPixels(520f);
		HAlign = 0.5f;
		VAlign = 0.5f;
		SetPadding(12f);
		BackgroundColor = new Color(20, 28, 46, 245);
		BorderColor = new Color(86, 126, 184);

		SelectFirstMap();
		Rebuild();
	}

	private void Rebuild()
	{
		RemoveAllChildren();
		BuildHeader();
		BuildCatalog();
		BuildDetails();
		Recalculate();
	}

	private void BuildHeader()
	{
		Append(new UIText(Language.GetText(LocalizationPath + "Title"), 0.55f, true)
		{
			Left = StyleDimension.FromPixels(4f),
			Top = StyleDimension.FromPixels(2f),
		});

		var bossTab = new UIPanelTab(nameof(MapCategory.BossDomains), Language.GetText(LocalizationPath + "BossDomains"), 0.82f)
		{
			Left = StyleDimension.FromPixels(0f),
			Top = StyleDimension.FromPixels(38f),
			TextColor = activeCategory == MapCategory.BossDomains ? Color.Yellow : Color.White,
		};
		bossTab.SetPadding(6f);
		bossTab.OnLeftClick += (_, _) => SetCategory(MapCategory.BossDomains);
		Append(bossTab);

		bossTab.Recalculate();
		var explorationTab = new UIPanelTab(nameof(MapCategory.ExplorableMaps), Language.GetText(LocalizationPath + "ExplorableMaps"), 0.82f)
		{
			Left = StyleDimension.FromPixels(bossTab.GetDimensions().Width + 8f),
			Top = StyleDimension.FromPixels(38f),
			TextColor = activeCategory == MapCategory.ExplorableMaps ? Color.Yellow : Color.White,
		};
		explorationTab.SetPadding(6f);
		explorationTab.OnLeftClick += (_, _) => SetCategory(MapCategory.ExplorableMaps);
		Append(explorationTab);

		var closeButton = new UIButton<string>("X")
		{
			Width = StyleDimension.FromPixels(34f),
			Height = StyleDimension.FromPixels(28f),
			HAlign = 1f,
		};
		closeButton.OnLeftClick += (_, _) =>
		{
			SoundEngine.PlaySound(SoundID.MenuClose);
			close();
		};
		Append(closeButton);
	}

	private void BuildCatalog()
	{
		var catalogPanel = new UIPanel
		{
			Left = StyleDimension.FromPixels(0f),
			Top = StyleDimension.FromPixels(76f),
			Width = StyleDimension.FromPixels(316f),
			Height = StyleDimension.FromPixels(408f),
			BackgroundColor = new Color(29, 39, 61, 230),
		};
		catalogPanel.SetPadding(7f);
		Append(catalogPanel);

		var list = new UIList
		{
			Width = new StyleDimension(-24f, 1f),
			Height = StyleDimension.Fill,
			ListPadding = 4f,
		};
		var scrollbar = new UIScrollbar
		{
			Width = StyleDimension.FromPixels(20f),
			Height = StyleDimension.Fill,
			HAlign = 1f,
		};
		list.SetScrollbar(scrollbar);
		catalogPanel.Append(list);
		catalogPanel.Append(scrollbar);

		foreach (int type in GetMapTypes(activeCategory))
		{
			list.Add(new MapChoiceRow(type, () => selectedType == type, () => SelectMap(type)));
		}
	}

	private void BuildDetails()
	{
		var details = new UIPanel
		{
			Left = StyleDimension.FromPixels(328f),
			Top = StyleDimension.FromPixels(76f),
			Width = StyleDimension.FromPixels(344f),
			Height = StyleDimension.FromPixels(408f),
			BackgroundColor = new Color(29, 39, 61, 230),
		};
		details.SetPadding(10f);
		Append(details);

		if (preview.IsAir || preview.ModItem is not Map map)
		{
			details.Append(new UIText(Language.GetText(LocalizationPath + "NoMaps"), 0.9f)
			{
				HAlign = 0.5f,
				VAlign = 0.5f,
			});
			return;
		}

		details.Append(new UIItemIcon(preview, false)
		{
			Left = StyleDimension.FromPixels(4f),
			Top = StyleDimension.FromPixels(2f),
		});
		details.Append(new UIText(preview.Name, 0.9f)
		{
			Left = StyleDimension.FromPixels(46f),
			Top = StyleDimension.FromPixels(8f),
		});

		PoTInstanceItemData data = preview.GetInstanceData();
		string summary = activeCategory == MapCategory.ExplorableMaps
			? Language.GetTextValue(LocalizationPath + "ExplorationSummary", tier, map.WorldLevel, rarity)
			: Language.GetTextValue(LocalizationPath + "BossSummary", map.GetMapTier(map.WorldLevel), map.WorldLevel);
		details.Append(new UIText(summary, 0.75f)
		{
			Left = StyleDimension.FromPixels(4f),
			Top = StyleDimension.FromPixels(48f),
			TextColor = Color.LightGray,
		});

		if (activeCategory == MapCategory.ExplorableMaps)
		{
			BuildExplorationControls(details);
		}

		float affixTop = activeCategory == MapCategory.ExplorableMaps ? 188f : 84f;
		details.Append(new UIText(Language.GetText(LocalizationPath + "Modifiers"), 0.82f)
		{
			Left = StyleDimension.FromPixels(4f),
			Top = StyleDimension.FromPixels(affixTop),
			TextColor = Color.LightBlue,
		});

		var affixList = new UIList
		{
			Left = StyleDimension.FromPixels(0f),
			Top = StyleDimension.FromPixels(affixTop + 24f),
			Width = new StyleDimension(-22f, 1f),
			Height = new StyleDimension(-(affixTop + 76f), 1f),
			ListPadding = 3f,
		};
		var affixScrollbar = new UIScrollbar
		{
			Top = StyleDimension.FromPixels(affixTop + 24f),
			Width = StyleDimension.FromPixels(18f),
			Height = new StyleDimension(-(affixTop + 76f), 1f),
			HAlign = 1f,
		};
		affixList.SetScrollbar(affixScrollbar);
		details.Append(affixList);
		details.Append(affixScrollbar);

		if (data.Affixes.Count == 0)
		{
			affixList.Add(new UIText(Language.GetText(LocalizationPath + "NoModifiers"), 0.75f)
			{
				TextColor = Color.Gray,
			});
		}
		else
		{
			foreach (ItemAffix affix in data.Affixes)
			{
				affixList.Add(new UIText($"{affix.Name}  T{affix.Tier + 1}  ({affix.Value:#0.##})", 0.72f)
				{
					TextColor = Color.LightGray,
				});
			}
		}

		var confirm = new UIButton<string>(Language.GetTextValue(LocalizationPath + "Confirm"))
		{
			Width = StyleDimension.FromPixels(132f),
			Height = StyleDimension.FromPixels(34f),
			HAlign = 1f,
			VAlign = 1f,
		};
		confirm.OnUpdate += element =>
		{
			var button = (UIButton<string>)element;
			bool enabled = CanConfirm();
			button.BackgroundColor = enabled ? new Color(48, 112, 74) : new Color(55, 55, 65);
			button.TextColor = enabled ? Color.White : Color.Gray;
		};
		confirm.OnLeftClick += (_, _) => Confirm();
		details.Append(confirm);
	}

	private void BuildExplorationControls(UIElement details)
	{
		var tierDown = new UIButton<string>("-")
		{
			Left = StyleDimension.FromPixels(4f),
			Top = StyleDimension.FromPixels(78f),
			Width = StyleDimension.FromPixels(34f),
			Height = StyleDimension.FromPixels(28f),
		};
		tierDown.OnLeftClick += (_, _) => ChangeTier(-1);
		details.Append(tierDown);

		details.Append(new UIText(Language.GetTextValue(LocalizationPath + "Tier", tier), 0.82f)
		{
			Left = StyleDimension.FromPixels(48f),
			Top = StyleDimension.FromPixels(83f),
		});

		var tierUp = new UIButton<string>("+")
		{
			Left = StyleDimension.FromPixels(132f),
			Top = StyleDimension.FromPixels(78f),
			Width = StyleDimension.FromPixels(34f),
			Height = StyleDimension.FromPixels(28f),
		};
		tierUp.OnLeftClick += (_, _) => ChangeTier(1);
		details.Append(tierUp);

		AddRarityButton(details, ItemRarity.Magic, 4f);
		AddRarityButton(details, ItemRarity.Rare, 112f);

		var reroll = new UIButton<string>(Language.GetTextValue(LocalizationPath + "Reroll"))
		{
			Left = StyleDimension.FromPixels(220f),
			Top = StyleDimension.FromPixels(118f),
			Width = StyleDimension.FromPixels(96f),
			Height = StyleDimension.FromPixels(30f),
		};
		reroll.OnLeftClick += (_, _) =>
		{
			RollPreview();
			Rebuild();
			SoundEngine.PlaySound(SoundID.Item37);
		};
		details.Append(reroll);
	}

	private void AddRarityButton(UIElement parent, ItemRarity buttonRarity, float left)
	{
		var button = new UIButton<string>(Language.GetTextValue(LocalizationPath + buttonRarity))
		{
			Left = StyleDimension.FromPixels(left),
			Top = StyleDimension.FromPixels(118f),
			Width = StyleDimension.FromPixels(100f),
			Height = StyleDimension.FromPixels(30f),
		};
		button.OnUpdate += element =>
		{
			var rarityButton = (UIButton<string>)element;
			bool selected = rarity == buttonRarity;
			rarityButton.BackgroundColor = selected ? new Color(73, 94, 171) : new Color(50, 61, 95);
			rarityButton.BorderColor = selected ? Color.Gold : Color.Black;
		};
		button.OnLeftClick += (_, _) => SetRarity(buttonRarity);
		parent.Append(button);
	}

	private void SetCategory(MapCategory category)
	{
		if (activeCategory == category)
		{
			return;
		}

		activeCategory = category;
		SelectFirstMap();
		Rebuild();
		SoundEngine.PlaySound(SoundID.MenuTick);
	}

	private void SelectFirstMap()
	{
		selectedType = GetMapTypes(activeCategory).FirstOrDefault();
		RollPreview();
	}

	private void SelectMap(int type)
	{
		if (selectedType == type)
		{
			return;
		}

		selectedType = type;
		RollPreview();
		Rebuild();
		SoundEngine.PlaySound(SoundID.MenuTick);
	}

	private void ChangeTier(int amount)
	{
		int newTier = Math.Clamp(tier + amount, 1, Map.MaxMapTier);
		if (newTier == tier)
		{
			return;
		}

		tier = newTier;
		RollPreview();
		Rebuild();
		SoundEngine.PlaySound(SoundID.MenuTick);
	}

	private void SetRarity(ItemRarity newRarity)
	{
		if (rarity == newRarity)
		{
			return;
		}

		rarity = newRarity;
		RollPreview();
		Rebuild();
		SoundEngine.PlaySound(SoundID.MenuTick);
	}

	private void RollPreview()
	{
		preview = new Item();
		if (selectedType <= ItemID.None)
		{
			return;
		}

		preview.SetDefaults(selectedType);
		if (preview.ModItem is not Map map)
		{
			preview.TurnToAir();
			return;
		}

		PoTInstanceItemData data = preview.GetInstanceData();
		if (map is ExplorableMap)
		{
			map.Tier = tier;
			data.RealLevel = Map.WorldLevelBasedOnTier(tier);
			data.Rarity = rarity;
			data.Affixes.Clear();
			data.ImplicitCount = 0;

			int affixCount = PoTItemHelper.GetAffixCount(rarity);
			for (int i = 0; i < affixCount; i++)
			{
				PoTItemHelper.AddNewAffix(preview, data);
			}

			PostRoll.Invoke(preview);
			data.NameAffix = GenerateNameAffixes.Invoke(preview);
		}
		else
		{
			data.Rarity = ItemRarity.Normal;
			PoTItemHelper.Roll(preview, map.WorldLevel);
		}
	}

	private bool CanConfirm()
	{
		return !preview.IsAir && preview.ModItem is Map && MapDeviceInterface.Entity == entity && !entity.PortalActive;
	}

	private void Confirm()
	{
		if (!CanConfirm())
		{
			SoundEngine.PlaySound(SoundID.MenuClose with { Pitch = -0.35f });
			return;
		}

		entity.StoredMap = preview.Clone();
		if (Main.netMode == NetmodeID.MultiplayerClient)
		{
			MapDeviceSync.Send(entity.ID, MapDeviceSync.Flags.Map, null);
		}

		SoundEngine.PlaySound(SoundID.Grab);
		close();
	}

	private static int[] GetMapTypes(MapCategory category)
	{
		return [.. ModContent.GetContent<Map>()
			.Where(map => category == MapCategory.ExplorableMaps ? map is ExplorableMap : map is not ExplorableMap)
			.OrderBy(map => map.Item.Name)
			.Select(map => map.Type)];
	}

	private sealed class MapChoiceRow : UIPanel
	{
		internal MapChoiceRow(int itemType, Func<bool> selected, Action choose)
		{
			Item item = new(itemType);
			Width = StyleDimension.Fill;
			Height = StyleDimension.FromPixels(48f);
			SetPadding(4f);

			Append(new UIItemIcon(item, false)
			{
				Left = StyleDimension.FromPixels(2f),
				VAlign = 0.5f,
			});
			Append(new UIText(item.Name, 0.78f)
			{
				Left = StyleDimension.FromPixels(44f),
				VAlign = 0.5f,
			});

			OnUpdate += _ =>
			{
				BackgroundColor = selected() ? new Color(58, 85, 137) : new Color(36, 48, 73);
				BorderColor = selected() ? Color.Gold : new Color(58, 73, 103);
			};
			OnLeftClick += (_, _) => choose();
		}
	}
}
#endif

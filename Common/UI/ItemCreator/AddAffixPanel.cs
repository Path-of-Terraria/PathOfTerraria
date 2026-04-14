using System.Collections.Generic;
using System.Linq;
using PathOfTerraria.Common.Data;
using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Core.Items;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace PathOfTerraria.Common.UI.ItemCreator;

#if DEBUG || STAGING
internal class AddAffixPanel : UIElement
{
	private readonly Item _item;
	private readonly Action _onAffixAdded;
	private UIList _affixOptions;

	public AddAffixPanel(Item item, Action onAffixAdded)
	{
		_item = item;
		_onAffixAdded = onAffixAdded;

		Width = StyleDimension.Fill;
		Height = StyleDimension.Fill;
	}

	public override void OnInitialize()
	{
		base.OnInitialize();

		var panel = new UIPanel
		{
			Width = StyleDimension.Fill,
			Height = StyleDimension.Fill,
			BackgroundColor = new Color(30, 30, 50) * 0.95f,
		};
		Append(panel);

		var title = new UIText("Select Affix to Add:", 0.9f)
		{
			Top = StyleDimension.FromPixels(4),
			HAlign = 0.5f,
			TextColor = Color.Gold,
		};
		panel.Append(title);

		var closeBtn = new UIButton<string>("Close")
		{
			Width = StyleDimension.FromPixels(60),
			Height = StyleDimension.FromPixels(24),
			HAlign = 1f,
			Top = StyleDimension.FromPixels(2),
		};
		closeBtn.OnLeftClick += (_, _) => Remove();
		panel.Append(closeBtn);

		_affixOptions = new UIList
		{
			Width = StyleDimension.Fill,
			Height = StyleDimension.FromPixelsAndPercent(-32, 1),
			Top = StyleDimension.FromPixels(32),
		};
		panel.Append(_affixOptions);

		UIScrollbar scrollbar = new()
		{
			Width = StyleDimension.FromPixels(20),
			Height = StyleDimension.FromPixelsAndPercent(-32, 1),
			Top = StyleDimension.FromPixels(32),
			HAlign = 1f,
		};
		_affixOptions.SetScrollbar(scrollbar);
		panel.Append(scrollbar);

		PopulateList();
	}

	private void PopulateList()
	{
		PoTInstanceItemData data = _item.GetInstanceData();
		List<ItemAffix> available = AffixHandler.GetAffixes(_item);
		HashSet<Type> existingTypes = data.Affixes.Select(a => a.GetType()).ToHashSet();

		foreach (ItemAffix proto in available)
		{
			Type affixType = proto.GetType();

			if (existingTypes.Contains(affixType))
			{
				continue;
			}

			string name = affixType.Name;
			var entry = new UIText(name, 0.85f)
			{
				Width = StyleDimension.Fill,
				Height = StyleDimension.FromPixels(24),
				TextColor = Color.LightCyan,
			};

			Type capturedType = affixType;
			entry.OnLeftClick += (_, _) => AddAffix(capturedType);
			entry.OnMouseOver += (_, el) => ((UIText)el).TextColor = Color.Yellow;
			entry.OnMouseOut += (_, el) => ((UIText)el).TextColor = Color.LightCyan;
			_affixOptions.Add(entry);
		}

		if (_affixOptions.Count == 0)
		{
			_affixOptions.Add(new UIText("No available affixes for this item.", 0.85f)
			{
				TextColor = Color.Gray,
			});
		}
	}

	private void AddAffix(Type affixType)
	{
		PoTInstanceItemData data = _item.GetInstanceData();
		var affix = (ItemAffix)Activator.CreateInstance(affixType);

		int level = data.RealLevel;

		if (level == 0)
		{
			level = 1;
		}

		affix.Value = AffixRegistry.GetRandomAffixValue(affix, _item, level);

		// If value came back 0, the affix may not have tier data for this item level.
		// Set reasonable defaults from tier 0 if available.
		if (affix.Value == 0)
		{
			var affixData = AffixRegistry.TryGetItemData(affixType, data.ItemType);

			if (affixData?.Tiers is { Count: > 0 })
			{
				var tier = affixData.Tiers[0];
				affix.Tier = 0;
				affix.MinValue = tier.MinValue;
				affix.MaxValue = tier.MaxValue;
				affix.Value = tier.MinValue;
			}
		}

		data.Affixes.Add(affix);
		_onAffixAdded?.Invoke();
		Remove();
	}
}
#endif

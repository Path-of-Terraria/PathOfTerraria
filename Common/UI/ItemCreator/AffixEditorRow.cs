#if DEBUG || STAGING
using System.Collections.Generic;
using PathOfTerraria.Common.Data;
using PathOfTerraria.Common.Data.Models;
using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Core.Items;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader.UI;
using Terraria.UI;

#nullable enable

namespace PathOfTerraria.Common.UI.ItemCreator;

internal class AffixEditorRow : UIElement
{
	private readonly ItemAffix _affix;
	private readonly ItemAffixData? _affixData;
	private readonly Item _item;
	private readonly int _affixIndex;
	private readonly Action _onModified;

	private UIText _nameText = null!;
	private UIText _tierText = null!;
	private UIText _valueText = null!;
	private UIText _rangeText = null!;

	private int _holdTime;
	private bool _resetHold;

	public AffixEditorRow(ItemAffix affix, Item item, int affixIndex, Action onModified)
	{
		_affix = affix;
		_item = item;
		_affixIndex = affixIndex;
		_onModified = onModified;

		ItemType itemType = item.GetInstanceData().ItemType;
		_affixData = AffixRegistry.TryGetItemData(affix.GetType(), itemType);

		Width = StyleDimension.Fill;
		Height = StyleDimension.FromPixels(36);
	}

	public override void OnInitialize()
	{
		base.OnInitialize();

		// Affix name
		string name = _affix.GetType().Name;
		Color nameColor = _affix.IsImplicit ? Color.Gray : (_affix.IsCorruptedAffix ? new Color(180, 80, 220) : Color.White);

		string prefix = _affix.IsImplicit ? "[Impl] " : "";

		_nameText = new UIText($"{prefix}{name}", 0.85f)
		{
			TextColor = nameColor,
			VAlign = 0.5f,
		};
		Append(_nameText);

		if (_affixData != null)
		{
			BuildTierControls();
			BuildValueControls();
		}
		else
		{
			// Display-only: show current value
			_valueText = new UIText($"Value: {FormatValue(_affix.Value)}", 0.8f)
			{
				Left = StyleDimension.FromPixels(280),
				VAlign = 0.5f,
				TextColor = Color.LightGray,
			};
			Append(_valueText);
		}

		BuildRemoveButton();
	}

	private void BuildTierControls()
	{
		Debug.Assert(_affixData != null);
		int maxTier = _affixData.Tiers.Count - 1;

		var tierDown = new UIButton<string>("<")
		{
			Width = StyleDimension.FromPixels(26),
			Height = StyleDimension.FromPixels(26),
			Left = StyleDimension.FromPixels(260),
			VAlign = 0.5f,
		};
		tierDown.OnLeftClick += (_, _) => ChangeTier(-1);
		Append(tierDown);

		_tierText = new UIText($"T{_affix.Tier + 1}", 0.85f)
		{
			Left = StyleDimension.FromPixels(290),
			VAlign = 0.5f,
			TextColor = Color.Gold,
		};
		Append(_tierText);

		var tierUp = new UIButton<string>(">")
		{
			Width = StyleDimension.FromPixels(26),
			Height = StyleDimension.FromPixels(26),
			Left = StyleDimension.FromPixels(320),
			VAlign = 0.5f,
		};
		tierUp.OnLeftClick += (_, _) => ChangeTier(1);
		Append(tierUp);
	}

	private void BuildValueControls()
	{
		var valueSub = new UIButton<string>("-")
		{
			Width = StyleDimension.FromPixels(26),
			Height = StyleDimension.FromPixels(26),
			Left = StyleDimension.FromPixels(360),
			VAlign = 0.5f,
		};
		valueSub.OnUpdate += ele => CheckHovering(ele, false);
		Append(valueSub);

		_valueText = new UIText(FormatValue(_affix.Value), 0.85f)
		{
			Left = StyleDimension.FromPixels(390),
			VAlign = 0.5f,
		};
		Append(_valueText);

		var valueAdd = new UIButton<string>("+")
		{
			Width = StyleDimension.FromPixels(26),
			Height = StyleDimension.FromPixels(26),
			Left = StyleDimension.FromPixels(450),
			VAlign = 0.5f,
		};
		valueAdd.OnUpdate += ele => CheckHovering(ele, true);
		Append(valueAdd);

		_rangeText = new UIText(GetRangeString(), 0.7f)
		{
			Left = StyleDimension.FromPixels(480),
			VAlign = 0.5f,
			TextColor = Color.LightGray,
		};
		Append(_rangeText);
	}

	private void BuildRemoveButton()
	{
		var remove = new UIButton<string>("X")
		{
			Width = StyleDimension.FromPixels(26),
			Height = StyleDimension.FromPixels(26),
			HAlign = 1f,
			VAlign = 0.5f,
		};
		remove.OnLeftClick += (_, _) => RemoveAffix();
		Append(remove);
	}

	public override void Update(GameTime gameTime)
	{
		_resetHold = true;
		base.Update(gameTime);

		if (_resetHold)
		{
			_holdTime = 0;
		}
	}

	private void ChangeTier(int direction)
	{
		if (_affixData == null)
		{
			return;
		}

		int maxTier = _affixData.Tiers.Count - 1;
		int newTier = Math.Clamp(_affix.Tier + direction, 0, maxTier);

		if (newTier == _affix.Tier)
		{
			return;
		}

		_affix.Tier = newTier;
		ItemAffixData.TierData tierData = _affixData.Tiers[newTier];
		_affix.MinValue = tierData.MinValue;
		_affix.MaxValue = tierData.MaxValue;
		_affix.Value = MathHelper.Clamp(_affix.Value, tierData.MinValue, tierData.MaxValue);

		if (_affix.Round)
		{
			_affix.Value = (float)Math.Round(_affix.Value);
		}

		UpdateDisplayText();
	}

	private void CheckHovering(UIElement element, bool add)
	{
		if (!element.GetOuterDimensions().ToRectangle().Contains(Main.MouseScreen.ToPoint()))
		{
			return;
		}

		if (!Main.mouseLeft)
		{
			return;
		}

		_holdTime++;
		_resetHold = false;

		if (_holdTime != 1 && (_holdTime <= 20 || _holdTime % 2 != 0))
		{
			return;
		}

		float range = _affix.MaxValue - _affix.MinValue;
		float increment = Math.Max(range / 40f, 0.01f);

		if (add)
		{
			_affix.Value += increment;
		}
		else
		{
			_affix.Value -= increment;
		}

		_affix.Value = MathHelper.Clamp(_affix.Value, _affix.MinValue, _affix.MaxValue);

		if (_affix.Round)
		{
			_affix.Value = (float)Math.Round(_affix.Value);
		}

		UpdateDisplayText();
	}

	private void RemoveAffix()
	{
		PoTInstanceItemData data = _item.GetInstanceData();

		if (_affixIndex < data.Affixes.Count)
		{
			bool wasImplicit = data.Affixes[_affixIndex].IsImplicit;
			data.Affixes.RemoveAt(_affixIndex);

			if (wasImplicit && data.ImplicitCount > 0)
			{
				data.ImplicitCount--;
			}
		}

		_onModified?.Invoke();
	}

	private void UpdateDisplayText()
	{
		_tierText?.SetText($"T{_affix.Tier + 1}");
		_valueText?.SetText(FormatValue(_affix.Value));
		_rangeText?.SetText(GetRangeString());
	}

	private string GetRangeString()
	{
		return $"[{FormatValue(_affix.MinValue)}-{FormatValue(_affix.MaxValue)}]";
	}

	private string FormatValue(float value)
	{
		return _affix.Round ? $"{(int)Math.Round(value)}" : $"{value:F1}";
	}
}
#endif

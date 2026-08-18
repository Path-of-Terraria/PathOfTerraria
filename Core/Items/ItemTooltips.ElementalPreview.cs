using System.Collections.Generic;
using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Common.Systems.Affixes.ItemTypes;
using PathOfTerraria.Common.Systems.ElementalDamage;
using PathOfTerraria.Common.Systems.ModPlayers;

namespace PathOfTerraria.Core.Items;

public sealed partial class ItemTooltips
{
	private static ElementalContainer BuildElementalPreview(Player player, Item previewItem)
	{
		ElementalContainer preview = player.GetModPlayer<ElementalPlayer>().Container.Clone();
		Item activeItem = UniversalBuffingPlayer.GetActiveMainItem(player);

		ApplyElementalAffixDelta(preview, activeItem, -1);
		ApplyElementalAffixDelta(preview, previewItem, 1);

		return preview;
	}

	private static void ApplyElementalAffixDelta(ElementalContainer container, Item item, int direction)
	{
		if (item.IsAir || !item.TryGetGlobalItem(out PoTInstanceItemData data))
		{
			return;
		}

		var contributions = new Dictionary<ElementType, (int Bonus, float Conversion)>();

		foreach (ItemAffix affix in data.Affixes)
		{
			if (affix is not IElementalDamageAffix elementalAffix)
			{
				continue;
			}

			contributions.TryGetValue(elementalAffix.ElementType, out (int Bonus, float Conversion) contribution);
			contribution.Bonus += elementalAffix.DamageBonus;
			contribution.Conversion += elementalAffix.DamageConversion;
			contributions[elementalAffix.ElementType] = contribution;
		}

		foreach ((ElementType type, (int bonus, float conversion)) in contributions)
		{
			ElementInstance element = container[type];
			int previewBonus = Math.Max(0, element.DamageModifier.DamageBonus + bonus * direction);
			float previewConversion = MathF.Max(0f, element.DamageModifier.DamageConversion + conversion * direction);

			element.DamageModifier = element.DamageModifier.ApplyOverride(previewBonus, previewConversion);
		}
	}
}

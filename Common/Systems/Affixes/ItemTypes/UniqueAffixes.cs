using Terraria.Localization;

namespace PathOfTerraria.Common.Systems.Affixes.ItemTypes;

internal class NoFallDamageAffix : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		if (player != null)
		{
			player.noFallDmg = true;
		}
	}

	public override void ApplyTooltip(Player player, Item item, AffixTooltipsHandler handler)
	{
		handler.AddOrModify(GetType(), item, 1, this.GetLocalization("Description"), IsCorruptedAffix, ModifyTooltip);
	}

	private string ModifyTooltip(AffixTooltip self, float value, float difference, float originalValue, LocalizedText text)
	{
		bool hasBuff = value > 0;
		string baseString = this.GetLocalizedValue(hasBuff ? "Description" : "Removed");

		if (!hasBuff && originalValue != value && value == 0)
		{
			self.Color = Color.Red;
		}

		return baseString;
	}
}

using PathOfTerraria.Content.Items.Gear;

namespace PathOfTerraria.Core.Systems.Affixes.Affixes.GearTypes.ArmorAffixes;

internal class ManaAffix : GearAffix
{
	public override GearType PossibleTypes => GearType.Armor;
	public override ModifierType ModifierType => ModifierType.Added;
	public override string Tooltip => "# Maximum Mana";
	public override bool Round => true;

	protected override float internalModifierCalculation(Gear gear)
	{
		return 5 + Value * 20 + gear.ItemLevel / 20;
	}

	public override void BuffPassive(Player player, Gear gear)
	{
		player.statManaMax2 += GetModifierIValue(gear);
	}
}

internal class ManaRegenAffix : GearAffix
{
	public override GearType PossibleTypes => GearType.Armor;
	public override ModifierType ModifierType => ModifierType.Added;
	public override string Tooltip => "# Mana Regeneration";
	public override bool Round => true;

	protected override float internalModifierCalculation(Gear gear)
	{
		return 1f + Value * 4f + gear.ItemLevel / 40f;
	}

	public override void BuffPassive(Player player, Gear gear)
	{
		player.manaRegen += GetModifierIValue(gear);
	}
}

internal class ManaPotionPowerAffix : GearAffix
{
	public override GearType PossibleTypes => GearType.Armor;
	public override ModifierType ModifierType => ModifierType.Added;
	public override string Tooltip => "# Potion Mana Restored";
	public override bool Round => true;

	protected override float internalModifierCalculation(Gear gear)
	{
		return 10f + Value * 10f + gear.ItemLevel / 20f;
	}

	public override void BuffPassive(Player player, Gear gear)
	{
		player.GetModPlayer<PotionSystem>().ManaPower += GetModifierIValue(gear);
	}
}

internal class ManaPotionCapAffix : GearAffix
{
	public override GearType PossibleTypes => GearType.Armor;
	public override ModifierType ModifierType => ModifierType.Added;
	public override bool Round => true;
	public override string Tooltip => "# Max Mana Potions";

	protected override float internalModifierCalculation(Gear gear)
	{
		return 1 + Value + gear.ItemLevel / 100f;
	}

	public override void BuffPassive(Player player, Gear gear)
	{
		player.GetModPlayer<PotionSystem>().MaxMana += GetModifierIValue(gear);
	}
}

internal class ManaPotionCooldownAffix : GearAffix
{
	public override GearType PossibleTypes => GearType.Armor;
	public override ModifierType ModifierType => ModifierType.Added;
	public override GearInfluence RequiredInfluence => GearInfluence.Solar;
	public override string Tooltip => "Mana potions are ready # seconds sooner";

	protected override float internalModifierCalculation(Gear gear)
	{
		return 0.5f + Value * 0.5f;
	}

	public override void BuffPassive(Player player, Gear gear)
	{
		player.GetModPlayer<PotionSystem>().ManaDelay -= (int)(60 * GetModifierValue(gear));
	}
}
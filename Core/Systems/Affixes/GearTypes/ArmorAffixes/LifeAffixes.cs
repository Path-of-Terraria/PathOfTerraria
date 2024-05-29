using PathOfTerraria.Content.Items.Gear;
using PathOfTerraria.Core.Systems;

namespace PathOfTerraria.Core.Systems.Affixes.Affixes.GearTypes.ArmorAffixes;

internal class LifeAffix : GearAffix
{
	public override GearType PossibleTypes => GearType.Armor;
	public override ModifierType ModifierType => ModifierType.Added;
	public override string Tooltip => "# Maximum Life";
	public override bool Round => true;
	protected override float internalModifierCalculation(Gear gear)
	{
		return 10f + Value * 30f + gear.ItemLevel / 100f;
	}

	public override void BuffPassive(Player player, Gear gear)
	{
		player.statLifeMax2 += GetModifierIValue(gear);
	}
}

internal class LifeRegenAffix : GearAffix
{
	public override GearType PossibleTypes => GearType.Armor;
	public override ModifierType ModifierType => ModifierType.Added;
	public override string Tooltip => "# Life Regeneration";
	public override bool Round => true;
	protected override float internalModifierCalculation(Gear gear)
	{
		return 1f + Value * 4f + gear.ItemLevel / 40f;
	}

	public override void BuffPassive(Player player, Gear gear)
	{
		player.lifeRegen += GetModifierIValue(gear);
	}
}

internal class LifePotionPowerAffix : GearAffix
{
	public override GearType PossibleTypes => GearType.Armor;
	public override ModifierType ModifierType => ModifierType.Added;
	public override string Tooltip => "# Life Regeneration";
	public override bool Round => true;
	protected override float internalModifierCalculation(Gear gear)
	{
		return 10f + Value * 10f + gear.ItemLevel / 20f;
	}
	public override void BuffPassive(Player player, Gear gear)
	{
		player.GetModPlayer<PotionSystem>().HealPower += GetModifierIValue(gear);
	}
}

internal class LifePotionCapAffix : GearAffix
{
	public override GearType PossibleTypes => GearType.Armor;
	public override ModifierType ModifierType => ModifierType.Added;
	public override string Tooltip => "# Life Regeneration";
	public override bool Round => true;

	protected override float internalModifierCalculation(Gear gear)
	{
		return 1 + Value + gear.ItemLevel / 100f;
	}
	public override void BuffPassive(Player player, Gear gear)
	{
		player.GetModPlayer<PotionSystem>().MaxHealing += GetModifierIValue(gear);
	}
}

internal class LifePotionCooldownAffix : GearAffix
{
	public override GearType PossibleTypes => GearType.Armor;
	public override ModifierType ModifierType => ModifierType.Added;
	public override GearInfluence RequiredInfluence => GearInfluence.Lunar;
	public override string Tooltip => "# Healing potions are ready # seconds sooner";

	protected override float internalModifierCalculation(Gear gear)
	{
		return 0.5f + Value * 0.5f;
	}

	public override void BuffPassive(Player player, Gear gear)
	{
		player.GetModPlayer<PotionSystem>().HealDelay -= (int)(60 * GetModifierValue(gear));
	}
}
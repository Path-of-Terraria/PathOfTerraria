using PathOfTerraria.Common.Systems.ElementalDamage;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives;

internal class IncreasedFireDamagePassive : Passive
{
	private const float MultPerLevel = 0.10f; // +10% per level

	public override void BuffPlayer(Player player)
	{
		ElementalPlayer elemental = player.GetModPlayer<ElementalPlayer>();
		elemental.Container.FireMultiplier += MultPerLevel * Level;
	}
}

internal class IncreasedColdDamagePassive : Passive
{
	private const float MultPerLevel = 0.10f;

	public override void BuffPlayer(Player player)
	{
		ElementalPlayer elemental = player.GetModPlayer<ElementalPlayer>();
		elemental.Container.ColdMultiplier += MultPerLevel * Level;
	}
}

internal class IncreasedLightningDamagePassive : Passive
{
	private const float MultPerLevel = 0.10f;

	public override void BuffPlayer(Player player)
	{
		ElementalPlayer elemental = player.GetModPlayer<ElementalPlayer>();
		elemental.Container.LightningMultiplier += MultPerLevel * Level;
	}
}
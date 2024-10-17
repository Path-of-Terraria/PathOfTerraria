using PathOfTerraria.Common.Events;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Common.Systems.TreeSystem;

namespace PathOfTerraria.Content.Passives;

/// <summary>
/// Increases the chance of landing a critical strike by a flat amount.
/// </summary>
internal class AddedCriticalStrikeChance : Passive
{
	private const float AmountPerLevel = 1.02f;

	public override void BuffPlayer(Player player)
	{
		player.GetCritChance(DamageClass.Generic) = player.GetCritChance(DamageClass.Generic) + AmountPerLevel * Level;
	}
}

/// <summary>
/// Increases the chance of landing a critical strike by a percentage of the base critical strike chance.
/// </summary>
internal class IncreasedCriticalStrikeChance : Passive
{
	private const float AmountPerLevel = 1.05f;

	public override void BuffPlayer(Player player)
	{
		player.GetCritChance(DamageClass.Generic) = player.GetCritChance(DamageClass.Generic) * (1 + AmountPerLevel * Level);
	}
}

/// <summary>
/// Critical strikes cause hits to deal "extra damage" compared to a non-critical strike, the magnitude of which is determined by critical strike multiplier.
/// Increased critical strike multiplier increases the amount of extra damage dealt by critical strikes.
/// </summary>
internal class IncreasedCriticalStrikeMultiplier : Passive
{
	private const float AmountPerLevel = 1.05f;

	public override void OnLoad()
	{
		PathOfTerrariaPlayerEvents.ModifyHitNPCEvent += BuffCritStrikeDamageMultiplier;
	}
	
	private void BuffCritStrikeDamageMultiplier(NPC target, ref NPC.HitModifiers modifiers)
	{
		int level = Main.LocalPlayer.GetModPlayer<PassiveTreePlayer>().GetCumulativeLevel(InternalIdentifier);

		if (level > 0)
		{
			modifiers.CritDamage *= level * AmountPerLevel;
		}
	}
}
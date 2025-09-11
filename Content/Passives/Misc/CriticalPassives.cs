using PathOfTerraria.Common.Events;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Common.Data;
using System.Linq;
using PathOfTerraria.Common.Systems.ModPlayers;

namespace PathOfTerraria.Content.Passives;

/// <summary>
/// Increases the chance of landing a critical strike by a flat amount.
/// </summary>
internal class AddedCriticalStrikeChance : Passive
{
	private const float AmountPerLevel = 1.02f;

	public override void BuffPlayer(Player player)
	{
		player.GetCritChance(DamageClass.Generic) += Value * Level;
	}
}

/// <summary>
/// Increases the chance of landing a critical strike by a percentage of the base critical strike chance.
/// </summary>
internal class IncreasedCriticalStrikeChance : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.GetCritChance(DamageClass.Generic) *= (1 + (Value / 100f) * Level);
	}
}

/// <summary>
/// Critical strikes cause hits to deal "extra damage" compared to a non-critical strike, the magnitude of which is determined by critical strike multiplier.
/// Increased critical strike multiplier increases the amount of extra damage dealt by critical strikes.
/// </summary>
internal class IncreasedCriticalStrikeMultiplier : Passive
{
	public override void BuffPlayer(Player player)
	{
		//This is more so just visual. The critical damage on hit is actually being applied in BuffCritStrikeDamageMultiplier. Maybe theres a better way to do this?
		player.GetModPlayer<UniversalBuffingPlayer>().UniversalModifier.CriticalMultiplier.Base += Value * Level;
	}
	public override void OnLoad()
	{
		PathOfTerrariaPlayerEvents.ModifyHitNPCEvent += BuffCritStrikeDamageMultiplier;
	}
	
	private void BuffCritStrikeDamageMultiplier(NPC target, ref NPC.HitModifiers modifiers)
	{
		Player player = Main.LocalPlayer;
		PassiveTreePlayer treePlayer = player.GetModPlayer<PassiveTreePlayer>();
		int level = treePlayer.GetCumulativeLevel(Name);

		if (level > 0)
		{
			float passiveValue = treePlayer.GetCumulativeValue<Passive>();
			modifiers.CritDamage *= 1f + (passiveValue / 100f) * level;
		}
	}
}

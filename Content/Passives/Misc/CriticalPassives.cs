using PathOfTerraria.Common.Events;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Common.Data;
using System.Linq;

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
			//This works but the other way is through a mod player. This is a bit of a hacky way to do it.
			var passiveData = PassiveRegistry.GetPassiveData().FirstOrDefault(p => p.InternalIdentifier == Name);
			
			if (passiveData != null)
			{
				modifiers.CritDamage *= 1f + (passiveData.Value / 100f) * level;
			}
		}
	}
}

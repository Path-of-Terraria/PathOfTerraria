using PathOfTerraria.Common.Systems.BlockSystem;
using PathOfTerraria.Common.Systems.ElementalDamage;
using PathOfTerraria.Common.Systems.EnergyShield;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.ModPlayers.SkillPlayers;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Common.Systems.VanillaModifications;

namespace PathOfTerraria.Content.Passives.Misc;

internal static class NotablePassiveUtils
{
	public static void AddMaxLifePercent(Player player, float percent)
	{
		player.statLifeMax2 = (int)(player.statLifeMax2 * (1f + percent));
	}

	public static void AddMaxManaPercent(Player player, float percent)
	{
		player.statManaMax2 = (int)(player.statManaMax2 * (1f + percent));
	}

	public static void AddAllResistances(Player player, float percent)
	{
		ElementalPlayer elemental = player.GetModPlayer<ElementalPlayer>();

		foreach (ElementInstance element in elemental.Container)
		{
			if (element.IsGeneric)
			{
				element.Resistance += percent;
			}
		}
	}

	public static void ReduceManaCost(Player player, float percent)
	{
		float factor = Math.Max(0f, 1f - percent);
		player.manaCost *= factor;
		player.GetModPlayer<SkillCombatPlayer>().GlobalBuff.CostModifier *= factor;
	}

	public static bool HasNearbyEnemies(Player player)
	{
		foreach (NPC npc in Main.ActiveNPCs)
		{
			if (npc.CanBeChasedBy(player) && npc.DistanceSQ(player.Center) <= PoTMod.NearbyDistanceSq)
			{
				return true;
			}
		}

		return false;
	}
}

internal class AllResistanceManaNotable : Passive
{
	public override void BuffPlayer(Player player)
	{
		NotablePassiveUtils.AddAllResistances(player, 0.10f);
		NotablePassiveUtils.AddMaxManaPercent(player, 0.05f);
	}
}

internal class LifeManaDefenseNotable : Passive
{
	public override void BuffPlayer(Player player)
	{
		NotablePassiveUtils.AddMaxLifePercent(player, 0.05f);
		NotablePassiveUtils.AddMaxManaPercent(player, 0.05f);
		player.statDefense += 3;
	}
}

internal class ManaRegenLifeDefenseNotable : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.GetModPlayer<ManaRegenRework.ManaRegenPlayer>().ManaRegen.Flat += ManaRegenRework.ManaPerSecondToManaRegen(2f);
		NotablePassiveUtils.AddMaxLifePercent(player, 0.05f);
		player.statDefense += 5;
	}
}

internal class DamageReductionReducedManaCostNotable : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.endurance += 0.03f;
		NotablePassiveUtils.ReduceManaCost(player, 0.05f);
	}
}

internal class MovementSpeedLifeNotable : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.moveSpeed += 0.05f;
		player.statLifeMax2 += 30;
	}
}

internal class LifeEnergyShieldNotable : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.statLifeMax2 += 25;
		player.GetModPlayer<EnergyShieldPlayer>().AddGlobalEnergyShield(25f);
	}
}

internal class AllResistanceProjectileSpeedMovementSpeedNotable : Passive
{
	public override void BuffPlayer(Player player)
	{
		NotablePassiveUtils.AddAllResistances(player, 0.05f);
		player.GetModPlayer<UniversalBuffingPlayer>().UniversalModifier.ProjectileSpeed += 0.05f;
		player.moveSpeed += 0.03f;
	}
}

internal class LifeDefenseNoNearbyEnemiesNotable : Passive
{
	public override void BuffPlayer(Player player)
	{
		NotablePassiveUtils.AddMaxLifePercent(player, 0.05f);

		if (!NotablePassiveUtils.HasNearbyEnemies(player))
		{
			player.statDefense += 5;
		}
	}
}

internal class LifeDefenseNotable : Passive
{
	public override void BuffPlayer(Player player)
	{
		NotablePassiveUtils.AddMaxLifePercent(player, 0.05f);
		player.statDefense += 5;
	}
}

internal class DefenseWhileMovingMovementSpeedNotable : Passive
{
	public override void BuffPlayer(Player player)
	{
		if (player.velocity != Vector2.Zero)
		{
			player.statDefense += 7;
		}

		player.moveSpeed += 0.05f;
	}
}

internal class LifeRegenDamageReductionNotable : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.lifeRegen += 2 * 2;
		player.endurance += 0.03f;
	}
}

internal class BlockChanceLifeNotable : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.GetModPlayer<BlockPlayer>().AddBlockChance(0.05f);
		player.statLifeMax2 += 20;
	}
}

internal class MinionMovementSpeedLifeNotable : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.GetModPlayer<UniversalBuffingPlayer>().UniversalModifier.ProjectileBehaviourSpeed += 0.05f;
		NotablePassiveUtils.AddMaxLifePercent(player, 0.05f);
	}
}

internal class MovementSpeedLifeBlockChanceNotable : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.moveSpeed += 0.05f;
		player.statLifeMax2 += 15;
		player.GetModPlayer<BlockPlayer>().AddBlockChance(0.05f);
	}
}

internal class LifeManaAllResistanceNotable : Passive
{
	public override void BuffPlayer(Player player)
	{
		NotablePassiveUtils.AddMaxLifePercent(player, 0.05f);
		NotablePassiveUtils.AddMaxManaPercent(player, 0.05f);
		NotablePassiveUtils.AddAllResistances(player, 0.03f);
	}
}

internal class FlatLifeManaDefenseNotable : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.statLifeMax2 += 30;
		player.statManaMax2 += 30;
		player.statDefense += 3;
	}
}

internal class EnergyShieldGlobalDamageNotable : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.GetModPlayer<EnergyShieldPlayer>().AddGlobalEnergyShield(30f);
		player.GetDamage(DamageClass.Generic) += 0.05f;
	}
}
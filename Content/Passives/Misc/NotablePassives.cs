using PathOfTerraria.Common.Systems.BlockSystem;
using PathOfTerraria.Common.Systems.ElementalDamage;
using PathOfTerraria.Common.Systems.EnergyShield;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.ModPlayers.SkillPlayers;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Common.Systems.VanillaModifications;

namespace PathOfTerraria.Content.Passives;

internal static class NotablePassiveUtils
{
	public static void AddMaxLifePercent(Player player, int level, float percent)
	{
		player.statLifeMax2 = (int)(player.statLifeMax2 * (1f + percent * level));
	}

	public static void AddMaxManaPercent(Player player, int level, float percent)
	{
		player.statManaMax2 = (int)(player.statManaMax2 * (1f + percent * level));
	}

	public static void AddAllResistances(Player player, int level, float percent)
	{
		ElementalPlayer elemental = player.GetModPlayer<ElementalPlayer>();
		float resistanceBonus = percent * level;

		foreach (ElementInstance element in elemental.Container)
		{
			if (element.IsGeneric)
			{
				element.Resistance += resistanceBonus;
			}
		}
	}

	public static void ReduceManaCost(Player player, int level, float percent)
	{
		float factor = Math.Max(0f, 1f - percent * level);
		player.manaCost *= factor;
		player.GetModPlayer<SkillCombatPlayer>().GlobalBuff.CostModifier *= factor;
	}

	public static bool CalculateNearbyEnemies(Player player)
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

internal class NotablePassivesPlayer : ModPlayer
{
	public float DodgeChance;
	public bool HasNearbyEnemies;
	private int _nearbyEnemyCheckTimer;

	public override void ResetEffects()
	{
		DodgeChance = 0f;
	}

	public override void PreUpdate()
	{
		if (_nearbyEnemyCheckTimer <= 0)
		{
			HasNearbyEnemies = NotablePassiveUtils.CalculateNearbyEnemies(Player);
			_nearbyEnemyCheckTimer = 15;
		}
		else
		{
			_nearbyEnemyCheckTimer--;
		}
	}

	public override bool FreeDodge(Player.HurtInfo info)
	{
		if (DodgeChance <= 0f || info.Damage <= 0)
		{
			return false;
		}

		bool dodged = Main.rand.NextFloat() < DodgeChance;

		if (dodged)
		{
			Player.AddImmuneTime(Terraria.ID.ImmunityCooldownID.General, Player.longInvince ? 80 : 40);
			Player.immune = true;
			Player.immuneNoBlink = false;
		}

		return dodged;
	}
}

internal class AllResistanceManaNotable : Passive
{
	public override void BuffPlayer(Player player)
	{
		NotablePassiveUtils.AddAllResistances(player, Level, 0.10f);
		NotablePassiveUtils.AddMaxManaPercent(player, Level, 0.05f);
	}
}

internal class LifeManaDefenseNotable : Passive
{
	public override void BuffPlayer(Player player)
	{
		NotablePassiveUtils.AddMaxLifePercent(player, Level, 0.05f);
		NotablePassiveUtils.AddMaxManaPercent(player, Level, 0.05f);
		player.statDefense += 3 * Level;
	}
}

internal class ManaRegenLifeDefenseNotable : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.GetModPlayer<ManaRegenRework.ManaRegenPlayer>().ManaRegen.Flat += ManaRegenRework.ManaPerSecondToManaRegen(2f * Level);
		NotablePassiveUtils.AddMaxLifePercent(player, Level, 0.05f);
		player.statDefense += 5 * Level;
	}
}

internal class DamageReductionReducedManaCostNotable : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.endurance += 0.03f * Level;
		NotablePassiveUtils.ReduceManaCost(player, Level, 0.05f);
	}
}

internal class MovementSpeedLifeNotable : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.moveSpeed += 0.05f * Level;
		player.statLifeMax2 += 30 * Level;
	}
}

internal class LifeDodgeNotable : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.statLifeMax2 += 15 * Level;
		player.GetModPlayer<NotablePassivesPlayer>().DodgeChance += 0.03f * Level;
	}
}

internal class AllResistanceProjectileSpeedMovementSpeedNotable : Passive
{
	public override void BuffPlayer(Player player)
	{
		NotablePassiveUtils.AddAllResistances(player, Level, 0.05f);
		player.GetModPlayer<UniversalBuffingPlayer>().UniversalModifier.ProjectileSpeed += 0.05f * Level;
		player.moveSpeed += 0.03f * Level;
	}
}

internal class LifeDefenseNoNearbyEnemiesNotable : Passive
{
	public override void BuffPlayer(Player player)
	{
		NotablePassiveUtils.AddMaxLifePercent(player, Level, 0.05f);

		if (!player.GetModPlayer<NotablePassivesPlayer>().HasNearbyEnemies)
		{
			player.statDefense += 5 * Level;
		}
	}
}

internal class LifeDefenseNotable : Passive
{
	public override void BuffPlayer(Player player)
	{
		NotablePassiveUtils.AddMaxLifePercent(player, Level, 0.05f);
		player.statDefense += 5 * Level;
	}
}

internal class DefenseWhileMovingMovementSpeedNotable : Passive
{
	public override void BuffPlayer(Player player)
	{
		if (player.velocity.LengthSquared() > 0.001f)
		{
			player.statDefense += 10 * Level;
		}

		player.moveSpeed += 0.05f * Level;
	}
}

internal class LessKnockbackDamageReductionNotable : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.knockBackResist *= 1f - 0.10f * Level;
		player.endurance += 0.03f * Level;
	}
}

internal class BlockChanceLifeNotable : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.GetModPlayer<BlockPlayer>().AddBlockChance(0.05f * Level);
		player.statLifeMax2 += 20 * Level;
	}
}

internal class MinionMovementSpeedLifeNotable : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.GetModPlayer<UniversalBuffingPlayer>().UniversalModifier.ProjectileBehaviourSpeed += 0.05f * Level;
		NotablePassiveUtils.AddMaxLifePercent(player, Level, 0.05f);
	}
}

internal class MovementSpeedLifeBlockChanceNotable : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.moveSpeed += 0.05f * Level;
		player.statLifeMax2 += 15 * Level;
		player.GetModPlayer<BlockPlayer>().AddBlockChance(0.05f * Level);
	}
}

internal class LifeManaAllResistanceNotable : Passive
{
	public override void BuffPlayer(Player player)
	{
		NotablePassiveUtils.AddMaxLifePercent(player, Level, 0.05f);
		NotablePassiveUtils.AddMaxManaPercent(player, Level, 0.05f);
		NotablePassiveUtils.AddAllResistances(player, Level, 0.03f);
	}
}

internal class FlatLifeManaDefenseNotable : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.statLifeMax2 += 30 * Level;
		player.statManaMax2 += 30 * Level;
		player.statDefense += 3 * Level;
	}
}

internal class EnergyShieldGlobalDamageNotable : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.GetModPlayer<EnergyShieldPlayer>().AddGlobalEnergyShield(30f * Level);
		player.GetDamage(DamageClass.Generic) += 0.05f * Level;
	}
}

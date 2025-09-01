using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using Terraria.ID;

namespace PathOfTerraria.Content.Passives;

internal class CloseRangePassive : Passive
{
	internal class CloseRangePlayer : ModPlayer
	{
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
		{
			float str = Player.GetModPlayer<PassiveTreePlayer>().GetCumulativeLevel(nameof(CloseRangePassive));

			if (str < 0)
			{
				return;
			}

			if (target.DistanceSQ(Player.Center) < 200 * 200)
			{
				modifiers.FinalDamage += str * 0.1f;
			}
		}
	}
}

internal class ChanceToBleedPassive : Passive
{
	internal class BleederPlayer : ModPlayer
	{
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (!hit.DamageType.CountsAsClass(DamageClass.Melee))
			{
				return;
			}

			ApplyChance(target, Player);
		}

		private static void ApplyChance(NPC npc, Player player)
		{
			float str = player.GetModPlayer<PassiveTreePlayer>().GetCumulativeLevel(nameof(ChanceToBleedPassive));

			if (str <= 0)
			{
				return;
			}

			npc.AddBuff(BuffID.Bleeding, (int)(2 + str) * 60);
		}
	}
}

internal class DamageReductionPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.endurance += 0.025f * Level;
	}
}

internal class AddedAggressionPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.aggro += 1 * Level;
	}
}

internal class DamageReflectionPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.GetModPlayer<UniversalBuffingPlayer>().UniversalModifier.ReflectedDamageModifier.Base += 10 * Level;
	}
}

internal class ArmorPenetrationPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.GetArmorPenetration(DamageClass.Melee) *= 1 + 0.15f * Level;
	}
}
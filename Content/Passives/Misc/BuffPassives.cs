using PathOfTerraria.Common.Buffs;
using PathOfTerraria.Common.Systems;
using PathOfTerraria.Common.Systems.ElementalDamage;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Content.Buffs;
using PathOfTerraria.Content.Buffs.ElementalBuffs;
using Terraria.ID;

namespace PathOfTerraria.Content.Passives;

internal class StrongerChillPassive : Passive
{
	internal class BoostChilledEffectNPC : GlobalBuff
	{
		public override void Update(int type, NPC npc, ref int buffIndex)
		{
			if (npc.HasBuff(BuffID.Chilled))
			{
				float multiplier = 1f;

				if (npc.lastInteraction != 255)
				{
					float chillPower = Main.player[npc.lastInteraction].GetModPlayer<PassiveTreePlayer>().GetCumulativeValue<StrongerChillPassive>();
					multiplier += chillPower * 0.1f;
				}

				npc.GetGlobalNPC<SlowDownNPC>().SpeedModifier += 0.1f * multiplier;
			}
		}
	}
}

internal class ChanceToChillPassive : Passive
{
	public class ChanceToChillPlayer : ModPlayer
	{
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			float str = Player.GetModPlayer<PassiveTreePlayer>().GetCumulativeValue<ChanceToChillPassive>() / 100f;

			if (Main.rand.NextFloat() < str)
			{
				target.AddBuff(BuffID.Chilled, 5 * 60);
			}
		}
	}
}

internal class ChanceToFreezePassive : Passive
{
	public class ChanceToFreezePlayer : ModPlayer
	{
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			float str = Player.GetModPlayer<PassiveTreePlayer>().GetCumulativeValue<ChanceToFreezePassive>() / 100f;

			if (Main.rand.NextFloat() < str)
			{
				ElementalPlayer.TryAddElementBuff(target, ElementType.Cold, damageDone, hit);
			}
		}
	}
}

internal class ShockChancePassive : Passive
{
	internal class ShockNPC : GlobalNPC
	{
		public override void OnHitByItem(NPC npc, Player player, Item item, NPC.HitInfo hit, int damageDone)
		{
			ApplyChance(npc, player);
		}

		public override void OnHitByProjectile(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone)
		{
			if (projectile.TryGetOwner(out Player owner))
			{
				ApplyChance(npc, owner);
			}
		}

		private static void ApplyChance(NPC npc, Player player)
		{
			float str = player.GetModPlayer<PassiveTreePlayer>().GetCumulativeValue<ShockChancePassive>();

			if (str <= 0)
			{
				return;
			}

			bool canAfflict = Main.rand.NextFloat() < str * 0.05f;

			if (canAfflict)
			{
				npc.AddBuff(ModContent.BuffType<ShockDebuff>(), 10 * 60);
			}
		}
	}
}

internal class ChanceToIgnitePassive : Passive
{
	public class ChanceToIgnoitePlayer : ModPlayer
	{
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			float str = Player.GetModPlayer<PassiveTreePlayer>().GetCumulativeValue<ChanceToIgnitePassive>() / 100f;

			if (Main.rand.NextFloat() < str)
			{
				hit.Crit = true; // Ignite doesn't proc otherwise
				ElementalPlayer.TryAddElementBuff(target, ElementType.Fire, damageDone, hit);
			}
		}
	}
}

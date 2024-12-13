using PathOfTerraria.Common.Systems;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Common.Systems.TreeSystem;
using PathOfTerraria.Content.Buffs;
using Terraria.ID;

namespace PathOfTerraria.Content.Passives;

internal class StrongerChillPassive : Passive
{
	internal class GiveChilledNPCFunctionality : GlobalBuff
	{
		public override void Update(int type, NPC npc, ref int buffIndex)
		{
			if (npc.HasBuff(BuffID.Chilled))
			{
				float multiplier = 1f;

				if (npc.lastInteraction != 255)
				{
					int chillPower = Main.player[npc.lastInteraction].GetModPlayer<PassiveTreePlayer>().GetCumulativeLevel(nameof(StrongerChillPassive));
					multiplier += chillPower * 0.1f;
				}

				npc.GetGlobalNPC<SlowDownNPC>().SlowDown += 0.1f * multiplier;
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

		private static void ApplyChance(NPC npc, Player player)
		{
			float str = player.GetModPlayer<PassiveTreePlayer>().GetCumulativeLevel(nameof(ShockChancePassive));

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

		public override void OnHitByProjectile(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone)
		{
			if (projectile.TryGetOwner(out Player owner))
			{
				ApplyChance(npc, owner);
			}
		}
	}
}

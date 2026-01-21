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
				ElementalPlayer.TryAddElementBuff(Player, target, ElementType.Cold, damageDone, new HitInfoContainer(hit, null));
			}
		}
	}
}

// Functionality is handled in ElementalDamage
internal class ShockChancePassive : Passive
{
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
				ElementalPlayer.TryAddElementBuff(Player, target, ElementType.Fire, damageDone, new HitInfoContainer(hit, null));
			}
		}
	}
}

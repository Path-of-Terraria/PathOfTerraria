using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Common.Systems.TreeSystem;
using PathOfTerraria.Core.Items;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Content.Passives;


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


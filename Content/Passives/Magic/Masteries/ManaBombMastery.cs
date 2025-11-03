using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Content.Projectiles.PassiveProjectiles;
using System.Collections.Generic;
using Terraria.DataStructures;

namespace PathOfTerraria.Content.Passives.Utility.Masteries;

internal class ManaBombMastery : Passive
{
	internal class ManaBombPlayer : ModPlayer
	{
		int manaUsed = 0;

		public override void OnConsumeMana(Item item, int manaConsumed)
		{
			if (!Player.GetModPlayer<PassiveTreePlayer>().HasNode<ManaBombMastery>())
			{
				return;
			}

			manaUsed += manaConsumed;

			while (manaUsed > 100 && Main.myPlayer == Player.whoAmI)
			{
				int damage = (int)Player.GetDamage(DamageClass.Magic).ApplyTo(Player.GetModPlayer<PassiveTreePlayer>().GetCumulativeValue<ManaBombMastery>());
				var src = new EntitySource_ItemUse(Player, item);
				int target = GetTarget(out Vector2 fallback);
				Projectile.NewProjectile(src, Player.Top, new Vector2(0, -6), ModContent.ProjectileType<ManaBomb>(), damage, 8, Player.whoAmI, target, fallback.X, fallback.Y);

				manaUsed -= 100;
			}
		}

		private int GetTarget(out Vector2 fallback)
		{
			fallback = Main.MouseWorld;
			PriorityQueue<int, float> whoAmIQueue = new();

			foreach (NPC npc in Main.ActiveNPCs)
			{
				if (npc.CanBeChasedBy() && npc.DistanceSQ(Player.Center) < 800 * 800)
				{
					whoAmIQueue.Enqueue(npc.whoAmI, Main.rand.NextFloat());
				}
			}

			return whoAmIQueue.Count == 0 ? -1 : whoAmIQueue.Dequeue();
		}
	}
}

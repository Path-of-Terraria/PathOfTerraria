using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Content.Projectiles.PassiveProjectiles;
using System.Collections.Generic;
using Terraria.DataStructures;

namespace PathOfTerraria.Content.Passives.Magic.Masteries;

internal class ManaBombMastery : Passive
{
	internal class ManaBombPlayer : ModPlayer
	{
		int manaUsed = 0;

		public override void Load()
		{
			On_Player.CheckMana_int_bool_bool += HijackCheckMana;
		}

		private static bool HijackCheckMana(On_Player.orig_CheckMana_int_bool_bool orig, Player self, int amount, bool pay, bool blockQuickMana)
		{
			bool success = orig(self, amount, pay, blockQuickMana);

			if (!self.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<ManaBombMastery>(out float value))
			{
				return success;
			}

			if (pay && success)
			{
				self.GetModPlayer<ManaBombPlayer>().manaUsed += amount;
				self.GetModPlayer<ManaBombPlayer>().CheckSpawnManaBomb(self.GetSource_FromThis(), value);
			}

			return success;
		}

		public override void OnConsumeMana(Item item, int manaConsumed)
		{
			if (!Player.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<ManaBombMastery>(out float value))
			{
				return;
			}

			manaUsed += manaConsumed;
			CheckSpawnManaBomb(new EntitySource_ItemUse(Player, item), value);
		}

		private void CheckSpawnManaBomb(IEntitySource src, float value)
		{
			while (manaUsed > 100 && Main.myPlayer == Player.whoAmI)
			{
				float manaAdd = Player.statManaMax2 * 0.025f;
				int damage = (int)Player.GetDamage(DamageClass.Magic).ApplyTo(value + manaAdd);
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

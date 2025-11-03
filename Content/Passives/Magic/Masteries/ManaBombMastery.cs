using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using Terraria.DataStructures;
using Terraria.ID;

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

			while (manaUsed > 100)
			{
				int damage = (int)Player.GetDamage(DamageClass.Magic).ApplyTo(Player.GetModPlayer<PassiveTreePlayer>().GetCumulativeValue<ManaBombMastery>());
				Projectile.NewProjectile(new EntitySource_ItemUse(Player, item), Player.Top, new Vector2(0, -6), ProjectileID.WoodenArrowFriendly, damage, 8);

				manaUsed -= 100;
			}
		}
	}
}

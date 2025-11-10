using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using Terraria.DataStructures;

namespace PathOfTerraria.Content.Passives.Magic.Masteries;

internal class ReverberationMastery : Passive
{
	internal class ReverberationPlayer : GlobalProjectile
	{
		public override void OnSpawn(Projectile projectile, IEntitySource source)
		{
			if (source is EntitySource_ItemUse_WithAmmo { Entity: Player player, Item: Item item })
			{
				if (!player.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<ReverberationMastery>(out float value) || player.channel)
				{
					return;
				}

				if (Main.rand.NextFloat() > value / 100f || !player.CheckMana(player.GetManaCost(item) / 2, true))
				{
					return;
				}

				Vector2 velocity = projectile.velocity.RotatedByRandom(0.3f) * Main.rand.NextFloat(0.9f, 1.05f);
				int damage = (int)(projectile.damage * 0.7f);
				float kb = projectile.knockBack;
				Projectile.NewProjectile(source, projectile.Center, velocity, projectile.type, damage, kb, projectile.owner, projectile.ai[0], projectile.ai[1], projectile.ai[2]);
			}
		}
	}
}

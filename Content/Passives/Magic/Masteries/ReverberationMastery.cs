using PathOfTerraria.Common.Items;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using Terraria.DataStructures;

namespace PathOfTerraria.Content.Passives.Magic.Masteries;

#nullable enable

internal class ReverberationMastery : Passive
{
	internal class ReverberationProjectile : GlobalProjectile
	{
		public override bool InstancePerEntity => true;

		/// <summary>
		/// Unsynced owner-only data used by the client for future duplication only.
		/// </summary>
		private (Item item, Player player)? _spawnedByItemUseInfo = null;

		public override void OnSpawn(Projectile projectile, IEntitySource source)
		{
			if (source is EntitySource_ItemUse_WithAmmo { Entity: Player player, Item: Item item })
			{
				_spawnedByItemUseInfo = (item, player);

				if (!CustomItemSets.VisualChannelOnly[item.type]) // Do not duplicate the visual channel projectile
				{
					ReverberationFunctionality(projectile, source, player, item);
				}
			}
			else if (source is EntitySource_Parent { Entity: Projectile parent } && parent.GetGlobalProjectile<ReverberationProjectile>()._spawnedByItemUseInfo is { } value)
			{
				ReverberationFunctionality(projectile, source, value.player, value.item);
			}
		}

		private static void ReverberationFunctionality(Projectile projectile, IEntitySource source, Player player, Item item)
		{
			bool notChanneled = player.channel && !CustomItemSets.VisualChannelOnly[item.type]; // Only spawn if this is not channeled or if this is from a visual channel item

			if (notChanneled || !player.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<ReverberationMastery>(out float value))
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
